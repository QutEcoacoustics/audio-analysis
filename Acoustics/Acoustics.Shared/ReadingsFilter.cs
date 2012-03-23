namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;
    using System.Xml.Serialization;
    using System.Web.Script.Serialization;
    using System.Data.Linq;
#if SILVERLIGHT
	using HttpUtility = System.Windows.Browser.HttpUtility;
#else

#endif


    /*
	 * Default for DataMember EmitDefaultValue is true.
	 * Default for DataMember IsRequired is false.
	 * http://msdn.microsoft.com/en-us/library/aa347792.aspx
	 */

    /// <summary>
    /// Stores attributes used to filter/query to create a Data Set.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ReadingsFilter : IEquatable<ReadingsFilter>
    {
        private static readonly string DoubleZero = "00";
        private static readonly string Colon = ":";
        private static readonly string DateFormatString = "yyyy-MM-dd";

        public static readonly ReadingsFilter Empty = new ReadingsFilter();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadingsFilter"/> class.
        /// </summary>
        public ReadingsFilter()
        {
            this.DeploymentNames = new List<string>();
            this.AudioTags = new List<string>();
            this.ExactAudioTags = new List<string>();
            this.AudioReadingIds = new List<Guid>();
            this.EntityIds = new List<int>();
            this.JobIds = new List<int>();
            this.DeploymentIds = new List<Guid>();
            this.HeardFilters = new List<HeardFilter>();
            this.RuiJobIds = new List<int>();
        }

        #region Dates and Times

        /// <summary>
        /// Used to allow for validation when setting FromDate.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore, OptionalField, DefaultValue(null)]
        private DateTime? fromDate;

        /// <summary>
        /// Gets or sets FromDate.
        /// <para>Include audio readings taken on this date and later. Includes this date.</para>
        /// <para>Considers only date part of DateTime.</para>
        /// </summary>
        [DataMember]
        public DateTime? FromDate
        {
            get
            {
                if (this.fromDate.HasValue)
                {
                    return this.fromDate.Value.Date;
                }

                return null;
            }

            set
            {
                if (value.HasValue)
                {
                    if (!this.fromDate.HasValue)
                    {
                        this.fromDate = value.Value.Date;
                    }
                    else if (this.fromDate.Value.Date != value.Value.Date)
                    {
                        this.fromDate = value.Value.Date;
                    }
                }
            }
        }

        /// <summary>
        /// Used to allow for validation when setting FromTime.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore, OptionalField, DefaultValue(null)]
        private TimeSpan? fromTime;

        /// <summary>
        /// Gets or sets FromTime.
        /// <para>Include audio readings taken at this time of day and later. Includes this time.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Cannot represent a day or more. Change FromDate instead.</exception>
        [DataMember]
        public TimeSpan? FromTime
        {
            get
            {
                return this.fromTime;
            }

            set
            {
                if (value.HasValue && value.Value.Days > 0)
                {
                    throw new ArgumentException("Cannot represent a day or more. Change FromDate instead.");
                }

                if (value.HasValue)
                {
                    if (!this.fromTime.HasValue)
                    {
                        this.fromTime = value;
                    }
                    else if (this.fromTime.Value != value.Value)
                    {
                        this.fromTime = value;
                    }
                }
            }
        }

        /// <summary>
        /// Used to allow for validation when setting ToDate.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore, OptionalField, DefaultValue(null)]
        private DateTime? toDate;

        /// <summary>
        /// Gets or sets ToDate.
        /// <para>Include audio readings taken on or before this date. Includes this date up to ToTime.</para>
        /// <para>If ToTime is null, includes the whole day.</para>
        /// <para>Considers only date part of DateTime.</para>
        /// </summary>
        [DataMember]
        public DateTime? ToDate
        {
            get
            {
                if (this.toDate.HasValue)
                {
                    return this.toDate.Value.Date;
                }

                return null;
            }

            set
            {
                if (value.HasValue)
                {
                    if (!this.toDate.HasValue)
                    {
                        this.toDate = value.Value.Date;
                    }
                    else if (this.toDate.Value.Date != value.Value.Date)
                    {
                        this.toDate = value.Value.Date;
                    }
                }
            }
        }

        /// <summary>
        /// Used to allow for validation when setting ToTime.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore, OptionalField, DefaultValue(null)]
        private TimeSpan? toTime;

        /// <summary>
        /// Gets or sets ToTime.
        /// <para>Include audio readings taken on or before this time. Includes this time.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Cannot represent a day or more. Change ToDate instead.</exception>
        [DataMember]
        public TimeSpan? ToTime
        {
            get
            {
                return this.toTime;
            }

            set
            {
                if (value.HasValue && value.Value.Days > 0)
                {
                    throw new ArgumentException("Cannot represent a day or more. Change ToDate instead.");
                }

                if (value.HasValue)
                {
                    if (!this.toTime.HasValue)
                    {
                        this.toTime = value;
                    }
                    else if (this.toTime.Value != value.Value)
                    {
                        this.toTime = value;
                    }
                }
            }
        }

        #endregion

        #region Three State Bools

        /// <summary>
        /// Gets or sets IsRead.
        /// <para>Filter Read status.</para>
        /// <para>true: include only read readings.</para>
        /// <para>false: include only unread readings.</para>
        /// <para>null: ignore read status.</para>
        /// </summary>
        [DataMember, DefaultValue(null)]
        public bool? IsRead { get; set; }

        /// <summary>
        /// Gets or sets ActiveDeploymentsFilter.
        /// <para>Filter active deployments.</para>
        /// <para>true: include only active deployments.</para>
        /// <para>false: include only inactive deployments.</para>
        /// <para>null: ignore active status.</para>
        /// </summary>
        [DataMember, DefaultValue(true)]
        public bool? ActiveDeploymentsFilter { get; set; }

        /// <summary>
        /// Gets or sets TestDeploymentsFilter.
        /// <para>Filter test deployments</para>
        /// <para>true: incldue only test deployments</para>
        /// <para>false: include only non-test deployments.</para>
        /// <para>null: ignore test status.</para>
        /// <para>Readings with no associated deployments will ALWAYS be included.</para>
        /// </summary>
        [DataMember, DefaultValue(false)]
        public bool? TestDeploymentsFilter { get; set; }

        /// <summary>
        /// Gets or sets HasProcessorResults.
        /// <para>Filters audioreading results depending on 
        /// whether they have results from automated processing.</para>
        /// <para>true: include only audio readings with processor results.</para>
        /// <para>false: include only audio readings WITHOUT processor results.</para>
        /// <para>null: ignore processor results.</para>
        /// </summary>
        [DataMember, DefaultValue(null)]
        public bool? HasProcessorResults { get; set; }

        /// <summary>
        /// Gets or sets HasTags.
        /// <para>Filters audioreadingresults depending on whether the audioreading has one or more tags.</para>
        /// <para>true: include only audio readings with tags.</para>
        /// <para>false: include only audio readings WITHOUT tags.</para>
        /// <para>null: ignore whether audioreading has tags or not.</para>
        /// </summary>
        [DataMember, DefaultValue(null)]
        public bool? HasTags { get; set; }

        /// <summary>
        /// Gets or sets IsReadyState.
        /// <para>Filters audio readings on state (AudioReadingState).</para>
        /// <para>true: include only audio readings that are Ready.</para>
        /// <para>false: include only audio readings that are not Ready.</para>
        /// <para>null: ignore state of audio reading.</para>
        /// </summary>
        [DataMember, DefaultValue(null)]
        public bool? IsReadyState { get; set; }

        /// <summary>
        /// Gets or sets HasLength.
        /// <para>Filters audio readings on whether they have a length.</para>
        /// <para>true: include only audio readings that have a length.</para>
        /// <para>false: include only audio readings that do not have a length.</para>
        /// <para>null: ignore length value of audio reading.</para>
        /// </summary>
        [DataMember, DefaultValue(null)]
        public bool? HasLength { get; set; }

        #endregion

        #region Comma Separated

        /// <summary>
        /// Gets or sets a comma separated list of deployment names (strings).
        /// </summary>
        [DataMember]
        public string CommaSeparatedDeploymentNames
        {
            get { return DeploymentNames != null ? DeploymentNames.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) DeploymentNames = value.ParseCommaSeparatedList(); }
        }

        /// <summary> Gets or sets a comma separated list of audio tag names (strings) which are used in
        /// a conjunction to restrict tag set to tag1 AND tag2 AND tag3....
        /// </summary>
        [DataMember]
        public string CommaSeparatedAudioTags
        {
            get { return AudioTags != null ? AudioTags.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) AudioTags = value.ParseCommaSeparatedList(); }
        }

        /// <summary> Gets or sets a comma separated list of audio tag names which are used in a disjuinction to
        /// get readings satisfying tag1 OR tag2 OR tag3...
        /// </summary>
        [DataMember]
        public string CommaSeparatedExactAudioTags
        {
            // TODO: try to devise a better name for this!
            get { return ExactAudioTags != null ? ExactAudioTags.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) ExactAudioTags = value.ParseCommaSeparatedList(); }
        }

        /// <summary>
        /// Gets or sets a comma separated list of AudioReadingIds (Guids).
        /// </summary>
        [DataMember]
        public string CommaSeparatedAudioReadingIds
        {
            get { return AudioReadingIds != null ? AudioReadingIds.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) AudioReadingIds = value.ParseGuidCommaSeparatedList(); }
        }

        /// <summary>
        /// Gets or sets a comma separated list of Project, Site and Deployment EntityIds (ints).
        /// </summary>
        [DataMember]
        public string CommaSeparatedEntityIds
        {
            get { return EntityIds != null ? EntityIds.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) EntityIds = value.ParseIntCommaSeparatedList(); }
        }

        /// <summary>
        /// Gets or sets a comma separated list of job EntityIds (ints). 
        /// The AudioReadings must be discovered via: 
        ///  - the Filter used to create the Job, 
        ///  - or the AudioReadings analysed by JobItems.
        /// </summary>
        [DataMember]
        public string CommaSeparatedJobIds
        {
            get { return JobIds != null ? JobIds.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) JobIds = value.ParseIntCommaSeparatedList(); }
        }

        /// <summary>
        /// Gets or sets a comma separated list of DeploymentIds (Guids).
        /// </summary>
        [DataMember]
        public string CommaSeparatedDeploymentIds
        {
            get { return DeploymentIds != null ? DeploymentIds.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) DeploymentIds = value.ParseGuidCommaSeparatedList(); }
        }

        /// <summary>
        /// Gets or sets a comma separated list of HeardFilters (created by HeardFilters.ToString()).
        /// </summary>
        [DataMember]
        public string CommaSeparatedHeardFilters
        {
            get { return HeardFilters != null ? HeardFilters.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) HeardFilters = HeardFilter.ParseCommaSeparatedList(value).ToList(); }
        }

        #region IList Properties

        /// <summary>
        /// Gets or sets Deployment Names. Use CommaSeparatedDeploymentNames to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<string> DeploymentNames { get; set; }

        /// <summary> Gets or sets the conjunction Audio Tags. Use CommaSeparatedAudioTags to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<string> AudioTags { get; set; }

        /// <summary> Gets or sets the list of disjunction Audio Tags. Use CommaSeparatedExactAudioTags to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<string> ExactAudioTags { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingIds. Use CommaSeparatedAudioReadingIds to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<Guid> AudioReadingIds { get; set; }

        /// <summary>
        /// Gets or sets Project, Site and Deployment EntityIds. Use CommaSeparatedEntityIds to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<int> EntityIds { get; set; }

        /// <summary>
        /// Gets or sets job EntityIds. Use CommaSeparatedJobIds to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<int> JobIds { get; set; }

        /// <summary>
        /// Gets or sets DeploymentIds. Use CommaSeparatedDeploymentIds to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<Guid> DeploymentIds { get; set; }

        /// <summary>
        /// Gets or sets HeardFilters. Use CommaSeparatedHeardFilters to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<HeardFilter> HeardFilters { get; set; }

        #endregion

        #endregion

        #region Result usage instructions

        /// <summary>
        /// Gets or sets Result Usage Instrctions Job entity ids.
        /// Include specified job entity id processing results when loading filter results.
        /// </summary>
        [DataMember]
        public string RuiCommaSeparatedJobIds
        {
            get { return RuiJobIds != null ? RuiJobIds.ToCommaSeparatedList() : string.Empty; }
            set { if (!string.IsNullOrEmpty(value)) RuiJobIds = value.ParseIntCommaSeparatedList(); }
        }

        /// <summary>
        /// Gets or sets job Entity Ids to load processor results. Use RuiCommaSeparatedJobIds to set with a string.
        /// </summary>
        [IgnoreDataMember, XmlIgnore, ScriptIgnore]
        public IList<int> RuiJobIds { get; set; }

        #endregion

        #region Load and Display

        /// <summary>
        /// Gets or sets the tag id to load, display and jump to.
        /// </summary>
        [DataMember, DefaultValue(null)]
        public int? AudioTagId { get; set; }

        /// <summary>
        /// Gets or sets the Time Offset (in milliseconds) to jump to when audio reading loads.
        /// </summary>
        [DataMember, DefaultValue(null)]
        public long? TimeOffset { get; set; }

        #endregion

        /// <summary>
        /// Validates current state of filter.
        /// </summary>
        /// <returns>True indicates filter is valid, otherwise false.</returns>
        public bool Validate()
        {
            // must have both fromtime and totime or neither
            // this is due to the way ReadingsFilterManager gets results by segmenting audio.
            return (!this.FromTime.HasValue || this.ToTime.HasValue) &&
                   (this.FromTime.HasValue || !this.ToTime.HasValue);
        }

        #region querystring identifiers

        private static readonly string FromDateString = "FromDate".ToLowerInvariant();
        private static readonly string FromTimeString = "FromTime".ToLowerInvariant();
        private static readonly string ToDateString = "ToDate".ToLowerInvariant();
        private static readonly string ToTimeString = "ToTime".ToLowerInvariant();

        private static readonly string IsReadString = "IsRead".ToLowerInvariant();
        private static readonly string ActiveDeploymentsFilterString = "ActiveDeploymentsFilter".ToLowerInvariant();
        private static readonly string TestDeploymentsFilterString = "TestDeploymentsFilter".ToLowerInvariant();
        private static readonly string HasProcessorResultsString = "HasProcessorResults".ToLowerInvariant();
        private static readonly string HasTagsString = "HasTags".ToLowerInvariant();
        private static readonly string IsReadyStateString = "IsReadyState".ToLowerInvariant();
        private static readonly string HasLengthString = "HasLength".ToLowerInvariant();

        private static readonly string DeploymentNamesString = "DeploymentNames".ToLowerInvariant();
        private static readonly string AudioTagsString = "AudioTags".ToLowerInvariant();
        private static readonly string ExactAudioTagsString = "ExactAudioTags".ToLowerInvariant();
        private static readonly string AudioReadingIdsString = "AudioReadingIds".ToLowerInvariant();
        private static readonly string EntityIdsString = "EntityIds".ToLowerInvariant();
        private static readonly string JobIdsString = "JobIds".ToLowerInvariant();
        private static readonly string DeploymentIdsString = "DeploymentIds".ToLowerInvariant();

        private static readonly string RuiJobIdsString = "RuiJobIds".ToLowerInvariant();

        private static readonly string AudioTagIdString = "AudioTagId".ToLowerInvariant();
        private static readonly string TimeOffsetString = "TimeOffset".ToLowerInvariant();

        #endregion

        #region write / save

        /// <summary>
        /// Get the query string representation of this ReadingsFilter.
        /// </summary>
        /// <param name="performUrlEncoding">
        /// Perform url encoding on the querystring.
        /// </param>
        /// <returns>
        /// Query string representation of this ReadingsFilter.
        /// </returns>
        public string ToQueryString(bool performUrlEncoding)
        {
            var values = new Dictionary<string, string>();

            // dates and times
            if (FromDate.HasValue) values.Add(FromDateString, FromDate.Value.ToString(DateFormatString));
            if (FromTime.HasValue) values.Add(FromTimeString, FromTime.Value.TotalMilliseconds.ToString());
            if (ToDate.HasValue) values.Add(ToDateString, ToDate.Value.ToString(DateFormatString));
            if (ToTime.HasValue) values.Add(ToTimeString, ToTime.Value.TotalMilliseconds.ToString());

            // Three State Bools
            if (IsRead.HasValue) values.Add(IsReadString, IsRead.Value.ToString());
            if (ActiveDeploymentsFilter.HasValue) values.Add(ActiveDeploymentsFilterString, ActiveDeploymentsFilter.Value.ToString());
            if (TestDeploymentsFilter.HasValue) values.Add(TestDeploymentsFilterString, TestDeploymentsFilter.Value.ToString());
            if (HasProcessorResults.HasValue) values.Add(HasProcessorResultsString, HasProcessorResults.Value.ToString());
            if (HasTags.HasValue) values.Add(HasTagsString, HasTags.Value.ToString());
            if (this.IsReadyState.HasValue) values.Add(IsReadyStateString, this.IsReadyState.Value.ToString());
            if (this.HasLength.HasValue) values.Add(HasLengthString, this.HasLength.Value.ToString());

            // Comma Separated
            if (DeploymentNames != null && DeploymentNames.Count > 0) values.Add(DeploymentNamesString, DeploymentNames.ToCommaSeparatedList());
            if (AudioTags != null && AudioTags.Count > 0) values.Add(AudioTagsString, AudioTags.ToCommaSeparatedList());
            if (ExactAudioTags != null && ExactAudioTags.Count > 0) values.Add(ExactAudioTagsString, ExactAudioTags.ToCommaSeparatedList());
            if (AudioReadingIds != null && AudioReadingIds.Count > 0) values.Add(AudioReadingIdsString, AudioReadingIds.ToCommaSeparatedList());
            if (EntityIds != null && EntityIds.Count > 0) values.Add(EntityIdsString, EntityIds.ToCommaSeparatedList());
            if (JobIds != null && JobIds.Count > 0) values.Add(JobIdsString, JobIds.ToCommaSeparatedList());
            if (DeploymentIds != null && DeploymentIds.Count > 0) values.Add(DeploymentIdsString, DeploymentIds.ToCommaSeparatedList());

            // Results Usage Instructions
            if (RuiJobIds != null && RuiJobIds.Count > 0) values.Add(RuiJobIdsString, RuiJobIds.ToCommaSeparatedList());

            // load and display
            if (AudioTagId != null && AudioTagId.Value > 0) values.Add(AudioTagIdString, AudioTagId.Value.ToString());
            if (TimeOffset != null && TimeOffset.Value > 0) values.Add(TimeOffsetString, TimeOffset.Value.ToString());

            return values.ToUrlParameterString(performUrlEncoding);
        }

        /// <summary>
        /// Get the User Interface string representation of this ReadingsFilter.
        /// </summary>
        /// <returns>
        /// User Interface string representation of this ReadingsFilter.
        /// </returns>
        public string ToUIString()
        {
            return this.ToUIStrings().Any() ?
                this.ToUIStrings().Aggregate((a, b) => a + " and " + b) :
                string.Empty;
        }

        /// <summary>
        /// Get the individual User Interface strings representing this ReadingsFilter.
        /// </summary>
        /// <returns>
        /// Individual User Interface strings representing this ReadingsFilter.
        /// </returns>
        public IEnumerable<string> ToUIStrings()
        {
            // dates and times
            var retFrom = string.Empty;
            if (FromDate.HasValue)
            {
                retFrom += FromDate.Value.Date.ToString(DateFormatString);
            }

            if (FromTime.HasValue)
            {
                retFrom += " " +
                    FromTime.Value.Hours.ToString(DoubleZero) + Colon +
                    FromTime.Value.Minutes.ToString(DoubleZero) + Colon +
                    FromTime.Value.Seconds.ToString(DoubleZero);
            }

            if (!string.IsNullOrEmpty(retFrom))
            {
                yield return "were recorded on or after " + retFrom;
            }

            var retTo = string.Empty;
            if (ToDate.HasValue)
            {
                retTo += ToDate.Value.Date.ToString(DateFormatString);
            }

            if (ToTime.HasValue)
            {
                retTo += " " +
                    ToTime.Value.Hours.ToString(DoubleZero) + Colon +
                    ToTime.Value.Minutes.ToString(DoubleZero) + Colon +
                    ToTime.Value.Seconds.ToString(DoubleZero);
            }

            if (!string.IsNullOrEmpty(retTo)) yield return "were recorded on or before " + retTo;

            // Three State Bools
            if (IsRead.HasValue) yield return IsRead.Value ? "have been read" : "have not been read";
            if (ActiveDeploymentsFilter.HasValue) yield return ActiveDeploymentsFilter.Value ? "were recorded on active deployments" : "were recorded on inactive deployments";
            if (TestDeploymentsFilter.HasValue) yield return TestDeploymentsFilter.Value ? "were recorded on test deployments" : "were not recorded on test deployments";
            if (HasProcessorResults.HasValue) yield return HasProcessorResults.Value ? "has results from analysis runs" : "do not have results from analysis runs";
            if (HasTags.HasValue) yield return HasTags.Value ? "have tags" : "do not have tags";
            if (this.IsReadyState.HasValue) yield return this.IsReadyState.Value ? "are ready" : "are not ready";
            if (this.HasLength.HasValue) yield return this.HasLength.Value ? "have a usable value for length" : "do not have a usable value for length";

            // Comma Separated
            if (DeploymentNames != null && DeploymentNames.Count > 0) yield return "were recorded using " + string.Join(", or ", DeploymentNames.ToArray());
            if (AudioTags != null && AudioTags.Count > 0) yield return "were tagged with " + string.Join(", and ", AudioTags.ToArray());
            if (ExactAudioTags != null && ExactAudioTags.Count > 0) yield return "were tagged with " + string.Join(", or ", ExactAudioTags.ToArray());
            if (AudioReadingIds != null && AudioReadingIds.Count > 0) yield return "include recordings with ids " + CommaSeparatedAudioReadingIds;
            if (EntityIds != null && EntityIds.Count > 0) yield return "include entities with ids " + CommaSeparatedEntityIds;
            if (JobIds != null && JobIds.Count > 0) yield return "include jobs with ids " + CommaSeparatedJobIds;
            if (DeploymentIds != null && DeploymentIds.Count > 0) yield return "include deployments with ids " + CommaSeparatedDeploymentIds;
        }

