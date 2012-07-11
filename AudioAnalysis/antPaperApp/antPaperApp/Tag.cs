using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace antPaperApp
{
    using System.Diagnostics.Contracts;

    public class TagModel
    {
        public string SpeciesName { get; set; }


        public string AudioTagID { get; set; }

        public string AudioReadingID { get; set; }

        private string tag;

        private DateTime? _properDate;

        /// <summary>
        /// Gets or sets Tag.
        /// </summary>
        public string Tag
        {
            get
            {
                if (tag == null)
                {
                    return string.Empty;
                }
                return this.tag;
            }
            set
            {
                this.tag = value;
            }
        }

        public DateTime StartDate { get; set; }

        public TimeSpan StartTime { get; set; }

        [LINQtoCSV.CsvColumn(OutputFormat = "O")]
        public DateTime ProperDate
        {
            get
            {
                if (_properDate.HasValue)
                {
                    return _properDate.Value;
                }
                return StartDate + StartTime;
            }
            set
            {
                _properDate = value;
                StartDate = _properDate.Value.Date;
                StartTime = _properDate.Value.TimeOfDay;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(StartDate + StartTime == ProperDate);

        }

        public string EndDate { get; set; }

        public string EndTime { get; set; }

        public string RelativeStartMs { get; set; }

        public string StartFrequency { get; set; }

        public string EndFrequency { get; set; }

        public string OldCreatedBy { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedTime { get; set; }

        public string ReferenceTag { get; set; }

        public string IsProcessorTag { get; set; }

        public string Project { get; set; }

        public string Site { get; set; }

        public string hardwareId { get; set; }

        public string url { get; set; }
    }
}
