// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheJobProcessor.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the CacheJobProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.CacheProcessor
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using QutSensors.Business.Cache;
    using QutSensors.Business.Request;
    using QutSensors.Data;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Cache Processor.
    /// </summary>
    public class CacheJobProcessor
    {
        /// <summary>
        /// Time to wait between checks for jobs in milliseconds.
        /// </summary>
        private const int InterJobWaitPeriod = 5000;

        private readonly ManualResetEvent stopRequestedEvent = new ManualResetEvent(false);
        private readonly ILogProvider log;
        private readonly ICacheManager cacheManager;

        private readonly ISpectrogramRequestManager spectrogramRequestManager;

        /// <summary>
        /// Worker thread for generating cache data.
        /// </summary>
        private Thread workerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheJobProcessor"/> class.
        /// </summary>
        /// <param name="log">
        /// Log provider to use.
        /// </param>
        /// <param name="cacheManager">
        /// The cache manager.
        /// </param>
        /// <param name="spectrogramRequestManager">
        /// The spectrogram Request Manager.
        /// </param>
        public CacheJobProcessor(ILogProvider log, ICacheManager cacheManager, ISpectrogramRequestManager spectrogramRequestManager)
        {
            this.log = log;
            this.cacheManager = cacheManager;
            this.spectrogramRequestManager = spectrogramRequestManager;
        }

        /// <summary>
        /// Gets a value indicating whether IsRunning.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return stopRequestedEvent.WaitOne(0);
            }
        }

        /// <summary>
        /// Start the processor.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            if (workerThread != null)
            {
                throw new InvalidOperationException("Worker thread already started.");
            }

            workerThread = new Thread(ThreadMain);
            workerThread.Start();
        }

        /// <summary>
        /// Stop the processor.
        /// </summary>
        public void Stop()
        {
            stopRequestedEvent.Set();
        }

        /// <summary>
        /// Get audio reading from db.
        /// </summary>
        /// <param name="jobId">
        /// The job Id.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        /// <returns>
        /// Audio Reading.
        /// </returns>
        /// <exception cref="ArgumentException"><c>ArgumentException</c>.</exception>
        private static AudioReading GetAudioReading(int jobId)
        {
            using (var db = new QutSensorsDb())
            {
                // get audio file, and save to file.
                var reading = db.Cache_Jobs.Where(j => j.JobID == jobId).Select(j => j.AudioReading).FirstOrDefault();

                if (reading == null)
                {
                    throw new ArgumentException("Could not get audio reading for job id: " + jobId);
                }

                if (!reading.Length.HasValue)
                {
                    throw new InvalidOperationException("Audio reading id " + reading.AudioReadingID + " for job id " + jobId + " does not have a valid duration (.Length).");
                }

                return reading;
            }
        }

        /// <summary>
        /// Main method for thread.
        /// </summary>
        private void ThreadMain()
        {
            try
            {
                if (log != null)
                {
                    log.WriteEntry(LogType.Information, "Cache Job Processor starting");
                }

                do
                {
                    if (!ProcessJob())
                    {
                        // No job or error so wait for new job to process.
                        // WaitOne will return straight away if signaled (when .Stop() is called)
                        // otherwise it will wait InterJobWaitPeriod milliseconds.
                        stopRequestedEvent.WaitOne(InterJobWaitPeriod);
                    }

                    // WaitOne returns true when stopRequestedEvent has been signaled, otherwise false.
                }
                while (!stopRequestedEvent.WaitOne(InterJobWaitPeriod));

                if (log != null)
                {
                    log.WriteEntry(LogType.Information, "Cache Job Processor stopping");
                }
            }
            catch (Exception ex)
            {
                if (log != null)
                {
                    log.WriteEntry(LogType.Error, ex.ToString());
                }
            }
            finally
            {
                stopRequestedEvent.Reset();
                workerThread = null;
            }
        }

        /// <summary>
        /// The process job.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// Unable to generate spectrogram from job processor
        /// </exception>
        /// <returns>
        /// True if job was processed successfully, otherwise false.
        /// </returns>
        /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
        /// <exception cref="DirectoryNotFoundException"><c>DirectoryNotFoundException</c>.</exception>
        private bool ProcessJob()
        {
            // only does spectrogram gen. for now.
            // see if audio cache is needed when using ffmpeg & file system.
            var jobId = cacheManager.GetUnprocessedJob(CacheJobType.SpectrogramGeneration);

            if (jobId.HasValue)
            {
                try
                {
                    var reading = GetAudioReading(jobId.Value);

                    if (reading == null)
                    {
                        throw new InvalidOperationException("Could not get audio reading for job.");
                    }

                    if (!reading.Length.HasValue)
                    {
                        throw new InvalidOperationException("Cannot process Audio reading with id " + reading.AudioReadingID + ". It has null length.");
                    }

                    return ProcessJobItem(reading, jobId.Value);
                }
                catch (Exception e)
                {
                    if (log != null)
                    {
                        log.WriteEntry(LogType.Error, "Error processing job id {0}: {1}", jobId.Value, e);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Process a job item.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="jobId">
        /// The job Id.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        /// <returns>
        /// True if the item was processed successfully, otherwise false.
        /// </returns>
        private bool ProcessJobItem(AudioReading reading, int jobId)
        {
            var request = cacheManager.GetUnprocessedRequest(jobId, CacheJobType.SpectrogramGeneration);

            if (request != null)
            {
                try
                {
                    if (request.AudioReadingID != reading.AudioReadingID)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                            "Job audio reading id ({0}) and item audio reading id ({1}) must be the same.",
                            reading.AudioReadingID,
                            request.AudioReadingID));
                    }

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    byte[] data = this.GenerateSpectrogram(reading, request);

                    stopWatch.Stop();

                    if (data != null && data.Length > 0)
                    {
                        cacheManager.Insert(request, data, stopWatch.Elapsed);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    if (log != null)
                    {
                        log.WriteEntry(LogType.Error, "Error processing item: {0}", e);
                    }

                    cacheManager.SubmitError(request, e.ToString());
                }
            }

            return false;
        }

        /// <summary>
        /// Get or generate spectrogram.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// Image bytes.
        /// </returns>
        /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
        private byte[] GenerateSpectrogram(AudioReading reading, CacheRequest request)
        {
            if (!reading.Length.HasValue)
            {
                if (!reading.Length.HasValue)
                {
                    throw new InvalidOperationException("Audio reading id " + reading.AudioReadingID + " does not have a valid duration (.Length).");
                }
            }

            var sectrogramRequest = SpectrogramRequest.Create(
                request.Start, request.End, null, null, null, false, TimeSpan.FromMilliseconds(reading.Length.Value));

            Image spectrogram = this.spectrogramRequestManager.GetSpectrogram(reading, sectrogramRequest);
            byte[] bytes = spectrogram.ToByteArray(ImageFormat.Jpeg);
            return bytes;
        }
    }
}