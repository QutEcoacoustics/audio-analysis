using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysis;
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
using TempFile = QutSensors.Processor.TempFile;
using Settings = QutSensors.Processor.Settings;
using Utilities = QutSensors.Processor.Utilities;
using ServiceWrapper = QutSensors.Processor.ServiceWrapper;

namespace ProcessorUI
{
    public delegate void GenericHandler<T>(object sender, T args);

    public class PollingSystem
    {
        public enum ProcessorState
        {
            Ready,
            Running,
            Stopping
        }

        AutoResetEvent stopped;
        long runningThreads;

        public PollingSystem()
        {
            TotalDuration = TimeSpan.Zero;
        }

        #region Properties
        public int FilesProcessed { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public long ThreadsRunning
        {
            get
            {
                lock (this)
                    return runningThreads;
            }
        }
        #endregion

        public void Start()
        {
            State = ProcessorState.Running;
            lock (this)
            {
                if (Settings.NumberOfThreads == 1)
                {
                    runningThreads = 1;
                    GetNextJob(Settings.WorkerName);
                }
                else
                {
                    for (int i = 0; i < Settings.NumberOfThreads; i++)
                    {
                        Interlocked.Increment(ref runningThreads);
                        GetNextJob(Settings.WorkerName + "_" + i.ToString());
                    }
                }
            }
        }

        public void Stop()
        {
            State = ProcessorState.Stopping;
        }

        public void StopAndWait()
        {
            if (State == ProcessorState.Ready)
                return;

            if (stopped == null)
                stopped = new AutoResetEvent(false);
            Stop();
            while (Interlocked.Read(ref runningThreads) > 0)
                stopped.WaitOne();
            stopped = null;
        }

        #region Properties
        public ProcessorState State { get; private set; }
        #endregion

        void GetNextJob(string workerName)
        {
            if (State == ProcessorState.Stopping)
            {
                OnLog("Stopping");
                OnStopped();
            }
            OnLog("Requesting jobs...");

            Manager.BeginGetNextJob(workerName, OnGotJob, workerName);
        }

        void OnGotJob(ProcessorJobItemDescription item, object state)
        {
            string workerName = (string)state;

            if (State == ProcessorState.Stopping)
            {
                OnLog("Stopping");
                OnStopped();
            }
            else
            {
                try
                {
                    bool processed = false;
                    
                    if (item == null)
                    {
                        OnLog("No jobs available");
                        System.Threading.Thread.Sleep(30000);
                        GetNextJob(workerName);
                    }
                    else
                    {
                        processed = ProcessJobItem(item, workerName);

                        if (State == ProcessorState.Running)
                            GetNextJob(workerName);
                        else
                            OnStopped();
                    }
                    
                }
                catch (Exception e)
                {
                    OnLog("ERROR! " + e.ToString());
                    OnLog("Sleeping...");
                    Thread.Sleep(5000);
                    GetNextJob(workerName);
                }
            }
        }

        bool ProcessJobItem(ProcessorJobItemDescription item, string workerName)
        {
            if (State == ProcessorState.Stopping)
                return false;

            TimeSpan? duration = null;
            bool success = Manager.ProcessItem(item, workerName, out duration);

            if (success)
            {
                try
                {
                    lock (this)
                    {
                        FilesProcessed++;
                        if (duration != null)
                            TotalDuration += duration.Value;
                    }
                }
                catch { } // Don't allow this to bring down the processor!
            }
            return success;
        }

        #region Events
        public event GenericHandler<string> Log;
        protected void OnLog(string format, params object[] args)
        {
            if (Log != null)
                Log(this, string.Format(format, args));
        }

        public event EventHandler Stopped;
        protected void OnStopped()
        {
            OnLog("Stopped");
            State = ProcessorState.Ready;
            Interlocked.Decrement(ref runningThreads);
            if (stopped != null)
                stopped.Set();
            if (Interlocked.Read(ref runningThreads) == 0 && Stopped != null)
                Stopped(this, EventArgs.Empty);
        }
        #endregion
    }

}