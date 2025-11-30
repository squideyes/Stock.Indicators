---
title: Swing Points
description: Swing Points identifies local extrema (swing highs and swing lows) in price data, carrying forward the most recent confirmed swing values for trend analysis and support/resistance identification.
permalink: /indicators/Swing/
image: /assets/charts/Swing.png
type: price-pattern
layout: indicator
---

# {{ page.title }}

Swing Points identifies local extrema (swing highs and swing lows) in price data.  A swing high occurs when a bar's high is higher than the highs of the surrounding bars within the strength window.  A swing low occurs when a bar's low is lower than the lows of the surrounding bars.  Once confirmed, swing values are carried forward until a new swing point is detected.
[[Discuss] &#128172;]({{site.github.repository_url}}/discussions "Community discussion about this indicator")

![chart for {{page.title}}]({{site.baseurl}}{{page.image}})

```csharp
// C# usage syntax
IReadOnlyList<SwingResult> results =
  quotes.ToSwing(strength);
```

## Parameters

**`strength`** _`int`_ - Number of bars on each side required to confirm a swing point (`S`).  Must be at least 1.  Default is 5.

The total evaluation window size is `2×S+1`, representing `±S` from the pivot point.

### Historical quotes requirements

You must have at least `2×S+1` periods of `quotes` to cover the warmup periods; however, more is typically provided since this is a chartable pattern.

`quotes` is a collection of generic `TQuote` historical price quotes.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide]({{site.baseurl}}/guide/#historical-quotes) for more information.

## Response

```csharp
IReadOnlyList<SwingResult>
```

- This method returns a time series of all available indicator values for the `quotes` provided.
- It always returns the same number of elements as there are in the historical quotes.
- It does not return a single incremental indicator value.
- The first `2×S` periods will have `null` values since there's not enough data to confirm a swing point.

> &#128073; **Repaint warning**: this price pattern looks forward and backward in the historical quotes so it will never identify a swing point until `S` periods after the pivot.  Swing points are retroactively identified and then carried forward.

### SwingResult

**`Timestamp`** _`DateTime`_ - Date from evaluated `TQuote`

**`SwingHigh`** _`double`_ - Most recent confirmed swing high value; otherwise `null` is returned.

**`SwingLow`** _`double`_ - Most recent confirmed swing low value; otherwise `null` is returned.

### Utilities

- [.Condense()]({{site.baseurl}}/utilities#condense)
- [.Find(lookupDate)]({{site.baseurl}}/utilities#find-indicator-result-by-date)
- [.RemoveWarmupPeriods(qty)]({{site.baseurl}}/utilities#remove-warmup-periods)

See [Utilities and helpers]({{site.baseurl}}/utilities#utilities-for-indicator-results) for more information.

## Chaining

This indicator is not chain-enabled and must be generated from `quotes`.  It **cannot** be used for further processing by other chain-enabled indicators.

