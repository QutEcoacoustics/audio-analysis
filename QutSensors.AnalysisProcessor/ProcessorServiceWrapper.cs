// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessorServiceWrapper.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   ProcessorService Wrapper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.AnalysisProcessor
{
    using System;

    using QutSensors.AnalysisProcessor.ProcessorService;

    /// <summary>
    /// Wraps the WCF Service to ensure Abort or Close is called as appropriate
    /// Close should be called in normal circumstances, Abort if there's an error.
    /// Easiest way to use is:
    /// using (var ws = new ServiceWrapper()) {
    ///     ws.Proxy.Call();
    ///     ws.Close();
    /// }
    /// That way, if Call() fails then an exception is thrown and Dispose is called without Close
    /// being called beforehand. In that case the wrapper will call Abort.
    /// </summary>
    public class ProcessorServiceWrapper : IDisposable
    {
        private ProcessorServiceClient proxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorServiceWrapper"/> class.
        /// </summary>
        public ProcessorServiceWrapper()
        {
            proxy = new ProcessorServiceClient("basicBinding_ProcessorService");
        }

        /// <summary>
        /// Gets Proxy.
        /// </summary>
        public ProcessorServiceClient Proxy
        {
            get { return proxy; }
        }

        /// <summary>
        /// Close the proxy connection.
        /// </summary>
        public void Close()
        {
            proxy.Close();
            proxy = null;
        }

        #region IDisposable

        /// <summary>
        /// Clean up proxy connections.
        /// </summary>
        public void Dispose()
        {
            if (proxy != null)
            {
                proxy.Abort();
            }
        }

        ~ProcessorServiceWrapper()
        {
            Dispose();
        }
        #endregion
    }
}
