using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using QutSensors.Data.Cache;
using QutSensors.Data.Linq;
using AudioTools;
using AudioTools.DirectShow;
using QutSensors.Shared;
using System.IO;
using System.Diagnostics;
using QutSensors.Data;
using Autofac;

namespace QutSensors.CacheProcessor
{
    public class CacheJobProcessor
    {
        const int BufferSize = 1024 * 1024 * 10;
        const int InterJobWaitPeriod = 5000;

        Thread workerThread;
        ManualResetEvent stopRequestedEvent = new ManualResetEvent(false);
        ILogProvider log;
        ICacheManager cacheManager;

        public CacheJobProcessor(ILogProvider log, ICacheManager cacheManager)
        {
            this.log = log;
            this.cacheManager = cacheManager;
        }

        #region Properties
        public bool IsRunning { get { return stopRequestedEvent.WaitOne(0); } }
        #endregion

        public void Start()
        {
            if (workerThread != null)
                throw new InvalidOperationException();

            workerThread = new Thread(ThreadMain);
            workerThread.Start();
        }

        public void Stop()
        {
            stopRequestedEvent.Set();
        }

        void ThreadMain()
        {
            log.WriteEntry(LogType.Information, "Cache Job Processor starting");

            do
            {
                if (!ProcessJob())
                    stopRequestedEvent.WaitOne(InterJobWaitPeriod); // No job or error so wait for new job to process.
            } while (!stopRequestedEvent.WaitOne(InterJobWaitPeriod));

            log.WriteEntry(LogType.Information, "Cache Job Processor stopping");

            stopRequestedEvent.Reset();
            workerThread = null;
        }

        bool ProcessJob()
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
                        cacheManager.Insert(request, data, stopWatch.Elapsed);
                    return true;
                }
                catch (Exception e)
                {
                    log.WriteEntry(LogType.Error, "Error processing job: {0}", e);
                    cacheManager.SubmitError(request, e.ToString());
                }
            }
            return false;    
        }

        byte[] SegmentAudio(CacheRequest request)
        {
            using (var db = new QutSensorsDb())
            {
                var reading = db.AudioReadings.FirstOrDefault(r => r.AudioReadingID == request.AudioReadingID);
                if (reading == null)
                    throw new InvalidOperationException("Unable to find referenced AudioReading");

                DirectShowStream stream = ConvertStream(reading.AudioReadingID, reading.MimeType, request.MimeType, request.Start, reading.Length == request.End ? null : request.End);

                using (var targetStream = new MemoryStream())
                {
                    if (stream == null)
                    {
                        // No conversion required
                        var sqlFile = SqlFilestream.CreateAudioReading(QutSensorsDb.ConnectionString, reading.AudioReadingID);
                        using (var fileStream = new System.Data.SqlTypes.SqlFileStream(sqlFile.FileName, sqlFile.Context, FileAccess.Read))
                        {
                            var buffer = new byte[BufferSize];
                            long pos = 0;
                            int read;
                            do
                            {
                                read = (int)fileStream.Read(buffer, 0, BufferSize);
                                if (read > 0)
                                {
                                    targetStream.Write(buffer, 0, read);
                                    pos += read;
                                }
                            } while (read > 0);
                        }
                    }
                    else
                    {
                        using (stream)
                        {
                            ReceiveData onReceivedData = new ReceiveData(delegate(int size, byte[] data)
                            {
                                if (size != -1)
                                    targetStream.Write(data, 0, data.Length);
                            });
                            stream.ReceivedData += onReceivedData;
                            stream.WaitForCompletion();
                            stream.ReceivedData -= onReceivedData;
                        }
                    }

                    log.WriteEntry(LogType.Information, "Segmented audio {0} ({1}-{2})", request.AudioReadingID, request.Start, request.End);

                    return targetStream.GetBuffer();
                }
            }
        }

        static DirectShowStream ConvertStream(Guid readingID, string srcType, string dstType, long? start, long? end)
        {
            var sqlFile = SqlFilestream.CreateAudioReading(QutSensorsDb.ConnectionString, readingID);
            var retVal = DShowConverter.ConvertTo(sqlFile, srcType, dstType, start, end);
            if (retVal == null)
                sqlFile.Dispose();
            else
                retVal.AssociatedObjects.Add(sqlFile);
            return retVal;
        }
    }
}