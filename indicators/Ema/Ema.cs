﻿using System;
using System.Collections.Generic;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // EXPONENTIAL MOVING AVERAGE
        public static IEnumerable<EmaResult> GetEma<TQuote>(
            IEnumerable<TQuote> history,
            int lookbackPeriod)
            where TQuote : IQuote
        {

            // convert history to basic format
            List<BasicData> bdList = Cleaners.ConvertHistoryToBasic(history, "C");

            // calculate
            return CalcEma(bdList, lookbackPeriod);
        }


        private static IEnumerable<EmaResult> CalcEma(
            List<BasicData> bdList, int lookbackPeriod)
        {

            // check parameter arguments
            ValidateEma(bdList, lookbackPeriod);

            // initialize
            List<EmaResult> results = new List<EmaResult>(bdList.Count);

            decimal k = 2 / (decimal)(lookbackPeriod + 1);
            decimal lastEma = 0;

            for (int i = 0; i < lookbackPeriod; i++)
            {
                lastEma += bdList[i].Value;
            }
            lastEma /= lookbackPeriod;

            // roll through history
            for (int i = 0; i < bdList.Count; i++)
            {
                BasicData h = bdList[i];
                int index = i + 1;

                EmaResult result = new EmaResult
                {
                    Date = h.Date
                };

                if (index > lookbackPeriod)
                {
                    result.Ema = lastEma + k * (h.Value - lastEma);
                    lastEma = (decimal)result.Ema;
                }
                else if (index == lookbackPeriod)
                {
                    result.Ema = lastEma;
                }

                results.Add(result);
            }

            return results;
        }


        private static void ValidateEma(
            List<BasicData> history,
            int lookbackPeriod)
        {

            // check parameter arguments
            if (lookbackPeriod <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lookbackPeriod), lookbackPeriod,
                    "Lookback period must be greater than 0 for EMA.");
            }

            // check history
            int qtyHistory = history.Count;
            int minHistory = Math.Max(2 * lookbackPeriod, lookbackPeriod + 100);
            if (qtyHistory < minHistory)
            {
                string message = "Insufficient history provided for EMA.  " +
                    string.Format(englishCulture,
                    "You provided {0} periods of history when at least {1} is required.  "
                    + "Since this uses a smoothing technique, for a lookback period of {2}, "
                    + "we recommend you use at least {3} data points prior to the intended "
                    + "usage date for better precision.",
                    qtyHistory, minHistory, lookbackPeriod, lookbackPeriod + 250);

                throw new BadHistoryException(nameof(history), message);
            }
        }

    }
}