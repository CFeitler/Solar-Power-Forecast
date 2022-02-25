using Scada.AddIn.Contracts;
using System;

namespace PreparePowerForecast
{

  [AddInExtension(name:"Power Forecast Preparation",
    description: "This extension prepares a zenon project for using the WeatherForecast GenericNet driver extension. GenericNetDriver needs to be named 'SolcastWeatherForecast'. Currently only variables are created.", 
    category:"Weather Forecast")]
  public class EngineeringStudioWizardExtension : IEditorWizardExtension
  {
    #region poor mans configuration
    //create a variable for each 30 minutes of power forecast for 7 days = 7 days * 24 hrs * 2 variables per hour = 336
    private readonly int variablesAmount = 336;
    private readonly string  variablePrefix = "estimate";
    private readonly string driverIdentification = "SolcastWeatherForecast";
    private readonly string histogrammScreenName = "Histogram";
    private readonly string elementPrefix = "rectangle";
    #endregion

    #region IEditorWizardExtension implementation

    public void Run(IEditorApplication context, IBehavior behavior)
    {
      var project = context.Workspace?.ActiveProject;
      if (project == null) return;
      var driver = project.DriverCollection[driverIdentification];
      if (driver == null) return;
      var dataType = project.DataTypeCollection["REAL"];
      if (dataType == null) return;
      var screen = project.ScreenCollection[histogrammScreenName];
      if (screen == null) return;

      for (int i = 0; i < variablesAmount; i++)
      {
        var variableName = $"{variablePrefix}_{i}";
        var variable = project.VariableCollection[variableName];
        if (variable == null)
        {
          variable = project.VariableCollection.Create(variableName,
            driver,
            Scada.AddIn.Contracts.Variable.ChannelType.PlcMarker,
            dataType);
        }
        variable.SetDynamicProperty("SymbAddr", variableName);

        var barGraph = screen.ScreenElementCollection.Create($"{elementPrefix}_{variableName}", Scada.AddIn.Contracts.ScreenElement.ElementType.BarDisplay);
        //configuration of bar graph
        barGraph.SetDynamicProperty("StartY", 200);
        barGraph.SetDynamicProperty("Height", 500);
        var barGraphMargin = 20;
        var barGraphWidth = 5;
        barGraph.SetDynamicProperty("StartX", (barGraphMargin + (i* barGraphWidth)));
        barGraph.SetDynamicProperty("Width", barGraphWidth);
        barGraph.SetDynamicProperty("Variable", variableName);
        barGraph.SetDynamicProperty("Transparent", true);
        barGraph.SetDynamicProperty("ShowScale", false);
        barGraph.SetDynamicProperty("ViewMin", 0);
        barGraph.SetDynamicProperty("ViewMax", 55);
      }
    }

    #endregion
  }

}