namespace Skender.Stock.Indicators;

public static partial class Swing
{
    /// <summary>
    /// Validates the Swing indicator parameters.
    /// </summary>
    internal static void Validate(int strength)
    {
        if (strength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(strength),
                strength,
                "Strength must be greater than or equal to 1.");
        }
    }

    /// <summary>
    /// Calculates swing high and swing low points.
    /// </summary>
    /// <typeparam name="TQuote">Type implementing IQuote.</typeparam>
    /// <param name="quotes">Historical price quotes.</param>
    /// <param name="strength">Number of bars on each side to confirm a swing point.</param>
    /// <returns>Time series of SwingResult values.</returns>
    public static IReadOnlyList<SwingResult> ToSwing<TQuote>(
        this IEnumerable<TQuote> quotes,
        int strength = 5)
        where TQuote : IQuote
    {
        // validate parameters
        Validate(strength);

        // convert to list for indexing
        List<TQuote> quotesList = quotes.ToList();
        int length = quotesList.Count;

        // initialize
        List<SwingResult> results = new(length);
        double? currentSwingHigh = null;
        double? currentSwingLow = null;

        // arrays for backfilling
        double?[] swingHighs = new double?[length];
        double?[] swingLows = new double?[length];

        // roll through quotes
        for (int i = 0; i < length; i++)
        {
            TQuote q = quotesList[i];

            // initialize with carried forward values
            swingHighs[i] = currentSwingHigh;
            swingLows[i] = currentSwingLow;

            // need at least 2 * strength bars to determine a swing
            if (i < 2 * strength)
            {
                continue;
            }

            // check pivot point at index (i - strength)
            int pivotIndex = i - strength;

            bool isSwingHigh = IsSwingHigh(quotesList, pivotIndex, strength);
            bool isSwingLow = IsSwingLow(quotesList, pivotIndex, strength);

            // handle case where both swing high and low at same bar
            if (isSwingHigh && isSwingLow)
            {
                if (IsFlatWindow(quotesList, pivotIndex, strength))
                {
                    continue;
                }

                double high = (double)quotesList[pivotIndex].High;
                double low = (double)quotesList[pivotIndex].Low;

                currentSwingHigh = high;
                currentSwingLow = low;

                for (int j = pivotIndex; j <= i; j++)
                {
                    swingHighs[j] = high;
                    swingLows[j] = low;
                }
            }
            else if (isSwingHigh)
            {
                double high = (double)quotesList[pivotIndex].High;
                currentSwingHigh = high;

                for (int j = pivotIndex; j <= i; j++)
                {
                    swingHighs[j] = high;
                }
            }
            else if (isSwingLow)
            {
                double low = (double)quotesList[pivotIndex].Low;
                currentSwingLow = low;

                for (int j = pivotIndex; j <= i; j++)
                {
                    swingLows[j] = low;
                }
            }
        }

        // build results
        for (int i = 0; i < length; i++)
        {
            results.Add(new SwingResult(quotesList[i].Timestamp) {
                SwingHigh = swingHighs[i],
                SwingLow = swingLows[i]
            });
        }

        return results;
    }

    private static bool IsSwingHigh<TQuote>(List<TQuote> quotes, int pivotIndex, int strength)
        where TQuote : IQuote
    {
        decimal pivot = quotes[pivotIndex].High;

        for (int i = pivotIndex - strength; i < pivotIndex; i++)
        {
            if (i < 0)
                continue;

            if (quotes[i].High >= pivot)
                return false;
        }

        for (int i = pivotIndex + 1; i <= pivotIndex + strength; i++)
        {
            if (i >= quotes.Count)
                return false;

            if (quotes[i].High > pivot)
                return false;
        }

        return true;
    }

    private static bool IsSwingLow<TQuote>(List<TQuote> quotes, int pivotIndex, int strength)
        where TQuote : IQuote
    {
        decimal pivot = quotes[pivotIndex].Low;

        for (int i = pivotIndex - strength; i < pivotIndex; i++)
        {
            if (i < 0)
                continue;

            if (quotes[i].Low <= pivot)
                return false;
        }

        for (int i = pivotIndex + 1; i <= pivotIndex + strength; i++)
        {
            if (i >= quotes.Count)
                return false;

            if (quotes[i].Low < pivot)
                return false;
        }

        return true;
    }

    private static bool IsFlatWindow<TQuote>(List<TQuote> quotes, int pivotIndex, int strength)
        where TQuote : IQuote
    {
        decimal high = quotes[pivotIndex].High;
        decimal low = quotes[pivotIndex].Low;

        for (int i = pivotIndex - strength; i <= pivotIndex + strength; i++)
        {
            if (i < 0 || i >= quotes.Count)
                continue;

            if (quotes[i].High != high || quotes[i].Low != low)
                return false;
        }

        return true;
    }
}
