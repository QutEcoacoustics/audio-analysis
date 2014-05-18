namespace Acoustics.Shared
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Linq;

    /// <summary>
    /// The audio upload info.
    /// </summary>
    [DataContract]
    public class AudioUploadInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets AudioReadingID.
        /// </summary>
        [DataMember]
        public Guid AudioReadingID { get; set; }

        /// <summary>
        /// Gets or sets DataLength.
        /// </summary>
        [DataMember]
        public long DataLength { get; set; }

        /// <summary>
        /// Gets or sets DeploymentID.
        /// </summary>
        [DataMember]
        public Guid DeploymentID { get; set; }

        /// <summary>
        /// Gets or sets HardwareID.
        /// </summary>
        [DataMember]
        public int HardwareID { get; set; }

        /// <summary>
        /// Gets or sets Length.
        /// </summary>
        [DataMember]
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets LocalReadingID.
        /// </summary>
        [DataMember]
        public int LocalReadingID { get; set; }

        /// <summary>
        /// Gets or sets MimeType.
        /// </summary>
        [DataMember]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets Time.
        /// </summary>
        [DataMember]
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UploadComplete.
        /// </summary>
        [DataMember]
        public bool UploadComplete { get; set; }

        /// <summary>
        /// Gets or sets UploadMetaData.
        /// </summary>
        [DataMember]
        public XElement UploadMetaData { get; set; }

        /// <summary>
        /// Gets or sets UploadType.
        /// </summary>
        [DataMember]
        public string UploadType { get; set; }

        /// <summary>
        /// Gets or sets UserId.
        /// </summary>
        [DataMember]
        public Guid UserId { get; set; }

        #endregion
    }
}