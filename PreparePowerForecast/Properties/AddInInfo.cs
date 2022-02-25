using Mono.Addins;

// Declares that this assembly is an add-in
[assembly: Addin("PreparePowerForecast", "1.0")]

// Declares that this add-in depends on the scada v1.0 add-in root
[assembly: AddinDependency("::scada", "1.0")]

[assembly: AddinName("PreparePowerForecast")]
[assembly: AddinDescription("This extension prepares a zenon project for using the WeatherForecast GenericNet driver extension. Currentyl only variables are created.")] 