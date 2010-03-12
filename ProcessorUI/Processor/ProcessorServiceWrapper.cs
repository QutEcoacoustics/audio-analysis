using System;
using QutSensors.Processor.ProcessorService;


namespace QutSensors.Processor
{
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
    public class ProcessorServiceWrapper : IDisposable
    {
        ProcessorServiceClient proxy;
        public ProcessorServiceWrapper()
        {
            proxy = new ProcessorServiceClient("basicBinding_ProcessorService");
        }

        public ProcessorServiceClient Proxy
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

        ~ProcessorServiceWrapper()
        {
            Dispose();
        }
        #endregion
    }
}
