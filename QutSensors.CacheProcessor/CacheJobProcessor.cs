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

    using QutSensors.Data.Cache;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Cache Processor.
    /// </summary>
    public class CacheJobProcessor
    {
        private const int BufferSize = 1024 * 1024 * 10;
        private const int InterJobWaitPeriod = 5000;

        private readonly ManualResetEvent stopRequestedEvent = new ManualResetEvent(false);
        private readonly ILogProvider log;
        private readonly ICacheManager cacheManager;

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
                throw new InvalidOperationException();
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

        private static DirectShowStream ConvertStream(Guid readingId, string srcType, string dstType, long? start, long? end)
        {
            var sqlFile = SqlFilestream.CreateAudioReading(QutSensorsDb.ConnectionString, readingId);
            var retVal = DShowConverter.ConvertTo(sqlFile, srcType, dstType, start, end);
            if (retVal == null)
                sqlFile.Dispose();
            else
                retVal.AssociatedObjects.Add(sqlFile);
            return retVal;
        }

        private void ThreadMain()
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

            stopRequestedEvent.Reset();
            workerThread = null;
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
                            data = SegmentAudio(request);
                            break;
                        case CacheJobType.SpectrogramGeneration:
                            throw new NotImplementedException("Unable to generate spectrogram from job processor");
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

                var stream = ConvertStream(reading.AudioReadingID, reading.MimeType, request.MimeType, request.Start, reading.Length == request.End ? null : request.End);

                using (var targetStream = new MemoryStream())
                {
                    if (stream == null)
                    {
                        // No conversion required
                        var sqlFile = SqlFilestream.CreateAudioReading(QutSensorsDb.ConnectionString, reading.AudioReadingID);
                        using (var fileStream = new System.Data.SqlTypes.SqlFileStream(sqlFile.FileName, sqlFile.Context, FileAccess.Read))
                        {
                            var buffer = new byte[BufferSize];
                            int read;

                            do
                            {
                                read = fileStream.Read(buffer, 0, BufferSize);
                                if (read > 0)
                                {
                                    targetStream.Write(buffer, 0, read);
                                }
                            }
                            while (read > 0);
                        }
                    }
                    else
                    {
                        using (stream)
                        {
                            var onReceivedData = new ReceiveData(delegate(int size, byte[] data)
                            {
                                if (size != -1)
                                {
                                    targetStream.Write(data, 0, data.Length);
                                }
                            });
                            stream.ReceivedData += onReceivedData;
                            stream.WaitForCompletion();
                            stream.ReceivedData -= onReceivedData;
                        }
                    }

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
    }
}