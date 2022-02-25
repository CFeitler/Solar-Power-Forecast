using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolCast
{
  public interface ISolarPowerForeCast
  {
    /// <summary>
    /// Gets the solar power forecast for a constant site.
    /// The site Id can be configure in constants.cs
    /// </summary>
    /// <param name="siteId">Id of site. Site must be created at the used SolCast account</param>
    /// <returns></returns>
    Task<List<ForecastDto>> GetSolarPowerForeCast();
  }
}
