using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;

namespace WeatherForecast
{
  public static class Constants
  {
    public static string WeatherForecastActive = "WeatherForecastActive";
    public const string Estimate = "Estimate";
    public const string Estimate10 = "Estimate10";
    public const string Estimate90 = "Estimate90";
    public static Regex DiscreteEstimateRegex => new Regex(@"(estimate_)(\d+)");

    public static string[] ContinousEstimates
    {
      get { return new[] { Estimate, Estimate10, Estimate90 }; }
    }

  }
}
