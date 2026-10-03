using System.Globalization;
using System.Text.RegularExpressions;
namespace PhantomFanControl.Core;

public static partial class TemperatureFormula
{
    public static double? Evaluate(string formula, IReadOnlyDictionary<string, double?> values)
    {
        var offset = 0d; var expression = formula.Trim();
        var offsetMatch = Offset().Match(expression);
        if (offsetMatch.Success) { offset = double.Parse(offsetMatch.Groups[1].Value, CultureInfo.InvariantCulture); expression = expression[..offsetMatch.Index].Trim(); }
        var match = Aggregate().Match(expression);
        if (!match.Success) return null;
        var selected = match.Groups[2].Value.Split(',', StringSplitOptions.TrimEntries).Select(n => values.GetValueOrDefault(n)).Where(v => v.HasValue).Select(v => v!.Value).ToArray();
        if (selected.Length == 0) return null;
        return (match.Groups[1].Value.Equals("MAX", StringComparison.OrdinalIgnoreCase) ? selected.Max() : selected.Average()) + offset;
    }
    [GeneratedRegex("\\s*([+-]\\s*\\d+(?:\\.\\d+)?)\\s*$", RegexOptions.IgnoreCase)] private static partial Regex Offset();
    [GeneratedRegex("^(MAX|AVERAGE)\\s*\\(([^)]*)\\)$", RegexOptions.IgnoreCase)] private static partial Regex Aggregate();
}
