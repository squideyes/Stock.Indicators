using System.Globalization;

#nullable enable

namespace Baseline;

/// <summary>
/// Baseline regression tests for the Swing indicator using fixed tick data.
/// These tests ensure the indicator produces consistent results across library changes.
/// </summary>
[TestClass]
public class SwingBaselineTests : TestBase
{
    private static readonly Lazy<IReadOnlyList<Quote>> EsCandlesLazy = new(() =>
        LoadAndAggregateTicks("s-z/Swing/KB_ES_20240108_JTH_ET.csv"));

    private static readonly Lazy<IReadOnlyList<Quote>> EuCandlesLazy = new(() =>
        LoadAndAggregateTicks("s-z/Swing/KB_EU_20240108_JTH_ET.csv"));

    private static IReadOnlyList<Quote> EsCandles => EsCandlesLazy.Value;
    private static IReadOnlyList<Quote> EuCandles => EuCandlesLazy.Value;

    #region ES Baseline Tests

    [TestMethod]
    public void ES_Baseline_Strength5_SwingHighs()
    {
        // Arrange
        const int strength = 5;
        IReadOnlyList<SwingResult> results = EsCandles.ToSwing(strength);

        // Assert structure
        Assert.HasCount(600, EsCandles);
        Assert.HasCount(600, results);

        // Get all swing high transitions (where value changes)
        var swingHighs = GetSwingTransitions(results, r => r.SwingHigh);

        // Validate each swing high value
        ValidateSwingHighs_ES_Strength5(swingHighs);
    }

    [TestMethod]
    public void ES_Baseline_Strength5_SwingLows()
    {
        // Arrange
        const int strength = 5;
        IReadOnlyList<SwingResult> results = EsCandles.ToSwing(strength);

        // Get all swing low transitions
        var swingLows = GetSwingTransitions(results, r => r.SwingLow);

        // Validate each swing low value
        ValidateSwingLows_ES_Strength5(swingLows);
    }

    #endregion

    #region EU Baseline Tests

    [TestMethod]
    public void EU_Baseline_Strength5_SwingHighs()
    {
        // Arrange
        const int strength = 5;
        IReadOnlyList<SwingResult> results = EuCandles.ToSwing(strength);

        // Assert structure
        Assert.HasCount(599, EuCandles);
        Assert.HasCount(599, results);

        // Get all swing high transitions
        var swingHighs = GetSwingTransitions(results, r => r.SwingHigh);

        // Validate each swing high value
        ValidateSwingHighs_EU_Strength5(swingHighs);
    }

    [TestMethod]
    public void EU_Baseline_Strength5_SwingLows()
    {
        // Arrange
        const int strength = 5;
        IReadOnlyList<SwingResult> results = EuCandles.ToSwing(strength);

        // Get all swing low transitions
        var swingLows = GetSwingTransitions(results, r => r.SwingLow);

        // Validate each swing low value
        ValidateSwingLows_EU_Strength5(swingLows);
    }

    #endregion

    #region Swing Value Extraction

    /// <summary>
    /// Gets all swing transitions (index where value first appears or changes).
    /// </summary>
    private static List<(int Index, double Value)> GetSwingTransitions(
        IReadOnlyList<SwingResult> results,
        Func<SwingResult, double?> selector)
    {
        var transitions = new List<(int Index, double Value)>();
        double? lastValue = null;

        for (int i = 0; i < results.Count; i++)
        {
            double? currentValue = selector(results[i]);
            if (currentValue != null && currentValue != lastValue)
            {
                transitions.Add((i, currentValue.Value));
                lastValue = currentValue;
            }
        }

        return transitions;
    }

    #endregion

    #region ES Strength5 Baseline Values

