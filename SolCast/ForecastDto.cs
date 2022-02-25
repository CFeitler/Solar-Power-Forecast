using System;
using System.Collections.Generic;
using System.Text;

namespace SolCast
{
  /// <summary>
  /// This data transferable object is used to represent the
  /// solar power forecast for a given location. The estimate
  /// is presented in a value as well as a 10% and a 90% range.
  /// The Estimate is given for a certain time period given in
  /// a period start and end.
  /// For obvious reasons the estimate might get a blurrier the
  /// further the period is in the future.
  /// </summary>
  public class ForecastDto
  {
    public double Estimate { get; set; }
    public double Estimate10 { get; set; }
    public double Estimate90 { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

  }
}
