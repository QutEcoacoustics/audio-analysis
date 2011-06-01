namespace QutSensors.UI.Display.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using QutSensors.Data.Linq;
    using QutSensors.Shared;

    public class AudioReadingSelectInfo
    {
        public AudioReadingSelectInfo(QutSensorsDb db, AudioReading audioReading, ReadingsFilter filter)
        {
            this.AudioReadingId = audioReading.AudioReadingID;
            this.Read = audioReading.Read ? string.Empty : "Unread";
            this.State = audioReading.State == AudioReadingState.Uploading ? "NotReady" : string.Empty;
            this.DateDisplay = audioReading.Time.ToString("ddd, d MMM yyyy");
            this.TimeDisplay = audioReading.Time.ToString("HH:mm:ss");
            this.DurationDisplay = audioReading.Length.HasValue
                                       ? new TimeSpan(0, 0, 0, 0, audioReading.Length.Value).ToReadableString()
                                       : "unknown";
            this.StartDate = audioReading.Time.ToString("yyyy-MM-dd");
            this.StartTimeMs = audioReading.Time.TimeOfDay.TotalMilliseconds.ToString();
            this.DurationMs = audioReading.Length.HasValue ? audioReading.Length.Value : 0;
            this.DeploymentName = audioReading.DeploymentName;

            // tags
            if (filter.AudioTags != null && filter.AudioTags.Count > 0)
            {
                var segmentSize = TimeSpan.FromMinutes(6);
                var segments = new List<long>();

                var audioReadingTags = db.AudioTags
                .Where(t => t.AudioReadingID == audioReading.AudioReadingID && filter.AudioTags.AsEnumerable().Contains(t.Tag))
                .Select(t => new { StartTimeMs = t.StartTime, EndTimeMs = t.EndTime })
                .ToList();

                foreach (var tag in audioReadingTags)
                {
                    long startSegment =
                        Convert.ToInt64((double)tag.StartTimeMs / (double)segmentSize.TotalMilliseconds);
                    long startOffsetMs = Convert.ToInt64(startSegment * segmentSize.TotalMilliseconds);
                    if (!segments.Contains(startOffsetMs))
                    {
                        segments.Add(startOffsetMs);
                    }
                }

                this.SegmentsString = string.Join("|", segments.Select(s => s.ToString()).ToArray());
                this.SegmentMatchCountString = (segments.Count > 0) ? segments.Count + " segments match *" : string.Empty;
            }
        }

        public string Read { get; private set; }

        public string State { get; private set; }

        public Guid AudioReadingId { get; private set; }

        public string DeploymentName { get; private set; }

        public string TimeDisplay { get; private set; }

        public string DateDisplay { get; private set; }

        public long DurationMs { get; private set; }

        public string DurationDisplay { get; private set; }

        public string StartDate { get; private set; }

        public string StartTimeMs { get; private set; }

        public string SegmentsString { get; private set; }

        public string SegmentMatchCountString { get; private set; }
    }
}

/*
<ItemTemplate>
                                        <div class="ReadingSummary <%# (bool)Eval("Read") ? string.Empty : "Unread" %> <%# (AudioReadingState)Eval("State") == AudioReadingState.Uploading ? "NotReady" : string.Empty %>"
                                            id="AudioReadingItem<%# Eval("AudioReadingID") %>" audioreadingstate="<%# Eval("State") %>"
                                            audiourl="/AudioReading.ashx?ID=<%# Eval("AudioReadingID") %>&amp;Type=" imageurl="/Spectrogram.ashx?ID=<%# Eval("AudioReadingID") %>"
                                            title="<%# Eval("DeploymentName") %> 
Date: <%# DataBinder.Eval(Container.DataItem,"Time","{0:ddd, d MMM yyyy}") %> 
Time: <%# DataBinder.Eval(Container.DataItem,"Time","{0:HH:mm:ss}") %> 
Duration: <%# ((int?)DataBinder.Eval(Container.DataItem,"Length")).HasValue ?  new TimeSpan(0,0,0,0,((int)DataBinder.Eval(Container.DataItem,"Length"))).ToReadableString() : "unknown" %>"
                                            audioreadingid="<%# Eval("AudioReadingID") %>" audiostartdate="<%# DataBinder.Eval(Container.DataItem,"Time","{0:yyyy-MM-dd}") %>"
                                            starttimems="<%# ((DateTime)Eval("Time")).TimeOfDay.TotalMilliseconds %>" durationms="<%# ((int?)DataBinder.Eval(Container.DataItem,"Length")).HasValue ? ((int)DataBinder.Eval(Container.DataItem,"Length")) : 0 %>">
                                            <div onclick="PlayerMasterControlHelper.prototype.SelectReadingAndLoadFirstSubItem('AudioReadingItem<%# Eval("AudioReadingID") %>');">
                                                <span style="height: 14px; overflow: hidden; display: block; float: right;">
                                                    <%# DataBinder.Eval(Container.DataItem,"Time","{0:ddd, d MMM yyyy}") %>
                                                </span><span style="height: 14px; overflow: hidden; display: block; float: right;
                                                    clear: right;">
                                                    <%# DataBinder.Eval(Container.DataItem,"Time","{0:HH:mm:ss}") %>
                                                </span><span style="height: 28px; overflow: hidden; display: block;">
                                                    <%# Eval("DeploymentName") %>
                                                </span>
                                            </div>
                                            <div class="AudioReadingSubItems" id="AudioReadingItemTree<%# Eval("AudioReadingID") %>"
                                                style="display: none; cursor: default;">
                                            </div>
                                        </div>
                                    </ItemTemplate>
*/