    private static void ValidateSwingHighs_ES_Strength5(List<(int Index, double Value)> actual)
    {
        // Expected swing high transitions (Index, Value)
        // Generated from current implementation - DO NOT MODIFY unless algorithm changes
        (int Index, double Value)[] expected =
        [
            (16, 4733),
            (45, 4734.75),
            (79, 4732.25),
            (92, 4737),
            (114, 4739.5),
            (123, 4739),
            (190, 4748.25),
            (198, 4748.5),
            (215, 4754.5),
            (244, 4759.5),
            (260, 4762.75),
            (278, 4759.75),
            (290, 4761.25),
            (308, 4761.75),
            (316, 4763),
            (333, 4767.75),
            (345, 4766.75),
            (371, 4768.75),
            (384, 4771.5),
            (397, 4772.5),
            (415, 4771.75),
            (424, 4770.5),
            (437, 4774.25),
            (449, 4776.5),
            (465, 4782.5),
            (483, 4789.75),
            (508, 4796),
            (529, 4795.25),
            (550, 4800.75),
            (569, 4803.5),
            (583, 4800.5),
            (592, 4801),
        ];

        Assert.HasCount(expected.Length, actual);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i].Index, actual[i].Index, $"ES SwingHigh[{i}] index mismatch");
            Assert.AreEqual(expected[i].Value, actual[i].Value, $"ES SwingHigh[{i}] value mismatch");
        }
    }

    private static void ValidateSwingLows_ES_Strength5(List<(int Index, double Value)> actual)
    {
        (int Index, double Value)[] expected =
        [
            (25, 4730),
            (46, 4732.75),
            (52, 4731.75),
            (61, 4730.25),
            (67, 4729.5),
            (83, 4729.25),
            (100, 4732.5),
            (119, 4736),
            (126, 4736.25),
            (139, 4734.25),
            (148, 4733),
            (158, 4732.75),
            (172, 4736),
            (193, 4739.75),
            (204, 4740.75),
            (224, 4748.5),
            (248, 4756),
            (274, 4755.5),
            (282, 4754.5),
            (293, 4757),
            (310, 4758.5),
            (319, 4759.75),
            (327, 4759.5),
            (344, 4764),
            (353, 4760.5),
            (388, 4768.25),
            (401, 4767.5),
            (419, 4768.25),
            (427, 4768.75),
            (455, 4774.25),
            (472, 4779.25),
            (511, 4790.75),
            (523, 4789.5),
            (540, 4791.5),
            (559, 4791.75),
            (577, 4799.5),
        ];

        Assert.HasCount(expected.Length, actual);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i].Index, actual[i].Index, $"ES SwingLow[{i}] index mismatch");
            Assert.AreEqual(expected[i].Value, actual[i].Value, $"ES SwingLow[{i}] value mismatch");
        }
    }

    #endregion

    #region EU Strength5 Baseline Values

    private static void ValidateSwingHighs_EU_Strength5(List<(int Index, double Value)> actual)
    {
        (int Index, double Value)[] expected =
        [
            (26, 1.09715),
            (39, 1.09795),
            (80, 1.09785),
            (93, 1.0981),
            (113, 1.09805),
            (129, 1.09875),
            (164, 1.099),
            (173, 1.0991),
            (185, 1.0994),
            (192, 1.09925),
            (228, 1.10025),
            (239, 1.1003),
            (263, 1.1006),
            (270, 1.10055),
            (283, 1.101),
            (291, 1.10085),
            (315, 1.101),
            (333, 1.10105),
            (368, 1.09985),
            (384, 1.0998),
            (396, 1.0997),
            (437, 1.09855),
            (450, 1.0989),
            (460, 1.099),
            (466, 1.09905),
            (477, 1.09915),
            (484, 1.09925),
            (491, 1.0993),
            (498, 1.0994),
            (548, 1.0988),
            (561, 1.09875),
            (568, 1.09885),
            (585, 1.09875),
        ];

        Assert.HasCount(expected.Length, actual);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i].Index, actual[i].Index, $"EU SwingHigh[{i}] index mismatch");
            Assert.AreEqual(expected[i].Value, actual[i].Value, 0.000001, $"EU SwingHigh[{i}] value mismatch");
        }
    }

    private static void ValidateSwingLows_EU_Strength5(List<(int Index, double Value)> actual)
    {
        (int Index, double Value)[] expected =
        [
            (17, 1.0968),
            (24, 1.09675),
            (48, 1.09735),
            (64, 1.0968),
            (86, 1.0974),
            (103, 1.09735),
            (110, 1.09745),
            (137, 1.09785),
            (157, 1.098),
            (166, 1.0985),
            (188, 1.09855),
            (206, 1.09845),
            (213, 1.0986),
            (234, 1.09925),
            (247, 1.09955),
            (268, 1.0999),
            (287, 1.1003),
            (297, 1.1002),
            (309, 1.10035),
            (322, 1.10025),
            (364, 1.09945),
            (376, 1.09935),
            (392, 1.0993),
            (408, 1.099),
            (423, 1.0983),
            (431, 1.098),
            (442, 1.0983),
            (455, 1.09835),
            (468, 1.0988),
            (489, 1.09895),
            (525, 1.09805),
            (558, 1.0982),
            (574, 1.09845),
            (582, 1.09855),
        ];

        Assert.HasCount(expected.Length, actual);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i].Index, actual[i].Index, $"EU SwingLow[{i}] index mismatch");
            Assert.AreEqual(expected[i].Value, actual[i].Value, 0.000001, $"EU SwingLow[{i}] value mismatch");
        }
    }

    #endregion



    #region Data Loading Utilities

    /// <summary>
    /// Loads tick data from CSV and aggregates into 1-minute candles.
    /// </summary>
    private static IReadOnlyList<Quote> LoadAndAggregateTicks(string relativePath)
    {
        var ticks = File.ReadAllLines(relativePath)
            .Select(ParseTickLine)
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .OrderBy(t => t.Timestamp)
            .ToList();

        return AggregateToOneMinuteCandles(ticks);
    }

    /// <summary>
    /// Parses a tick line from the CSV file.
    /// Format: Date,Time,Price,Low,High,Volume (e.g., "20240108,063000233,4727.5,4727.5,4727.75,1")
    /// </summary>
    private static (DateTime Timestamp, decimal Open, decimal Low, decimal High, decimal Volume)? ParseTickLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        string[] parts = line.Split(',');
        if (parts.Length < 6)
        {
            return null;
        }

        // Parse date: YYYYMMDD
        if (!int.TryParse(parts[0], out int dateInt))
        {
            return null;
        }

        int year = dateInt / 10000;
        int month = (dateInt / 100) % 100;
        int day = dateInt % 100;

        // Parse time: HHMMSSMMM (e.g., 063000233 = 06:30:00.233)
        if (!int.TryParse(parts[1], out int timeInt))
        {
            return null;
        }

        int hour = timeInt / 10000000;
        int minute = (timeInt / 100000) % 100;
        int second = (timeInt / 1000) % 100;
        int millisecond = timeInt % 1000;

        DateTime timestamp = new(year, month, day, hour, minute, second, millisecond);

        // Parse price values
        if (!decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) ||
            !decimal.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal low) ||
            !decimal.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal high) ||
            !decimal.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal volume))
        {
            return null;
        }

        return (timestamp, price, low, high, volume);
    }

    /// <summary>
    /// Aggregates tick data into 1-minute OHLCV candles.
    /// </summary>
    private static IReadOnlyList<Quote> AggregateToOneMinuteCandles(
        List<(DateTime Timestamp, decimal Open, decimal Low, decimal High, decimal Volume)> ticks)
    {
        var candles = new List<Quote>();

        var grouped = ticks
            .GroupBy(t => new DateTime(t.Timestamp.Year, t.Timestamp.Month, t.Timestamp.Day,
                                       t.Timestamp.Hour, t.Timestamp.Minute, 0))
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            var ticksInMinute = group.ToList();
            if (ticksInMinute.Count == 0)
            {
                continue;
            }

            decimal open = ticksInMinute[0].Open;
            decimal close = ticksInMinute[^1].Open;
            decimal high = ticksInMinute.Max(t => t.High);
            decimal low = ticksInMinute.Min(t => t.Low);
            decimal volume = ticksInMinute.Sum(t => t.Volume);

            candles.Add(new Quote(group.Key, open, high, low, close, volume));
        }

        return candles;
    }

    #endregion
}