#if ! SILVERLIGHT

        /// <summary>
        /// Serialise this ReadingsFilter to a byte array.
        /// </summary>
        /// <returns>
        /// This ReadingsFilter serialised to a byte array.
        /// </returns>
        public byte[] ToByteArray()
        {
            return this.BinarySerialize();
        }
        /// <summary>
        /// Serialise this ReadingsFilter to Binary.
        /// </summary>
        /// <returns>
        /// This ReadingsFilter serialised to Binary.
        /// </returns>
        public Binary ToBinary()
        {
            return this.BinarySerialize();
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            // Before serialization.
            // Prepare for serialization. For example, create optional data structures.
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            // After serialization.
            // Log serialization events.
        }
#endif

        #endregion

        #region read / load

        /// <summary>
        /// Get the ReadingsFilter object represented by the query string.
        /// </summary>
        /// <param name="filter">
        /// Query string representation of a ReadingsFilter.
        /// </param>
        /// <param name="removeUrlEncoding">
        /// Remove url encoding on the querystring.
        /// </param>
        /// <returns>
        /// ReadingsFilter object represented by the query string.
        /// </returns>
        public static ReadingsFilter FromQueryString(string filter, bool removeUrlEncoding)
        {
            var f = new ReadingsFilter();

            if (string.IsNullOrEmpty(filter)) return f;

            var values = filter.ParseUrlParameterString();

            // dates and times
            if (values.ContainsKey(FromDateString)) f.FromDate = DateTime.ParseExact(removeUrlEncoding ? HttpUtility.UrlDecode(values[FromDateString]) : values[FromDateString], DateFormatString, CultureInfo.InvariantCulture);
            if (values.ContainsKey(FromTimeString)) f.FromTime = TimeSpan.FromMilliseconds(double.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[FromTimeString]) : values[FromTimeString]));
            if (values.ContainsKey(ToDateString)) f.ToDate = DateTime.ParseExact(removeUrlEncoding ? HttpUtility.UrlDecode(values[ToDateString]) : values[ToDateString], DateFormatString, CultureInfo.InvariantCulture);
            if (values.ContainsKey(ToTimeString)) f.ToTime = TimeSpan.FromMilliseconds(double.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[ToTimeString]) : values[ToTimeString]));

            // Three State Bools
            if (values.ContainsKey(IsReadString)) f.IsRead = bool.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[IsReadString]) : values[IsReadString]);
            if (values.ContainsKey(ActiveDeploymentsFilterString)) f.ActiveDeploymentsFilter = bool.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[ActiveDeploymentsFilterString]) : values[ActiveDeploymentsFilterString]);
            if (values.ContainsKey(TestDeploymentsFilterString)) f.TestDeploymentsFilter = bool.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[TestDeploymentsFilterString]) : values[TestDeploymentsFilterString]);
            if (values.ContainsKey(HasProcessorResultsString)) f.HasProcessorResults = bool.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[HasProcessorResultsString]) : values[HasProcessorResultsString]);
            if (values.ContainsKey(HasTagsString)) f.HasTags = bool.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[HasTagsString]) : values[HasTagsString]);
            if (values.ContainsKey(IsReadyStateString)) f.IsReadyState = bool.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[IsReadyStateString]) : values[IsReadyStateString]);
            if (values.ContainsKey(HasLengthString)) f.HasLength = bool.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[HasLengthString]) : values[HasLengthString]);

            // Comma Separated
            if (values.ContainsKey(DeploymentNamesString)) f.DeploymentNames = (removeUrlEncoding ? HttpUtility.UrlDecode(values[DeploymentNamesString]) : values[DeploymentNamesString]).ParseCommaSeparatedList();
            if (values.ContainsKey(AudioTagsString)) f.AudioTags = (removeUrlEncoding ? HttpUtility.UrlDecode(values[AudioTagsString]) : values[AudioTagsString]).ParseCommaSeparatedList();
            if (values.ContainsKey(ExactAudioTagsString)) f.ExactAudioTags = (removeUrlEncoding ? HttpUtility.UrlDecode(values[ExactAudioTagsString]) : values[ExactAudioTagsString]).ParseCommaSeparatedList();
            if (values.ContainsKey(AudioReadingIdsString)) f.AudioReadingIds = (removeUrlEncoding ? HttpUtility.UrlDecode(values[AudioReadingIdsString]) : values[AudioReadingIdsString]).ParseGuidCommaSeparatedList();
            if (values.ContainsKey(EntityIdsString)) f.EntityIds = (removeUrlEncoding ? HttpUtility.UrlDecode(values[EntityIdsString]) : values[EntityIdsString]).ParseIntCommaSeparatedList();

            if (values.ContainsKey(JobIdsString)) f.JobIds = (removeUrlEncoding ? HttpUtility.UrlDecode(values[JobIdsString]) : values[JobIdsString]).ParseIntCommaSeparatedList();
            if (values.ContainsKey(DeploymentIdsString)) f.DeploymentIds = (removeUrlEncoding ? HttpUtility.UrlDecode(values[DeploymentIdsString]) : values[DeploymentIdsString]).ParseGuidCommaSeparatedList();

            // Results Usage Instructions
            if (values.ContainsKey(RuiJobIdsString)) f.RuiJobIds = (removeUrlEncoding ? HttpUtility.UrlDecode(values[RuiJobIdsString]) : values[RuiJobIdsString]).ParseIntCommaSeparatedList();

            // load and display
            if (values.ContainsKey(AudioTagIdString) && values[AudioTagIdString].IsInteger()) f.AudioTagId = int.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[AudioTagIdString]) : values[AudioTagIdString]);
            if (values.ContainsKey(TimeOffsetString) && values[TimeOffsetString].IsInteger()) f.TimeOffset = int.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[TimeOffsetString]) : values[TimeOffsetString]);

            return f;
        }

