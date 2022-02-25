using System;
using System.Collections.Generic;
using System.Text;

namespace SolCast
{
  /// <summary>
  /// Todo: Discuss. Should configurations be in a configuration file...
  /// </summary>
  public static class Constants
  {
    public static string ApiKey => "<your API key>";;
    public static string SiteId => "e1eb-2eb1-42f9-96e9"; //Copa-Data Headquarter
    public static string BaseUrl => "https://api.solcast.com.au/";
    public static string GetRooftopSitesForecastUrl => "rooftop_sites/{0}/forecasts";

  }
}
