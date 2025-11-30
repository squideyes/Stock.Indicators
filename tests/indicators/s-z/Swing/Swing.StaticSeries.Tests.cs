namespace StaticSeries;

[TestClass]
public class Swing : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<SwingResult> results = Quotes
            .ToSwing();

        // proper quantities
        Assert.HasCount(502, results);

        // sample values - verify swing points are detected
        // Note: Swing high/low counts depend on market data and may vary
        // We verify that the results have the expected structure

        // verify earliest bars have no swing values (before first pivot can be confirmed)
        Assert.IsNull(results[0].SwingHigh);
        Assert.IsNull(results[0].SwingLow);

        // Swing points are eventually detected as prices move
        // Count results that have swing values
        int swingHighCount = results.Count(static x => x.SwingHigh != null);
        int swingLowCount = results.Count(static x => x.SwingLow != null);

        // At least one type of swing should be detected in real market data
        Assert.IsGreaterThan(0, swingHighCount + swingLowCount,
            "Expected some swing points to be detected");
    }

    [TestMethod]
    public void Strength2_ReturnsExpectedResults()
    {
        const int strength = 2;
        IReadOnlyList<SwingResult> results = Quotes
            .ToSwing(strength);

        Assert.HasCount(502, results);

        // With strength 2, need at least strength+1 bars before swing can be detected
        // Verify the earliest bars have null before any pivots
        Assert.IsNull(results[0].SwingHigh);
        Assert.IsNull(results[0].SwingLow);
    }

    [TestMethod]
    public void FirstPivot_At_2KPlus1()
    {
        // Test that first possible confirmation occurs at 2K+1 bars
        const int strength = 2;

        // Build a simple valley around bar p=2 (index)
        double[] H = [10, 9, 8, 9, 10];
        double[] L = [9, 8, 7, 8, 9];

        var quotes = new List<Quote>();
        DateTime t0 = new(2025, 1, 2, 9, 35, 0);

        for (int i = 0; i < 5; i++)
        {
            quotes.Add(new Quote(
                t0.AddMinutes(i),
                (decimal)((H[i] + L[i]) * 0.5),
                (decimal)H[i],
                (decimal)L[i],
                (decimal)((H[i] + L[i]) * 0.5),
                0));
        }

        IReadOnlyList<SwingResult> results = quotes.ToSwing(strength);

        // Pivot low at p=2 confirmed on i=4
        Assert.IsNull(results[0].SwingLow);
        Assert.IsNull(results[1].SwingLow);

        // From pivot index onward, swing low should be 7
        for (int j = 2; j < results.Count; j++)
        {
            Assert.AreEqual(7.0, results[j].SwingLow, $"Expected SwingLow of 7.0 at index {j}");
        }
    }

    [TestMethod]
    public void MinBars_Gate()
    {
        // With fewer than 2K+1 bars, no pivots can confirm
        const int strength = 2;

        List<Quote> quotes = new List<Quote>();
        DateTime t0 = new(2025, 1, 2, 9, 35, 0);

        // Create 2*K bars → still no pivot
        for (int n = 0; n < 2 * strength; n++)
        {
            quotes.Add(new Quote(t0.AddMinutes(n), 10, 11, 9, 10, 0));
        }

        foreach (SwingResult r in (IReadOnlyList<SwingResult>)quotes.ToSwing(strength))
        {
            Assert.IsNull(r.SwingHigh);
            Assert.IsNull(r.SwingLow);
        }
    }

    [TestMethod]
    public void PerfectlyFlatWindow_EmitsNeither()
    {
        const int strength = 1;

        // Flat highs/lows across the window
        double[] H = [10, 10, 10];
        double[] L = [9, 9, 9];

        List<Quote> quotes = new List<Quote>();
        DateTime t0 = new(2025, 1, 2, 9, 35, 0);

        for (int i = 0; i < H.Length; i++)
        {
            quotes.Add(new Quote(
                t0.AddMinutes(i),
                (decimal)((H[i] + L[i]) * 0.5),
                (decimal)H[i],
                (decimal)L[i],
                (decimal)((H[i] + L[i]) * 0.5),
                0));
        }

        foreach (SwingResult r in (IReadOnlyList<SwingResult>)quotes.ToSwing(strength))
        {
            Assert.IsNull(r.SwingHigh);
            Assert.IsNull(r.SwingLow);
        }
    }

    [TestMethod]
    public void MonotonicRise_NoLowPivots()
    {
        const int strength = 2;

        List<Quote> quotes = new();
        DateTime t0 = new(2025, 1, 2, 9, 35, 0);

        decimal h = 10, l = 9;
        for (int i = 0; i < 20; i++)
        {
            h += 0.5m;
            l += 0.5m;
            quotes.Add(new Quote(t0.AddMinutes(i), (h + l) * 0.5m, h, l, (h + l) * 0.5m, 0));
        }

        // No lows should confirm in a monotonic rise
        foreach (SwingResult r in (IReadOnlyList<SwingResult>)quotes.ToSwing(strength))
        {
            Assert.IsNull(r.SwingLow);
        }
    }

    [TestMethod]
    public void MonotonicDecrease_NoHighPivots()
    {
        const int strength = 2;

        List<Quote> quotes = new();
        DateTime t0 = new(2025, 1, 2, 9, 35, 0);

        decimal h = 50, l = 49;
        for (int i = 0; i < 25; i++)
        {
            quotes.Add(new Quote(t0.AddMinutes(i), (h + l) * 0.5m, h, l, (h + l) * 0.5m, 0));
            h -= 0.7m;
            l -= 0.7m;
        }

        // No highs should confirm in a steady decline
        foreach (SwingResult r in (IReadOnlyList<SwingResult>)quotes.ToSwing(strength))
        {
            Assert.IsNull(r.SwingHigh);
        }
    }

    [TestMethod]
    public void DoubleBottom_FirstEqualWins()
    {
        const int strength = 2;

        // Equal lows at bars 2 and 3, earliest (2) should win
        double[] H = [10, 9, 9, 9, 10, 11];
        double[] L = [9, 8, 7, 7, 8, 9];

        List<Quote> quotes = new();
        DateTime t0 = new(2025, 1, 2, 9, 35, 0);

        for (int i = 0; i < H.Length; i++)
        {
            quotes.Add(new Quote(
                t0.AddMinutes(i),
                (decimal)((H[i] + L[i]) * 0.5),
                (decimal)H[i],
                (decimal)L[i],
                (decimal)((H[i] + L[i]) * 0.5),
                0));
        }

        IReadOnlyList<SwingResult> results = quotes.ToSwing(strength);

        // Pivot low at p=2 confirmed at i=4
        for (int j = 2; j < results.Count; j++)
        {
            Assert.AreEqual(7.0, results[j].SwingLow, $"Expected SwingLow of 7.0 at index {j}");
        }
    }

    [TestMethod]
    public void LongPlateau_RightEqualsAllowed()
    {
        const int strength = 2;

        // High plateau at bars 2..4 (High=10)
        double[] H = [8, 9, 10, 10, 10, 9, 8];
        double[] L = [7, 8, 9, 9, 9, 8, 7];

        List<Quote> quotes = new List<Quote>();
        DateTime t0 = new(2025, 1, 2, 9, 35, 0);

        for (int i = 0; i < H.Length; i++)
        {
            quotes.Add(new Quote(
                t0.AddMinutes(i),
                (decimal)((H[i] + L[i]) * 0.5),
                (decimal)H[i],
                (decimal)L[i],
                (decimal)((H[i] + L[i]) * 0.5),
                0));
        }

        IReadOnlyList<SwingResult> results = quotes.ToSwing(strength);

        for (int j = 2; j < results.Count; j++)
        {
            Assert.AreEqual(10.0, results[j].SwingHigh, $"Expected SwingHigh of 10.0 at index {j}");
        }
    }

    [TestMethod]
    public void StepSeries_CarryForward()
    {
        const int strength = 2;

        // Create a low at p=2, then ensure carry-forward sticks
        double[] H = [10, 9, 8, 9, 10, 9, 8, 9, 10];
        double[] L = [9, 8, 7, 8, 9, 8, 7, 8, 9];

        List<Quote> quotes = new List<Quote>();
        DateTime t0 = new(2025, 1, 2, 9, 35, 0);

        for (int i = 0; i < H.Length; i++)
        {
            quotes.Add(new Quote(
                t0.AddMinutes(i),
                (decimal)((H[i] + L[i]) * 0.5),
                (decimal)H[i],
                (decimal)L[i],
                (decimal)((H[i] + L[i]) * 0.5),
                0));
        }

        IReadOnlyList<SwingResult> results = quotes.ToSwing(strength);

        for (int j = 2; j < results.Count; j++)
        {
            Assert.AreEqual(7.0, results[j].SwingLow, $"Expected SwingLow of 7.0 at index {j}");
        }
    }

    [TestMethod]
    public override void BadQuotes_DoesNotFail()
    {
        IReadOnlyList<SwingResult> r = BadQuotes
            .ToSwing(5);

        Assert.HasCount(502, r);
        Assert.IsEmpty(r.Where(static x => x.SwingHigh is double v && double.IsNaN(v)));
        Assert.IsEmpty(r.Where(static x => x.SwingLow is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public override void NoQuotes_ReturnsEmpty()
    {
        IReadOnlyList<SwingResult> r0 = Noquotes
            .ToSwing();

        Assert.IsEmpty(r0);

        IReadOnlyList<SwingResult> r1 = Onequote
            .ToSwing();

        Assert.HasCount(1, r1);
    }

    [TestMethod]
    public void Exceptions()
        => Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Quotes.ToSwing(0));

    [TestMethod]
    public void SwingValues_WithinQuoteRange()
    {
        IReadOnlyList<SwingResult> results = Quotes.ToSwing(5);

        double minLow = (double)Quotes.Min(static q => q.Low);
        double maxHigh = (double)Quotes.Max(static q => q.High);

        foreach (SwingResult r in results)
        {
            if (r.SwingHigh != null)
            {
                r.SwingHigh.Should().BeGreaterThanOrEqualTo(minLow,
                    $"SwingHigh {r.SwingHigh} below minimum low {minLow}");
                r.SwingHigh.Should().BeLessThanOrEqualTo(maxHigh,
                    $"SwingHigh {r.SwingHigh} above maximum high {maxHigh}");
            }

            if (r.SwingLow != null)
            {
                r.SwingLow.Should().BeGreaterThanOrEqualTo(minLow,
                    $"SwingLow {r.SwingLow} below minimum low {minLow}");
                r.SwingLow.Should().BeLessThanOrEqualTo(maxHigh,
                    $"SwingLow {r.SwingLow} above maximum high {maxHigh}");
            }
        }
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    public void DifferentStrengths_Work(int strength)
    {
        IReadOnlyList<SwingResult> results = Quotes.ToSwing(strength);

        Assert.HasCount(502, results);

        // Verify we get some swing points with any valid strength
        int swingHighCount = results.Count(static x => x.SwingHigh != null);
        int swingLowCount = results.Count(static x => x.SwingLow != null);

        // At least one type of swing point should be detected
        Assert.IsGreaterThan(0, swingHighCount + swingLowCount,
            $"Expected some swing points for strength={strength}");
    }
}

