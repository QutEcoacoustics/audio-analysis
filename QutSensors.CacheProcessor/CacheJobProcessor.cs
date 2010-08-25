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
    using System.IO;
    using System.Linq;
    using System.Threading;

    using AudioTools;
    using AudioTools.DirectShow;

    using Autofac;

    using QutSensors.Business;
    using QutSensors.Data;
    using QutSensors.Data.Cache;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Cache Processor.
    /// </summary>
    public class CacheJobProcessor
    {
        /// <summary>
        /// Default buffer size in bytes.
        /// </summary>
        private const int BufferSize = 1024 * 1024 * 10;

        /// <summary>
        /// Time to wait between checks for jobs in milliseconds.
        /// </summary>
        private const int InterJobWaitPeriod = 5000;

        private readonly ManualResetEvent stopRequestedEvent = new ManualResetEvent(false);
        private readonly ILogProvider log;
        private readonly ICacheManager cacheManager;

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
        public CacheJobProcessor(ILogProvider log, ICacheManager cacheManager)
        {
            this.log = log;
            this.cacheManager = cacheManager;
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
                        stopRequestedEvent.WaitOne(InterJobWaitPeriod);
                    }
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
        private bool ProcessJob()
        {
            var request = cacheManager.GetUnprocessedRequest();
            if (request != null)
            {
                try
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    byte[] data = null;
                    switch (request.Type)
                    {
                        case CacheJobType.AudioSegmentation:
                            data = this.SegmentAudio(request);
                            break;
                        case CacheJobType.SpectrogramGeneration:
                            data = this.GenerateSpectrogram(request);
                            break;
                        default:
                            throw new InvalidOperationException("Unrecognised CacheRequest type: " + request.Type);
                    }

                    stopWatch.Stop();

                    if (data != null)
                    {
                        cacheManager.Insert(request, data, stopWatch.Elapsed);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    if (log != null)
                    {
                        log.WriteEntry(LogType.Error, "Error processing job: {0}", e);
                    }

                    cacheManager.SubmitError(request, e.ToString());
                }
            }

            return false;
        }

        /// <summary>
        /// The segment audio.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Unable to find referenced AudioReading
        /// </exception>
        /// <returns>
        /// Byte array representing segment of audio.
        /// </returns>
        private byte[] SegmentAudio(CacheRequest request)
        {
            using (var db = new QutSensorsDb())
            {
                var reading = db.AudioReadings.FirstOrDefault(r => r.AudioReadingID == request.AudioReadingID);
                if (reading == null)
                {
                    throw new InvalidOperationException("Unable to find referenced AudioReading");
                }

                var segment = new AudioReadingSegments(
                    reading,
                    QutDependencyContainer.Instance.Container.Resolve<IAudioReadingManager>(),
                    QutDependencyContainer.Instance.Container.Resolve<ICacheManager>());

                using (var targetStream = new MemoryStream())
                {
                    segment.GetAudio(
                        request.Start,
                        reading.Length == request.End ? null : request.End,
                        request.MimeType,
                        targetStream,
                        BufferSize,
                        false);

                    if (log != null)
                    {
                        log.WriteEntry(
                            LogType.Information,
                            "Segmented audio {0} ({1}-{2})",
                            request.AudioReadingID,
                            request.Start,
                            request.End);
                    }

                    return targetStream.GetBuffer();
                }
            }
        }

        /// <summary>
        /// Generate spectrogram based on CacheRequest.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Unable to find referenced AudioReading
        /// </exception>
        /// <returns>
        /// The generated spectrogram.
        /// </returns>
        private byte[] GenerateSpectrogram(CacheRequest request)
        {
            using (var db = new QutSensorsDb())
            {
                var reading = db.AudioReadings.FirstOrDefault(r => r.AudioReadingID == request.AudioReadingID);
                if (reading == null)
                {
                    throw new InvalidOperationException("Unable to find referenced AudioReading");
                }

                var spec = new Spectrogram(
               reading,
               QutDependencyContainer.Instance.Container.Resolve<IAudioReadingManager>(),
               QutDependencyContainer.Instance.Container.Resolve<ISpectrogramGenerator>());

                var bytes = spec.Generate(
                    false,
                    null,
                    request.Start,
                    reading.Length == request.End ? null : request.End,
                    null,
                    false,
                    false);

                if (log != null)
                {
                    log.WriteEntry(
                        LogType.Information,
                        "Generated spectrogram {0} ({1}-{2})",
                        request.AudioReadingID,
                        request.Start,
                        request.End);
                }

                return bytes;
            }
        }
    }
}