#if ! SILVERLIGHT

        /// <summary>
        /// Create a ReadingsFilter from a byte array representation.
        /// </summary>
        /// <param name="binaryFilter">
        /// The byte array representation.
        /// </param>
        /// <returns>
        /// ReadingsFilter created from the byte array.
        /// </returns>
        public static ReadingsFilter FromByteArray(byte[] binaryFilter)
        {
            return binaryFilter.BinaryDeserialize(new ReadingsFilterTypeConvertor()) as ReadingsFilter;
        }
        /// <summary>
        /// Create a ReadingsFilter from a Binary representation.
        /// </summary>
        /// <param name="binaryFilter">
        /// The Binary representation.
        /// </param>
        /// <returns>
        /// ReadingsFilter created from the Binary representation.
        /// </returns>
        public static ReadingsFilter FromBinary(Binary binaryFilter)
        {
            return binaryFilter.BinaryDeserialize(new ReadingsFilterTypeConvertor()) as ReadingsFilter;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            // Before deserialization. This callback is invoked before the deserialization constructor, if one is present.
            // Initialize default values for optional fields.
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // After deserialization.
            // Fix optional field values based on contents of other fields.
        }
#endif

        #endregion

        #region Overrides

        /// <summary>
        /// Check if two ReadingsFilters are equal.
        /// </summary>
        /// <param name="obj">
        /// Other ReadingsFilter.
        /// </param>
        /// <returns>
        /// True if ReadingsFilters are equal, otherwise false.
        /// </returns>
        public override bool Equals(object obj)
        {
            var rfObj = obj as ReadingsFilter;
            return rfObj != null && this.Equals(rfObj);
        }

        /// <summary>
        /// Get the UI string representation.
        /// </summary>
        /// <returns>
        /// UI string representation.
        /// </returns>
        public override string ToString()
        {
            return this.ToUIString();
        }

        /// <summary>
        /// Get the HashCode.
        /// </summary>
        /// <returns>
        /// ReadingsFilter HashCode.
        /// </returns>
        public override int GetHashCode()
        {
            // not expecting this to be compared.
            // only need to check for equality.
            return 357;
        }

        #endregion

        #region IEquatable<ReadingsFilter> Members

        /// <summary>
        /// Check if two ReadingsFilters are equal.
        /// </summary>
        /// <param name="other">
        /// Other ReadingsFilter.
        /// </param>
        /// <returns>
        /// True if ReadingsFilters are equal, otherwise false.
        /// </returns>
        public bool Equals(ReadingsFilter other)
        {
            return this.AudioTags.Count == other.AudioTags.Count
                && this.AudioTags.All(t => other.AudioTags.Contains(t))

                && this.ExactAudioTags.Count == other.ExactAudioTags.Count
                && this.ExactAudioTags.All(t => other.ExactAudioTags.Contains(t))

                && this.DeploymentNames.Count == other.DeploymentNames.Count
                && this.DeploymentNames.All(t => other.DeploymentNames.Contains(t))

                && this.AudioReadingIds.Count == other.AudioReadingIds.Count
                && this.AudioReadingIds.All(t => other.AudioReadingIds.Contains(t))

                && this.DeploymentIds.Count == other.DeploymentIds.Count
                && this.DeploymentIds.All(t => other.DeploymentIds.Contains(t))

                && this.EntityIds.Count == other.EntityIds.Count
                && this.EntityIds.All(t => other.EntityIds.Contains(t))

                && this.JobIds.Count == other.JobIds.Count
                && this.JobIds.All(t => other.JobIds.Contains(t))

                && this.FromDate == other.FromDate
                && this.ToDate == other.ToDate
                && this.FromTime == other.FromTime
                && this.ToTime == other.ToTime

                && this.HasProcessorResults == other.HasProcessorResults
                && this.HasTags == other.HasTags
                && this.ActiveDeploymentsFilter == other.ActiveDeploymentsFilter
                && this.IsRead == other.IsRead
                && this.TestDeploymentsFilter == other.TestDeploymentsFilter
                && this.IsReadyState == other.IsReadyState
                && this.HasLength == other.HasLength
                ;
        }

        #endregion

        /// <summary> Populates this filter with a copy of the content of another.
        /// (Support for Copy-on-write usage).
        /// </summary>
        /// <param name="filter"></param>

        public void CopyFrom(ReadingsFilter filter)
        {
            this.ActiveDeploymentsFilter = filter.ActiveDeploymentsFilter;
            this.AudioReadingIds = new List<Guid>(filter.AudioReadingIds);
            this.AudioTagId = filter.AudioTagId;
            this.AudioTags = new List<string>(filter.AudioTags);
            this.ExactAudioTags = new List<string>(filter.ExactAudioTags);
            this.DeploymentIds = new List<Guid>(filter.DeploymentIds);
            this.DeploymentNames = new List<string>(filter.DeploymentNames);
            this.EntityIds = new List<int>(filter.EntityIds);
            this.FromDate = filter.FromDate;
            this.FromTime = filter.FromTime;
            this.HasLength = filter.HasLength;
            this.HasProcessorResults = filter.HasProcessorResults;
            this.HasTags = filter.HasTags;
            this.HeardFilters = new List<HeardFilter>(filter.HeardFilters.Select(hf => hf.Clone()));
            this.IsRead = filter.IsRead;
            this.IsReadyState = filter.IsReadyState;
            this.JobIds = new List<int>(filter.JobIds);
            this.RuiJobIds = new List<int>(filter.RuiJobIds);
            this.TestDeploymentsFilter = filter.TestDeploymentsFilter;
            this.TimeOffset = filter.TimeOffset;
            this.ToDate = filter.ToDate;
            this.ToTime = filter.ToTime;
        }

        /// <summary> Creates a deep copy of the current object.
        /// </summary>
        /// <returns></returns>

        public ReadingsFilter Clone()
        {
            ReadingsFilter clone = new ReadingsFilter();
            clone.CopyFrom(this);
            return clone;
        }
    }

    /// <summary>
    /// Stores details of heard and unheard audio.
    /// </summary>
    [Serializable]
    [DataContract]
    public class HeardFilter : IEquatable<HeardFilter>, IComparable, IComparable<HeardFilter>
    {
        /// <summary>
        /// Gets or sets a value indicating whether IsHeard. Defaults to matching only unheard audio.
        /// </summary>
        [DataMember, DefaultValue(false)]
        public bool IsHeard { get; set; }

        /// <summary>
        /// Gets or sets UserId.
        /// </summary>
        [DataMember, DefaultValue(null)]
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets JobId.
        /// </summary>
        [DataMember, DefaultValue(null)]
        public int? JobId { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingId.
        /// </summary>
        [DataMember, DefaultValue(null)]
        public Guid? AudioReadingId { get; set; }

        /// <summary>
        /// Gets or sets AudioStart.
        /// </summary>
        [DataMember, DefaultValue(null)]
        public int? AudioStart { get; set; }

        /// <summary>
        /// Gets or sets AudioEnd.
        /// </summary>
        [DataMember, DefaultValue(null)]
        public int? AudioEnd { get; set; }

        #region querystring identifiers

        private static readonly string IsHeardString = "IsHeard".ToLowerInvariant();
        private static readonly string UserIdString = "UserId".ToLowerInvariant();
        private static readonly string JobIdString = "JobId".ToLowerInvariant();
        private static readonly string AudioReadingIdString = "AudioReadingId".ToLowerInvariant();
        private static readonly string AudioStartString = "AudioStart".ToLowerInvariant();
        private static readonly string AudioEndString = "AudioEnd".ToLowerInvariant();


        #endregion

        /// <summary> Get a HeardFilter from a query string representation.
        /// </summary>
        /// <param name="heardFilter">
        /// Query string representation of a HeardFilter.
        /// </param>
        /// <param name="removeUrlEncoding">
        /// Remove url encoding on the querystring.
        /// </param>
        /// <returns>
        /// HeardFilter object represented by the query string.
        /// </returns>

        public static HeardFilter FromQueryString(string heardFilter, bool removeUrlEncoding)
        {
            var f = new HeardFilter();

            if (string.IsNullOrEmpty(heardFilter)) return f;

            var values = heardFilter.ParseUrlParameterString();

            if (values.ContainsKey(UserIdString)) f.UserId = new Guid(removeUrlEncoding ? HttpUtility.UrlDecode(values[UserIdString]) : values[UserIdString]);
            if (values.ContainsKey(JobIdString)) f.JobId = int.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[JobIdString]) : values[JobIdString]);
            if (values.ContainsKey(AudioReadingIdString)) f.AudioReadingId = new Guid(removeUrlEncoding ? HttpUtility.UrlDecode(values[AudioReadingIdString]) : values[AudioReadingIdString]);
            if (values.ContainsKey(AudioStartString)) f.AudioStart = int.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[AudioStartString]) : values[AudioStartString]);
            if (values.ContainsKey(AudioEndString)) f.AudioEnd = int.Parse(removeUrlEncoding ? HttpUtility.UrlDecode(values[AudioEndString]) : values[AudioEndString]);

            return f;
        }

        /// <summary> Parse comma separated list of HeardFilters.
        /// </summary>
        /// <param name="commaSeparatedList">
        /// The comma separated list.
        /// </param>
        /// <returns>
        /// List of HeardFilters.
        /// </returns>
        public static IEnumerable<HeardFilter> ParseCommaSeparatedList(string commaSeparatedList)
        {
            return commaSeparatedList.ParseCommaSeparatedList().Select(s => FromQueryString(s, true));
        }

        /// <summary> Get the query string representation of this HeardFilter.
        /// </summary>
        /// <param name="performUrlEncoding">
        /// Perform url encoding on the querystring.
        /// </param>
        /// <returns>
        /// Query string representation of this HeardFilter.
        /// </returns>
        /// 

        public string ToQueryString(bool performUrlEncoding)
        {
            var values = new Dictionary<string, string>
                {
                    { IsHeardString, this.IsHeard.ToString() }
                };

            if (UserId.HasValue) values.Add(UserIdString, UserId.Value.ToString());
            if (JobId.HasValue) values.Add(JobIdString, JobId.Value.ToString());
            if (AudioReadingId.HasValue) values.Add(AudioReadingIdString, AudioReadingId.Value.ToString());
            if (AudioStart.HasValue) values.Add(AudioStartString, AudioStart.Value.ToString());
            if (AudioEnd.HasValue) values.Add(AudioEndString, AudioEnd.Value.ToString());

            return values.ToUrlParameterString(performUrlEncoding);
        }

        /// <summary> Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// An integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
        /// <para>Less than zero: This object is less than the <paramref name="other"/> parameter.</para>
        /// <para>Zero: This object is equal to <paramref name="other"/>.</para>
        /// <para>Greater than zero: This object is greater than <paramref name="other"/>.</para>
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>

        public int CompareTo(HeardFilter other)
        {
            // compare is heard, user, audio, start, end, job
            throw new NotImplementedException();
        }

        /// <summary> Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>

        public bool Equals(HeardFilter other)
        {
            // all should be equal
            throw new NotImplementedException();
        }

        /// <summary> Get the query string representation of this HeardFilter, performing url encoding.
        /// </summary>
        /// <returns>
        /// Query string representation of this HeardFilter.
        /// </returns>

        public override string ToString()
        {
            return this.ToQueryString(true);
        }

        /// <summary> Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="obj"/> parameter; otherwise, false.
        /// </returns>

        public override bool Equals(object obj)
        {
            var other = obj as HeardFilter;
            return other == null ? false : Equals(other);
        }

        /// <summary> Gets HashCode.
        /// </summary>
        /// <returns>
        /// Returns HashCode.
        /// </returns>

        public override int GetHashCode()
        {
            // use all properties
            return base.GetHashCode();
        }

        /// <summary> Compares the current instance with another object of the same type and 
        /// returns an integer that indicates whether the current instance precedes, 
        /// follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// An integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
        /// <para>Less than zero: This object is less than the <paramref name="obj"/> parameter.</para>
        /// <para>Zero: This object is equal to <paramref name="obj"/>.</para>
        /// <para>Greater than zero: This object is greater than <paramref name="obj"/>.</para>
        /// </returns>
        /// <param name="obj">An object to compare with this instance.</param>

        public int CompareTo(object obj)
        {
            var other = obj as HeardFilter;
            return other == null ? -1 : this.CompareTo(other);
        }

        /// <summary> Replaces the content of the current object with that of another.
        /// </summary>
        /// <param name="filter"></param>

        public void CopyFrom(HeardFilter filter)
        {
            this.AudioEnd = filter.AudioEnd;
            this.AudioReadingId = filter.AudioReadingId;
            this.AudioStart = filter.AudioStart;
            this.IsHeard = filter.IsHeard;
            this.JobId = filter.JobId;
            this.UserId = filter.UserId;
        }

        /// <summary> Creates a duplicate of the current object.
        /// </summary>
        /// <returns></returns>

        public HeardFilter Clone()
        {
            HeardFilter clone = new HeardFilter();
            clone.CopyFrom(this);
            return clone;
        }
    }

#if ! SILVERLIGHT
    /// <summary>
    /// Used to serialise and deserialise ReadingsFilter.
    /// </summary>
    public sealed class ReadingsFilterTypeConvertor : SerializationBinder
    {
        /// <summary>
        /// Ensure serialised ReadingsFilters are deserialised to the correct type.
        /// </summary>
        /// <param name="assemblyName">
        /// The assembly name.
        /// </param>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <returns>
        /// Deserialised ReadingsFilter.
        /// </returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyFullName = assembly.FullName;
            if (string.IsNullOrEmpty(assemblyFullName))
            {
                assemblyFullName = "QutSensors.Shared";
            }

            if (typeName.Contains("ReadingsFilter"))
            {
                typeName = "QutSensors.Shared.ReadingsFilter";
                assemblyName = assemblyFullName;
            }

            return Type.GetType(typeName + ", " + assemblyName);
        }
    }
#endif
}