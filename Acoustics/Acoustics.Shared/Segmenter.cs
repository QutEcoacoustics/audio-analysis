// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Segmenter.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Segmenter implementation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Segmenter interface.
    /// </summary>
    public interface ISegmenter
    {
        /// <summary>
        /// Create segments using a range, max and min segment size.
        /// Segments smaller than <paramref name="minimumSegmentSizeMs"/> will be combined with other segments.
        /// </summary>
        /// <param name="rangeMs">
        /// The range to segment in milliseconds within item.
        /// </param>
        /// <param name="itemDurationMs">
        /// Total duration of item being segmented.
        /// </param>
        /// <param name="segmentSizeMs">
        /// The maximum segment size in milliseconds.
        /// </param>
        /// <param name="minimumSegmentSizeMs">
        /// The minimum segment size in milliseconds.
        /// </param>
        /// <param name="wholeSegmentsOnly">
        /// If <paramref name="wholeSegmentsOnly"/> is false, segments may be smaller than <paramref name="segmentSizeMs"/>.
        /// If <paramref name="wholeSegmentsOnly"/> is true, segments will be <paramref name="segmentSizeMs"/> as much as possible.
        /// </param>
        /// <returns>
        /// List of ranges representing segments.
        /// </returns>
        IEnumerable<Range<long>> CreateSegments(Range<long> rangeMs, long itemDurationMs, long segmentSizeMs, long minimumSegmentSizeMs, bool wholeSegmentsOnly);

        /// <summary>
        /// Create segments using a range, segment site and min size.
        /// Segments smaller than <paramref name="minimumSegmentSize"/> will be combined with other segments.
        /// </summary>
        /// <param name="range">
        /// The range.
        /// </param>
        /// <param name="itemDuration">
        /// Duration of item being segmented.
        /// </param>
        /// <param name="segmentSize">
        /// The segment size.
        /// </param>
        /// <param name="minimumSegmentSize">
        /// The minimum segment size.
        /// </param>
        /// <param name="wholeSegmentsOnly">
        /// If <paramref name="wholeSegmentsOnly"/> is false, segments may be smaller than <paramref name="segmentSize"/>.
        /// If <paramref name="wholeSegmentsOnly"/> is true, segments will be <paramref name="segmentSize"/> as much as possible.
        /// </param>
        /// <returns>
        /// List of ranges representing segments.
        /// </returns>
        IEnumerable<Range<TimeSpan>> CreateSegments(Range<TimeSpan> range, TimeSpan itemDuration, TimeSpan segmentSize, TimeSpan minimumSegmentSize, bool wholeSegmentsOnly);

        /// <summary>
        /// Create segments based on from and to dates/times.
        /// </summary>
        /// <param name="startDateTime">
        /// The overall start datetime.
        /// </param>
        /// <param name="duration">
        /// The overall duration.
        /// </param>
        /// <param name="fromTime">
        /// The from time.
        /// </param>
        /// <param name="fromDate">
        /// The from date.
        /// </param>
        /// <param name="toTime">
        /// The to time.
        /// </param>
        /// <param name="toDate">
        /// The to date.
        /// </param>
        /// <returns>
        /// Segment start/end datetimes.
        /// </returns>
        IEnumerable<Range<DateTime>> CreateTimeSegments(DateTime startDateTime, TimeSpan duration, TimeSpan? fromTime, DateTime? fromDate, TimeSpan? toTime, DateTime? toDate);

        /// <summary>
        /// Create segments using a segment site and min size. Resulting segments will together account for entire <paramref name="itemDuration"/>.
        /// Segments smaller than <paramref name="minimumSegmentSize"/> will be combined with other segments.
        /// </summary>
        /// <param name="itemDuration">
        /// Duration of item being segmented.
        /// </param>
        /// <param name="targetSegmentSize">
        /// The target Segment Size.
        /// </param>
        /// <param name="minimumSegmentSize">
        /// The minimum segment size.
        /// </param>
        /// <returns>
        /// List of ranges representing segments.
        /// </returns>
        IEnumerable<Range<TimeSpan>> CreateSegments(TimeSpan itemDuration, TimeSpan targetSegmentSize, TimeSpan minimumSegmentSize);
    }

    /// <summary>
    /// Segmenter implementation.
    /// </summary>
    public class Segmenter : ISegmenter
    {
        /// <summary>
        /// Create segments using a range, max and min segment size.
        /// Segments smaller than <paramref name="minimumSegmentSizeMs"/> will be combined with other segments.
        /// </summary>
        /// <param name="rangeMs">
        /// The range to segment in milliseconds within item.
        /// </param>
        /// <param name="itemDurationMs">
        /// Total duration of item being segmented.
        /// </param>
        /// <param name="segmentSizeMs">
        /// The maximum segment size in milliseconds.
        /// </param>
        /// <param name="minimumSegmentSizeMs">
        /// The minimum segment size in milliseconds.
        /// </param>
        /// <param name="wholeSegmentsOnly">
        /// If <paramref name="wholeSegmentsOnly"/> is false, segments may be smaller than <paramref name="segmentSizeMs"/>.
        /// If <paramref name="wholeSegmentsOnly"/> is true, segments will be <paramref name="segmentSizeMs"/> as much as possible.
        /// </param>
        /// <returns>
        /// List of ranges representing segments.
        /// </returns>
        public IEnumerable<Range<long>> CreateSegments(Range<long> rangeMs, long itemDurationMs, long segmentSizeMs, long minimumSegmentSizeMs, bool wholeSegmentsOnly)
        {
            /*
             * Procedure:
             *  - calculate first and last segments.
             *  - if these segments share a boundary, return them as ranges depending on 'wholeSegmentsOnly'.
             *  - of they don't share a bondary, get the ranges in between.
             *  - return in between segments as ranges, plus first and last segments depending on 'wholeSegmentsOnly'.
             */

            // need valid values to continue.
            if (rangeMs == null || rangeMs.Maximum == rangeMs.Minimum || itemDurationMs < 1 || segmentSizeMs < 1 || minimumSegmentSizeMs < 1)
            {
                // make this easy for consumers - don't return null.
                return new List<Range<long>>();
            }

            // correct min range. This could throw exceptions if preferred.
            if (rangeMs.Minimum < 0)
            {
                rangeMs.Minimum = 0;
            }

            // correct max range. This could throw exceptions if preferred.
            if (rangeMs.Maximum > itemDurationMs)
            {
                rangeMs.Maximum = itemDurationMs;
            }

            // segment size cannot be larger than duration.
            if (segmentSizeMs > itemDurationMs)
            {
                segmentSizeMs = itemDurationMs;
            }

            // min segment size cannot be larger than segment size.
            if (minimumSegmentSizeMs > segmentSizeMs)
            {
                minimumSegmentSizeMs = segmentSizeMs;
            }

            var ranges =
                (wholeSegmentsOnly
                     ? CreateSegmentsFirstLastWhole(rangeMs, segmentSizeMs, itemDurationMs)
                     : CreateSegmentsFirstLastExact(rangeMs, segmentSizeMs)).
                    ToList();

            if (ranges.Count() < 2)
            {
                return ranges;
            }

            // check if first and last are equal.
            if (ranges.First() == ranges.Last())
            {
                return new List<Range<long>>
                    {
                        ranges.First()
                    };
            }

            // if first and last share boundaries.
            if (ranges.First().Maximum == ranges.Last().Minimum)
            {
                // combine if either is less than min size.
                if (ranges.First().Maximum - ranges.First().Minimum < minimumSegmentSizeMs ||
                    ranges.Last().Maximum - ranges.Last().Minimum < minimumSegmentSizeMs)
                {
                    return new List<Range<long>>
                        {
                            new Range<long>
                                {
                                    Minimum = ranges.First().Minimum, 
                                    Maximum = ranges.Last().Maximum
                                }
                        };
                }

                return ranges;
            }

            // if first and last do not share boundaries.
            var betweenDuration = ranges.Last().Minimum - ranges.First().Maximum;
            var betweenRangeCount = betweenDuration / segmentSizeMs;

            for (var index = 0; index < betweenRangeCount; index++)
            {
                var min = ranges.First().Maximum + (segmentSizeMs * index);
                var max = ranges.First().Maximum + (segmentSizeMs * (index + 1));
                ranges.Insert(index + 1, new Range<long> { Minimum = min, Maximum = max });
            }

            // pre- or ap-pend if less than min size
            var firstRange = ranges.First();
            var lastRange = ranges.Last();

            // combine if first segment is less than min size.
            if (firstRange.Maximum - firstRange.Minimum < minimumSegmentSizeMs && ranges.Count > 1)
            {
                // remove first
                ranges.RemoveAt(0);

                // modify new first
                ranges[0].Minimum = firstRange.Minimum;
            }

            // combine if last segment is less than min size.
            if (lastRange.Maximum - lastRange.Minimum < minimumSegmentSizeMs && ranges.Count > 1)
            {
                // remove last
                ranges.RemoveAt(ranges.Count - 1);

                // modify new last
                ranges[ranges.Count - 1].Maximum = lastRange.Maximum;
            }

            return ranges;
        }

        /// <summary>
        /// Create segments using a range, segment site and min size.
        /// Segments smaller than <paramref name="minimumSegmentSize"/> will be combined with other segments.
        /// </summary>
        /// <param name="range">
        /// The range.
        /// </param>
        /// <param name="itemDuration">
        /// Duration of item being segmented.
        /// </param>
        /// <param name="segmentSize">
        /// The segment size.
        /// </param>
        /// <param name="minimumSegmentSize">
        /// The minimum segment size.
        /// </param>
        /// <param name="wholeSegmentsOnly">
        /// If <paramref name="wholeSegmentsOnly"/> is false, segments may be smaller than <paramref name="segmentSize"/>.
        /// If <paramref name="wholeSegmentsOnly"/> is true, segments will be <paramref name="segmentSize"/> as much as possible.
        /// </param>
        /// <returns>
        /// List of ranges representing segments.
        /// </returns>
        public IEnumerable<Range<TimeSpan>> CreateSegments(Range<TimeSpan> range, TimeSpan itemDuration, TimeSpan segmentSize, TimeSpan minimumSegmentSize, bool wholeSegmentsOnly)
        {
            var rangeMs = new Range<long>
            {
                Minimum = (long)range.Minimum.TotalMilliseconds,
                Maximum = (long)range.Maximum.TotalMilliseconds
            };

            var itemDurationMs = (long)itemDuration.TotalMilliseconds;
            var segmentSizeMs = (long)segmentSize.TotalMilliseconds;
            var minimumSegmentSizeMs = (long)minimumSegmentSize.TotalMilliseconds;

            var segmentsMs = CreateSegments(rangeMs, itemDurationMs, segmentSizeMs, minimumSegmentSizeMs, wholeSegmentsOnly);

            var segments = segmentsMs.Select(s => new Range<TimeSpan>
            {
                Maximum = TimeSpan.FromMilliseconds(s.Maximum),
                Minimum = TimeSpan.FromMilliseconds(s.Minimum)
            });

            return segments;
        }

        /// <summary>
        /// Create segments based on from and to dates/times.
        /// </summary>
        /// <param name="startDateTime">
        /// The overall start datetime.
        /// </param>
        /// <param name="duration">
        /// The overall duration.
        /// </param>
        /// <param name="fromTime">
        /// The from time.
        /// </param>
        /// <param name="fromDate">
        /// The from date.
        /// </param>
        /// <param name="toTime">
        /// The to time.
        /// </param>
        /// <param name="toDate">
        /// The to date.
        /// </param>
        /// <returns>
        /// Segment start/end datetimes.
        /// </returns>
        public IEnumerable<Range<DateTime>> CreateTimeSegments(
            DateTime startDateTime,
            TimeSpan duration,
            TimeSpan? fromTime,
            DateTime? fromDate,
            TimeSpan? toTime,
            DateTime? toDate
        )
        {
            if (startDateTime == DateTime.MaxValue || startDateTime == DateTime.MinValue)
            {
                throw new ArgumentOutOfRangeException("startDateTime", "Parameter 'startDateTime' must be a valid value.");
            }


            /*
            // FilterResults are the segments of audioreadings 
            // that match the date and time restrictions in the filter.

            // dates and times should be greater than or equal to fromTime/Date (>=) !(<)
            // but less than toTime/Date (<) !(>=)
            */

            TimeSpan? filterToTime = toTime;
            TimeSpan? filterFromTime = fromTime;
            if (filterToTime.HasValue && filterFromTime.HasValue && filterToTime.Value == filterFromTime.Value)
            {
                // if from and to times are equal, it is as if they weren't set.
                // The ReadingsFilter retains the set times, however.
                filterToTime = null;
                filterFromTime = null;
            }

            double readingDuration = duration.TotalMilliseconds;

            /**************
             * Times
             **************/

            DateTime overallBeginDate = startDateTime;
            if (filterFromTime.HasValue && filterToTime.HasValue)
            {
                if (filterFromTime.Value < filterToTime.Value)
                {
                    // begin from larger of readings start TimeOfDay and filterFromTime
                    var timeOfDayBegin = overallBeginDate.TimeOfDay > filterFromTime.Value ?
                        overallBeginDate.TimeOfDay : filterFromTime.Value;

                    // if time of day is larger than totime, need to go to the next day
                    overallBeginDate = timeOfDayBegin > filterToTime.Value ?
                        overallBeginDate.Date.AddDays(1).Add(filterFromTime.Value) :
                        overallBeginDate.Date.Add(timeOfDayBegin);
                }
                else
                {
                    // if start of reading is outside segment, move to next segment
                    if (overallBeginDate.TimeOfDay >= filterToTime.Value &&
                        overallBeginDate.TimeOfDay < filterFromTime.Value)
                    {
                        overallBeginDate = overallBeginDate.Date.Add(filterFromTime.Value);
                    }
                }
            }

            DateTime overallEndDate = startDateTime.AddMilliseconds(readingDuration);
            if (filterFromTime.HasValue && filterToTime.HasValue)
            {
                var timeOfDayEnd = new TimeSpan(24, 0, 0); // end of the day

                if (filterFromTime.Value < filterToTime.Value)
                {
                    // end at smaller of readings end TimeOfDay and filterToTime
                    timeOfDayEnd = overallEndDate.TimeOfDay < filterToTime.Value ?
                        overallEndDate.TimeOfDay : filterToTime.Value;
                }
                else
                {
                    // end at smaller of readings end TimeOfDay and end of the day
                    timeOfDayEnd = overallEndDate.TimeOfDay < timeOfDayEnd ?
                        overallEndDate.TimeOfDay : timeOfDayEnd;
                }

                overallEndDate = overallEndDate.Date.Add(timeOfDayEnd);
            }

            /**************
             * Dates
             **************/

            DateTime? modifiedToDate = toDate.HasValue ? toDate.Value.AddDays(1) : new DateTime?();

            var minStartDate = overallBeginDate;
            if (fromDate.HasValue) minStartDate = minStartDate > fromDate.Value ? minStartDate : fromDate.Value;
            if (modifiedToDate.HasValue) minStartDate = minStartDate < modifiedToDate.Value ? minStartDate : modifiedToDate.Value;

            overallBeginDate = minStartDate;

            var maxEndDate = overallEndDate;
            if (fromDate.HasValue) maxEndDate = maxEndDate > fromDate.Value ? maxEndDate : fromDate.Value;
            if (modifiedToDate.HasValue) maxEndDate = maxEndDate < modifiedToDate.Value ? maxEndDate : modifiedToDate.Value;

            overallEndDate = maxEndDate;

            DateTime currentDate = overallBeginDate;

            while (currentDate < overallEndDate)
            {
                var segmentStart = currentDate;
                var segmentEnd = overallEndDate;

                if (filterFromTime.HasValue && filterToTime.HasValue)
                {
                    // find start of next segment
                    var segmentStartTime = TimeToSegmentBoundary(currentDate, filterFromTime.Value, filterToTime.Value);
                    segmentStart = currentDate.Add(segmentStartTime);
                    currentDate = currentDate.Add(segmentStartTime);

                    // find end of segment
                    var segmentEndTime = TimeRemainingInSegment(currentDate, filterFromTime.Value, filterToTime.Value);
                    segmentEnd = segmentStart.Add(segmentEndTime);
                    currentDate = currentDate.Add(segmentEndTime);
                }
                else
                {
                    currentDate = overallEndDate;
                }

                if (currentDate > overallEndDate)
                {
                    currentDate = overallEndDate;
                }

                if (segmentStart >= overallEndDate) break;
                if (segmentEnd > overallEndDate) segmentEnd = overallEndDate;

                if (fromDate.HasValue && segmentEnd < fromDate.Value) break;
                if (fromDate.HasValue && segmentStart < fromDate.Value) segmentStart = fromDate.Value;

                if (modifiedToDate.HasValue && segmentStart >= modifiedToDate.Value) break;
                if (modifiedToDate.HasValue && segmentEnd > modifiedToDate.Value) segmentEnd = modifiedToDate.Value;

                var timeToSegmentStart = segmentStart - startDateTime;
                var timeToSegmentEnd = segmentEnd - startDateTime;

                yield return new Range<DateTime>
                        {
                            Minimum = startDateTime.Add(timeToSegmentStart),
                            Maximum = startDateTime.Add(timeToSegmentEnd)
                        };
            }
        }

        /// <summary>
        /// Create segments using a segment site and min size. Resulting segments will together account for entire <paramref name="itemDuration"/>.
        /// Segments smaller than <paramref name="minimumSegmentSize"/> will be combined with other segments.
        /// </summary>
        /// <param name="itemDuration">
        /// Duration of item being segmented.
        /// </param>
        /// <param name="targetSegmentSize">
        /// The target Segment Size.
        /// </param>
        /// <param name="minimumSegmentSize">
        /// The minimum segment size.
        /// </param>
        /// <returns>
        /// List of ranges representing segments.
        /// </returns>
        public IEnumerable<Range<TimeSpan>> CreateSegments(TimeSpan itemDuration, TimeSpan targetSegmentSize, TimeSpan minimumSegmentSize)
        {
            var rangeMs = new Range<long>
            {
                Minimum = 0,
                Maximum = (long)itemDuration.TotalMilliseconds
            };

            var itemDurationMs = (long)itemDuration.TotalMilliseconds;
            var segmentSizeMs = (long)targetSegmentSize.TotalMilliseconds;
            var minimumSegmentSizeMs = (long)minimumSegmentSize.TotalMilliseconds;

            var segmentsMs = CreateSegments(rangeMs, itemDurationMs, segmentSizeMs, minimumSegmentSizeMs, false);

            var segments = segmentsMs.Select(s => new Range<TimeSpan>
            {
                Maximum = TimeSpan.FromMilliseconds(s.Maximum),
                Minimum = TimeSpan.FromMilliseconds(s.Minimum)
            });

            return segments;
        }

        /// <summary>
        /// Create first and last whole segments.
        /// </summary>
        /// <param name="rangeMs">Range in milliseconds.</param>
        /// <param name="segmentSizeMs">Segment size in milliseconds.</param>
        /// <param name="itemDurationMs">Item duration in milliseconds.</param>
        /// <returns>First and last segments.</returns>
        private static IEnumerable<Range<long>> CreateSegmentsFirstLastWhole(Range<long> rangeMs, long segmentSizeMs, long itemDurationMs)
        {
            // calculate boundaries.
            var firstSegmentStart = (rangeMs.Minimum / segmentSizeMs) * segmentSizeMs;
            var firstSegmentEnd = (long)Math.Ceiling((double)rangeMs.Minimum / (double)segmentSizeMs) * segmentSizeMs;

            var lastSegmentStart = (rangeMs.Maximum / segmentSizeMs) * segmentSizeMs;
            var lastSegmentEnd = (long)Math.Ceiling((double)rangeMs.Maximum / (double)segmentSizeMs) * segmentSizeMs;

            lastSegmentEnd = Math.Min(lastSegmentEnd, itemDurationMs);

            var ranges = new List<Range<long>>
                    {
                        new Range<long>
                            {
                                Minimum = firstSegmentStart,
                                Maximum = firstSegmentEnd
                            },
                        new Range<long>
                            {
                                Minimum = lastSegmentStart,
                                Maximum = lastSegmentEnd
                            },
                    };

            return ranges;
        }

        /// <summary>
        /// Create first and last exact segments.
        /// </summary>
        /// <param name="rangeMs">Range in milliseconds.</param>
        /// <param name="segmentSizeMs">Segment size in milliseconds.</param>
        /// <returns>First and last segments.</returns>
        private static IEnumerable<Range<long>> CreateSegmentsFirstLastExact(Range<long> rangeMs, long segmentSizeMs)
        {
            // calculate boundaries.
            var firstSegmentEnd = (long)Math.Ceiling((double)rangeMs.Minimum / (double)segmentSizeMs) * segmentSizeMs;
            var lastSegmentStart = (rangeMs.Maximum / segmentSizeMs) * segmentSizeMs;

            var ranges = new List<Range<long>>
                    {
                        new Range<long>
                            {
                                Minimum = rangeMs.Minimum,
                                Maximum = firstSegmentEnd
                            },
                        new Range<long>
                            {
                                Minimum = lastSegmentStart,
                                Maximum = rangeMs.Maximum
                            },
                    };

            return ranges;
        }

        /// <summary>
        /// Get the time to the next segment boundary.
        /// </summary>
        /// <param name="startDateTime">Date and time to begin looking.</param>
        /// <param name="fromTime">Time segment starts.</param>
        /// <param name="toTime">Time segment ends.</param>
        /// <returns>Amount of time from <paramref name="startDateTime"/> next segment boundary occurs.</returns>
        /// <exception cref="ArgumentException"><c>ArgumentException</c>.</exception>
        private static TimeSpan TimeToSegmentBoundary(DateTime startDateTime, TimeSpan fromTime, TimeSpan toTime)
        {
            if (fromTime < toTime)
            {
                if (startDateTime.TimeOfDay < fromTime)
                {
                    // outside segment, segment starts at fromTime
                    // time of day is earlier than from time
                    return fromTime - startDateTime.TimeOfDay;
                }

                if (startDateTime.TimeOfDay >= fromTime && startDateTime.TimeOfDay < toTime)
                {
                    // inside segment, segment starts immediately
                    // time of day is equal or after from time, but before to time
                    return TimeSpan.Zero;
                }

                if (startDateTime.TimeOfDay >= toTime)
                {
                    // outside segment, segment starts next day
                    // after to time, no segment for this day
                    return startDateTime.Date.AddDays(1).AddMilliseconds(fromTime.TotalMilliseconds) - startDateTime;
                }
            }
            else if (fromTime > toTime)
            {
                if (startDateTime.TimeOfDay < toTime)
                {
                    // inside segment, segment starts immediately
                    // segment spans days, still inside segment that started yesterday.
                    return TimeSpan.Zero;
                }

                if (startDateTime.TimeOfDay >= toTime && startDateTime.TimeOfDay < fromTime)
                {
                    // outside segment, segment starts at fromTime
                    // between end of one segment and start of next
                    return fromTime - startDateTime.TimeOfDay;
                }

                if (startDateTime.TimeOfDay >= fromTime)
                {
                    // inside segment, segment starts immediately
                    // inside second segment of the day
                    return TimeSpan.Zero;
                }
            }

            throw new ArgumentException("Could not calculate time to segment boundary. Using startDateTime=" + startDateTime + ". fromTime=" + fromTime + ". toTime=" + toTime + ".");
        }

        /// <summary>
        /// The time remaining in segment.
        /// </summary>
        /// <param name="startDateTime">
        /// The start Date Time.
        /// </param>
        /// <param name="fromTime">
        /// The from Time.
        /// </param>
        /// <param name="toTime">
        /// The to Time.
        /// </param>
        /// <exception cref="ArgumentException">
        /// startDateTime is not inside a segment OR could not calculate remaining time.
        /// </exception>
        /// <returns>
        /// Time remaining in segment.
        /// </returns>
        private static TimeSpan TimeRemainingInSegment(DateTime startDateTime, TimeSpan fromTime, TimeSpan toTime)
        {
            if (startDateTime.TimeOfDay < fromTime && startDateTime.TimeOfDay >= toTime)
            {
                // startDateTime must be inside segment
                throw new ArgumentException("startDateTime is not inside a segment.");
            }

            if (startDateTime.TimeOfDay < toTime)
            {
                return toTime - startDateTime.TimeOfDay;
            }

            if (fromTime >= toTime && startDateTime.TimeOfDay >= fromTime && startDateTime.TimeOfDay >= toTime)
            {
                // goes into next day, until toTime
                return startDateTime.Date.AddDays(1).AddMilliseconds(toTime.TotalMilliseconds) - startDateTime;
            }

            throw new ArgumentException("Could not calculate time remaining in segment. Using startDateTime=" + startDateTime + ". fromTime=" + fromTime + ". toTime=" + toTime + ".");
        }
    }
}