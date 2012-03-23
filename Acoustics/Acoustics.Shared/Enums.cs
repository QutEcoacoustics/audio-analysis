namespace Acoustics.Shared
{
    using System;

    /// <summary>
    /// The enum permission.
    /// </summary>
    public enum EnumPermission
    {
        /// <summary>
        /// The manage.
        /// </summary>
        Manage = 0,

        /// <summary>
        /// The view permission.
        /// </summary>
        View = 1
    }

    /// <summary>
    /// The entity type.
    /// </summary>
    public enum EntityType
    {
        /// <summary>
        /// Incorrectly used to identify Deployments in the database. 
        /// Original purpose seems to be for identifying Entities that represent labels/text/very simple objects.
        /// </summary>
        [Obsolete("Should not be used at all. Most entities that use this should be Deployments.")]
        Label = 0,

        /// <summary>
        /// Projects are top-level containers for Sites.
        /// </summary>
        Project = 1,

        /// <summary>
        /// Sites are containers for Sensors and Deployments.
        /// </summary>
        Site = 2,

        /// <summary>
        /// Sensors represent specific pieces of equipment (Device/Hardware).
        /// </summary>
        [Obsolete("Sensors are not part of the entity structure. Use Deployment instead.")]
        Sensor = 3,

        /// <summary>
        /// Deployments are instances of sensors. 
        /// Wireless Sensors (auto/3G sensors) may have one or more deployments.
        /// Recorders should have only 1 deployment.
        /// Currently Deployments are represented by Labels.
        /// </summary>
        Deployment = 4,

        /// <summary>
        /// Jobs are processing tasks that use filters to analyse a sub-set of audio recordings.
        /// </summary>
        Job = 5,

        /// <summary>
        /// Filters are ReadingFilters in binary serialised form. 
        /// </summary>
        /// <remarks>
        /// It would be useful to be able to list the filters that include a certain deployment / hardware / audiorecording.
        /// Eg. When displaying a recording/device/deployment/project/site (etc...), get a list of filters that include the displayed item.
        /// </remarks>
        Filter = 6
    }

    /// <summary>
    /// The entity access level.
    /// </summary>
    public enum EntityAccessLevel
    {
        /// <summary>
        /// Cannot view or modify Entity.
        /// </summary>
        None = 0,

        /// <summary>
        /// Can view Entity details, but cannot modify.
        /// </summary>
        Readonly = 1,

        /// <summary>
        /// Can view and modify Entity.
        /// </summary>
        Full = 2
    }

    /// <summary>
    /// The spectrogram type.
    /// </summary>
    public enum SpectrogramType
    {
        /// <summary>
        /// The wave form.
        /// </summary>
        WaveForm = 0,

        /// <summary>
        /// The spectrogram.
        /// </summary>
        Spectrogram = 1
    }

    /// <summary>
    /// The job status.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// The ready.
        /// </summary>
        Ready = 0,

        /// <summary>
        /// The running.
        /// </summary>
        Running = 1,

        /// <summary>
        /// The complete.
        /// </summary>
        Complete = 2,

        /// <summary>
        /// The error.
        /// </summary>
        Error = 3
    }

    /// <summary>
    /// The job item result status.
    /// </summary>
    public enum JobItemResultStatus
    {
        /// <summary>
        /// Job Item was processed successfully and result has been stored as a tag. The tag may or may not be valid.
        /// </summary>
        Ready = 0,

        /// <summary>
        /// Generated tag (result) was identified as invalid or not useful.
        /// </summary>
        Invalid = 1
    }

    /// <summary>
    /// The cache job type.
    /// </summary>
    public enum CacheJobType
    {
        /// <summary>
        /// The audio segmentation.
        /// </summary>
        AudioSegmentation = 0,

        /// <summary>
        /// The spectrogram generation.
        /// </summary>
        SpectrogramGeneration = 1
    }

    /// <summary>
    /// The cache job item status.
    /// </summary>
    public enum CacheJobItemStatus
    {
        /// <summary>
        /// The ready.
        /// </summary>
        Ready = 0,

        /// <summary>
        /// The processing.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// The error.
        /// </summary>
        Error = 2,

        /// <summary>
        /// The complete.
        /// </summary>
        Complete = 3
    }

    /// <summary>
    /// The audio reading state.
    /// </summary>
    public enum AudioReadingState
    {
        /// <summary>
        /// Data is being uploaded but is incomplete.
        /// </summary>
        Uploading = 0,

        /// <summary>
        /// Audio data is being segmented and inserted in cache.
        /// </summary>
        SegmentingAudio = 1,

        /// <summary>
        /// Spectrograms are being generated and inserted in cache.
        /// </summary>
        GeneratingSpectrograms = 2,

        /// <summary>
        /// All data uploaded and caches populated, ready to display reading.
        /// </summary>
        Ready = 3,

        /// <summary>
        /// Audio data is courrupt/unreadable/useless. File should still exist.
        /// </summary>
        Corrupt = 4
    }

    /// <summary>
    /// Audio reading Data location.
    /// </summary>
    public enum AudioReadingDataLocation
    {
        /// <summary>
        /// Data is in Sql File Stream. Default data location.
        /// </summary>
        SqlFileStream = 0,

        /// <summary>
        /// Data is in a file.
        /// </summary>
        FileSystem = 1,

        /// <summary>
        /// Tried to expor from Sql FileStream to File system, but failed.
        /// </summary>
        SqlFileStreamExportFailed = 2
    }
}