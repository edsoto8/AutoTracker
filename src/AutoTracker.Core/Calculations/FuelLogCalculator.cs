using AutoTracker.Core.Models;

namespace AutoTracker.Core.Calculations;

public static class FuelLogCalculator
{
    /// <summary>
    /// Orders logs by date then odometer and populates computed values.
    /// The first entry per vehicle has null computed values (displayed as N/A).
    /// </summary>
    public static IReadOnlyList<FuelLog> Calculate(IEnumerable<FuelLog> logs)
    {
        var ordered = logs
            .OrderBy(l => l.Date)
            .ThenBy(l => l.Odometer)
            .ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            if (i == 0)
            {
                ordered[i].MilesSinceLastFillup = null;
                ordered[i].Mpg = null;
                ordered[i].CostPerMile = null;
                continue;
            }

            var miles = ordered[i].Odometer - ordered[i - 1].Odometer;
            ordered[i].MilesSinceLastFillup = miles;
            ordered[i].Mpg = ordered[i].Gallons > 0 ? Math.Round((decimal)miles / ordered[i].Gallons, 2) : null;
            ordered[i].CostPerMile = miles > 0 ? Math.Round(ordered[i].TotalCost / miles, 4) : null;
        }

        return ordered;
    }
}
