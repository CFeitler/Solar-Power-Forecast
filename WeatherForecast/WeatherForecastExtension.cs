using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CopaData.Drivers.Contracts;
using SolCast;

namespace WeatherForecast
{
  public class WeatherForecastExtension : IDriverExtension
  {
    private ILogger _logger;
    private IValueCallback _valueCallback;
    private bool _isWeatherForecastActive;
    private Dictionary<string, object> _subscriptions;
    private ISolarPowerForeCast _forecast; //initialise only once as otherwise it would query the REST api multiple times.

    public Task InitializeAsync(ILogger logger, IValueCallback valueCallback, string configFilePath)
    {
      _logger = logger;
      _valueCallback = valueCallback;
      _subscriptions = new Dictionary<string, object>();
      _forecast = new SolCast.SolCast(_logger);
      return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
      _subscriptions.Clear();
      return Task.CompletedTask;
    }

    public Task<bool> SubscribeAsync(string symbolicAddress)
    {
      if (_subscriptions.ContainsKey(symbolicAddress))
      {
        _logger.DeepDebug($"'{symbolicAddress}' is already subscribed");
      }
      else
      {
        _logger.DeepDebug($"Added '{symbolicAddress}' to subscriptions");
        _subscriptions.Add(symbolicAddress, null);
        if (symbolicAddress == Constants.WeatherForecastActive)
        {
          _subscriptions[Constants.WeatherForecastActive] = (double)1.0;
        }
      }
      return Task.FromResult(true);
    }

    public Task UnsubscribeAsync(string symbolicAddress)
    {
      _logger.DeepDebug($"Unsubscribe '{symbolicAddress}'");

      _subscriptions.Remove(symbolicAddress);
      return Task.CompletedTask;
    }

    public Task ReadAllAsync()
    {
      var forecasts = new List<ForecastDto>();
      if (_subscriptions.ContainsKey(Constants.WeatherForecastActive)
        && (double)_subscriptions[Constants.WeatherForecastActive] >= 0)
      {
        forecasts = _forecast.GetSolarPowerForeCast().Result;
      }

      foreach (var variable in _subscriptions)
      {
        //This is for setting all the values (at once) to the (3) variables for continous
        //estimates. The values are set with the according timestamp. zenon treats the
        //changed timestamp as value change and logs it in the archive which is set to
        //log on value change (won't work with cyclical archives)
        //The values in these archives can be used to display in an extended trend.
        //drawback of this method: multiple values exist for the same timestamp if
        //the forecast changes over time. The extended trend - zenon - can not distinguish
        //which value was the latest one or is the correct one.
        if (Constants.ContinousEstimates.Contains(variable.Key))
        {
          _logger.DeepDebug($"Writing values for '{variable.Key}' from forecast");
          foreach (var forecast in forecasts)
          {
            double value = 0.0;
            switch (variable.Key)
            {
              case Constants.Estimate:
                value = forecast.Estimate;
                break;
              case Constants.Estimate10:
                value = forecast.Estimate10;
                break;
              case Constants.Estimate90:
                value = forecast.Estimate90;
                break;
            }
            _valueCallback.SetValue(variable.Key, value, forecast.PeriodStart, StatusBits.Spontaneous);
          }
          continue;
        }

        //This is for setting values to a discrete forcast.
        //The variables have to follow a certain naming: estimate_0 is for the
        //current estimate. estimate_1 is for the estimate in 30 minutes
        //estimate_2 is for the estimate in one hour and so on. As the forecast
        //is provided for the next 7 days there could be 7 days * 24 hrs * 2 variables per hour = 336
        //variables. There variables can be created in zenon with the PreparePowerForecast Engineering
        //Studio Extension in this very same solution (don't overanalyze it - it's a poor mans implementation
        //var now = new DateTime(2022, 01, 04, 14, 30, 0); //for test use: 2022-01-04T14:30:00.0000000Z
        var now = System.DateTime.Now;                    //for productive use: System.DateTime.Now;
        if (Constants.DiscreteEstimateRegex.IsMatch(variable.Key))
        {
          int distance;
          try
          {
            var distanceString = Constants.DiscreteEstimateRegex.Match(variable.Key)
              .Groups
              .Values
              .Last()
              .Value;
            distance = int.Parse(distanceString);
          }
          catch (Exception)
          {
            //do nothing and continue
            _logger.DeepDebug("something happened when trying to get the time distance to now from variable name (variable for discrete extimate)");
            continue;
          }
          var timeForVariable = now + TimeSpan.FromMinutes(distance * 30);
          timeForVariable = RoundUp(timeForVariable, TimeSpan.FromMinutes(30));
          var valueForVariable = forecasts.Where(f => f.PeriodEnd.Equals(timeForVariable)).FirstOrDefault()?.Estimate;
          if(valueForVariable == null)
          {
            valueForVariable = 0.0;
          }
          _valueCallback.SetValue(variable.Key,(double)valueForVariable,DateTime.Now,StatusBits.Spontaneous);
        }




        if (variable.Value != null)
        {
          if (variable.Value is string stringValue)
          {
            _logger.DeepDebug($"read value of string variable'{variable.Key}'");
            _valueCallback.SetValue(variable.Key, stringValue, StatusBits.Spontaneous);
          }
          else if (variable.Value is double value)
          {
            _logger.DeepDebug($"read value of double variable '{variable.Key}'");
            _valueCallback.SetValue(variable.Key, value, StatusBits.Spontaneous);
          }
        }
      }
      return Task.CompletedTask;
    }

    public Task<bool> WriteStringAsync(string symbolicAddress, string value, DateTime dateTime, StatusBits statusBits)
    {
      _logger.DeepDebug($"WriteString '{symbolicAddress}'");

      _subscriptions[symbolicAddress] = value;
      return Task.FromResult(true);
    }

    public Task<bool> WriteNumericAsync(string symbolicAddress, double value, DateTime dateTime, StatusBits statusBits)
    {
      _logger.DeepDebug($"WriteNumeric '{symbolicAddress}'");

      _subscriptions[symbolicAddress] = value;
      return Task.FromResult(true);
    }

    private DateTime RoundUp(DateTime dt, TimeSpan d)
    {
      return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
    }
  }
}
