namespace Skender.Stock.Indicators;

/// <summary>
/// Result for Swing indicator containing swing high and low values.
/// </summary>
[Serializable]
public sealed class SwingResult : IReusable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwingResult"/> class.
    /// </summary>
    /// <param name="timestamp">The date/time of the result.</param>
    public SwingResult(DateTime timestamp)
    {
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets the timestamp for this result.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets or sets the most recent confirmed swing high value.
    /// </summary>
    public double? SwingHigh { get; set; }

    /// <summary>
    /// Gets or sets the most recent confirmed swing low value.
    /// </summary>
    public double? SwingLow { get; set; }

    /// <inheritdoc/>
    public double Value => SwingHigh ?? double.NaN;
}
