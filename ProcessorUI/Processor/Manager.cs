using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors.Processor.WebServices;
using AudioTools;
using System.Threading;
using QutSensors.Data;
using System.Xml.Linq;
using System.IO;
using TowseyLib;
using QutSensors.AudioAnalysis.AED;
using QutSensors.Processor;

using WebServices = QutSensors.Processor.WebServices;

namespace QutSensors.Processor
{
    public delegate void GenericHandler<T>(object sender, T args);

    public static class Manager
    {
        public delegate void GetNextJobCallback(ProcessorJobItemDescription item, object state);

        public static void BeginGetNextJob(string workerName, GetNextJobCallback callback, object state)
        {
            var ws = new ServiceWrapper();
            ws.Proxy.BeginGetJobItem(new GetJobItemRequest(workerName), OnGotJob, new object[] { ws, workerName, callback, state });
        }
        
        static void OnGotJob(IAsyncResult ar)
        {
            object[] asyncState = (object[])ar.AsyncState;
            var incomingWs = (ServiceWrapper)asyncState[0];
            var workerName = (string)asyncState[1];
            var callback = (GetNextJobCallback)asyncState[2];
            var state = asyncState[3];

            try
            {
                using (incomingWs)
                {
                    GetJobItemResponse item = incomingWs.Proxy.EndGetJobItem(ar);
                    incomingWs.Close();
                    callback(item.GetJobItemResult, state);
                }
            }
            catch
            {
                callback(null, state);
            }
        }

        public static ProcessorJobItemDescription GetJobItem(string workerName)
        {
            using (var incomingWs = new ServiceWrapper())
            {
                try
                {
                    GetJobItemResponse item = incomingWs.Proxy.GetJobItem(new GetJobItemRequest(workerName));
                    incomingWs.Close();
                    return item.GetJobItemResult;
                }
                catch 
                {
                    return null;
                }
            }
        }

        public static bool ProcessItem(ProcessorJobItemDescription item, string workerName, out TimeSpan? duration)
        {
            bool processed = false;
            duration = null;

            try
            {
                if (item != null)
                {
                    using (var tempFile = new TempFile(".wav"))
                    {
                        Utilities.DownloadFile(item.AudioReadingUrl, tempFile.FileName);
                        IEnumerable<ProcessorJobItemResult> results = null;
                        
                        try
                        {
                            Processor processor = ProcessorFactory.GetProcessor(item);
                            if (processor == null)
                                return false;

                            results = processor.Process(tempFile, item, out duration);
                        }
                        catch (Exception e)
                        {
                            using (var ws = new ServiceWrapper())
                            {
                                ws.Proxy.ReturnJobWithError(new ReturnJobWithErrorRequest(workerName, item.JobItemID, e.ToString()));
                                ws.Close();
                            }
                            processed = true;
                            return true;
                        }

                        if (results != null)
                        {
                            using (var ws = new ServiceWrapper())
                            {
                                ws.Proxy.SubmitResults(new SubmitResultsRequest( workerName, item.JobItemID, results.ToArray()));
                                ws.Close();
                            }
                            processed = true;
                            return true;
                        }
                    }
                }
                return false;
            }
            finally
            {
                if (!processed && item != null)
                {
                    using (var ws = new ServiceWrapper())
                    {
                        ws.Proxy.ReturnJob(new ReturnJobRequest(workerName, item.JobItemID));
                        ws.Close();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Wraps the WCF Service to ensure Abort or Close is called as appropriate
    /// Close should be called in normal circumstances, Abort if there's an error.
    /// Easiest way to use is:
    /// using (var ws = new ServiceWrapper()) {
    ///		ws.Proxy.Call();
    ///		ws.Close();
    ///	}
    ///	That way, if Call() fails then an exception is thrown and Dispose is called without Close
    ///	being called beforehand. In that case the wrapper will call Abort.
    /// </summary>
    public class ServiceWrapper : IDisposable
    {
        WebServices.ProcessorClient proxy;
        public ServiceWrapper()
        {
            proxy = new WebServices.ProcessorClient("WSHttpBinding_Processor", Settings.Server);
        }

        public WebServices.ProcessorClient Proxy
        {
            get { return proxy; }
        }

        public void Close()
        {
            proxy.Close();
            proxy = null;
        }

        #region IDisposable
        public void Dispose()
        {
            if (proxy != null)
                proxy.Abort();
        }

        ~ServiceWrapper()
        {
            Dispose();
        }
        #endregion
    }

}