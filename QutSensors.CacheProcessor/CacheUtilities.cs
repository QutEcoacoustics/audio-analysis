// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheUtilities.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Cache utilities.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.CacheProcessor
{
    using System.Configuration;
    using System.IO;
    using System.Linq;

    using AudioTools;

    using QutSensors.Data;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;

    /// <summary>
    /// Cache utilities.
    /// </summary>
    public static class CacheUtilities
    {
        /// <summary>
        /// Get a segment from an mp3 file.
        /// </summary>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// Byte array of audio segment. Byte array will be null or 0 length if segmentation failed.
        /// </returns>
        public static byte[] SegmentMp3(string audioFile, CacheRequest request)
        {
            try
            {
                const string Mp3SpltPathKey = "PathToMp3Splt";

                var pathToMp3Split = ConfigurationManager.AppSettings.AllKeys.Contains(Mp3SpltPathKey)
                                         ? ConfigurationManager.AppSettings[Mp3SpltPathKey]
                                         : string.Empty;

                const string ConversionfolderKey = "ConversionFolder";

                var conversionPath = ConfigurationManager.AppSettings.AllKeys.Contains(ConversionfolderKey)
                                         ? ConfigurationManager.AppSettings[ConversionfolderKey]
                                         : string.Empty;

                var mimeType = MimeTypes.GetMimeTypeFromExtension(Path.GetExtension(audioFile));

                if (mimeType == MimeTypes.MimeTypeMp3 && request.MimeType == MimeTypes.MimeTypeMp3 &&
                    !string.IsNullOrEmpty(pathToMp3Split) && File.Exists(pathToMp3Split) &&
                    !string.IsNullOrEmpty(conversionPath) && Directory.Exists(conversionPath))
                {
                    byte[] bytes;

                    using (var tempFile = new TempFile(MimeTypes.ExtMp3))
                    {
                        var mp3Splt = new SplitMp3(pathToMp3Split) { Mp3FileName = new FileInfo(audioFile) };

                        var segmentedFile = mp3Splt.SingleSegment(
                            tempFile.FileName,
                            request.Start.HasValue ? request.Start.Value : 0,
                            request.End.HasValue ? request.End.Value : long.MaxValue);

                        bytes = File.ReadAllBytes(segmentedFile);
                    }

                    return bytes;
                }
            }
            catch
            {
                return new byte[0];
            }

            return new byte[0];
        }

        /// <summary>
        /// Export data for one job.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <param name="directory">
        /// The directory.
        /// </param>
        public static void Export(int jobId, string directory)
        {
            using (var db = new QutSensorsDb())
            {
                var job = db.Cache_Jobs.Where(j => j.JobID == jobId).FirstOrDefault();

                if (job == null) return;

                var jobItems = db.Cache_JobItems.Where(ji => ji.JobID == jobId);

                foreach (var jobItem in jobItems)
                {
                    string name = jobItem.JobID + "_" + jobItem.JobItemID + "_" + jobItem.Start + "-" + jobItem.End +
                                  "." + MimeTypes.GetExtension(jobItem.MimeType);
                    string fileName = Path.Combine(directory, name);

                    Cache_JobItem item = jobItem;
                    var data = db.Cache_Datas.Where(d => d.JobItemID == item.JobItemID).FirstOrDefault();
                    if (data == null)
                    {
                        continue;
                    }

                    File.WriteAllBytes(fileName, data.Data.ToArray());
                }
            }
        }
    }
}
