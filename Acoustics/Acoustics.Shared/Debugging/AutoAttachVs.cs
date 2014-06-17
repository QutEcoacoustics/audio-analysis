// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoAttachVs.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Example taken from this gist.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


#if DEBUG

namespace Acoustics.Shared.Debugging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    using EnvDTE;

    using DTEProcess = EnvDTE.Process;
    using Process = System.Diagnostics.Process;

    #region Classes

    /// <summary>
    /// Example taken from <a href="https://gist.github.com/3813175">this gist</a>.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", 
        Justification = "Reviewed. Suppression is OK here.", Scope = "class")]
    public static class VisualStudioAttacher
    {
        #region Public Methods

        [DllImport("ole32.dll")]
        public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);


        [DllImport("ole32.dll")]
        public static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        public static string GetSolutionForVisualStudio(Process visualStudioProcess)
        {
            _DTE visualStudioInstance;
            if (TryGetVsInstance(visualStudioProcess.Id, out visualStudioInstance))
            {
                try
                {
                    return visualStudioInstance.Solution.FullName;
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        public static Process GetAttachedVisualStudio(Process applicationProcess)
        {
            IEnumerable<Process> visualStudios = GetVisualStudioProcesses();

            foreach (Process visualStudio in visualStudios)
            {
                _DTE visualStudioInstance;
                if (TryGetVsInstance(visualStudio.Id, out visualStudioInstance))
                {
                    try
                    {
                        foreach (Process debuggedProcess in visualStudioInstance.Debugger.DebuggedProcesses)
                        {
                            if (debuggedProcess.Id == applicationProcess.Id)
                            {
                                return debuggedProcess;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// The method to use to attach visual studio to a specified process.
        /// </summary>
        /// <param name="visualStudioProcess">
        /// The visual studio process to attach to.
        /// </param>
        /// <param name="applicationProcess">
        /// The application process that needs to be debugged.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the application process is null.
        /// </exception>
        public static void AttachVisualStudioToProcess(Process visualStudioProcess, Process applicationProcess)
        {
            _DTE visualStudioInstance;

            if (TryGetVsInstance(visualStudioProcess.Id, out visualStudioInstance))
            {
                // Find the process you want the VS instance to attach to...
                DTEProcess processToAttachTo =
                    visualStudioInstance.Debugger.LocalProcesses.Cast<DTEProcess>()
                                        .FirstOrDefault(process => process.ProcessID == applicationProcess.Id);

                // Attach to the process.
                if (processToAttachTo != null)
                {
                    processToAttachTo.Attach();

                    ShowWindow((int)visualStudioProcess.MainWindowHandle, 3);
                    SetForegroundWindow(visualStudioProcess.MainWindowHandle);
                }
                else
                {
                    throw new InvalidOperationException(
                        "Visual Studio process cannot find specified application '" + applicationProcess.Id + "'");
                }
            }
        }

        /// <summary>
        /// The get visual studio for solutions.
        /// </summary>
        /// <param name="solutionNames">
        /// The solution names.
        /// </param>
        /// <returns>
        /// The <see cref="Process"/>.
        /// </returns>
        public static Process GetVisualStudioForSolutions(List<string> solutionNames)
        {
            foreach (string solution in solutionNames)
            {
                Process visualStudioForSolution = GetVisualStudioForSolution(solution);
                if (visualStudioForSolution != null)
                {
                    return visualStudioForSolution;
                    
                }
            }

            return null;
        }

        /// <summary>
        /// The get visual studio process that is running and has the specified solution loaded.
        /// </summary>
        /// <param name="solutionName">
        /// The solution name to look for.
        /// </param>
        /// <returns>
        /// The visual studio <see cref="Process"/> with the specified solution name.
        /// </returns>
        public static Process GetVisualStudioForSolution(string solutionName)
        {
            IEnumerable<Process> visualStudios = GetVisualStudioProcesses();

            foreach (Process visualStudio in visualStudios)
            {
                _DTE visualStudioInstance;
                if (TryGetVsInstance(visualStudio.Id, out visualStudioInstance))
                {
                    try
                    {
                        string actualSolutionName = Path.GetFileName(visualStudioInstance.Solution.FullName);

                        if (string.Compare(
                            actualSolutionName, 
                            solutionName, 
                            StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            return visualStudio;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Private Methods


        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        private static IEnumerable<Process> GetVisualStudioProcesses()
        {
            Process[] processes = Process.GetProcesses();
            return processes.Where(o => o.ProcessName.Contains("devenv"));
        }

        private static bool TryGetVsInstance(int processId, out _DTE instance)
        {
            IntPtr numFetched = IntPtr.Zero;
            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            IMoniker[] monikers = new IMoniker[1];

            GetRunningObjectTable(0, out runningObjectTable);
            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                IBindCtx ctx;
                CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                object runningObjectVal;
                runningObjectTable.GetObject(monikers[0], out runningObjectVal);

                if (runningObjectVal is _DTE && runningObjectName.StartsWith("!VisualStudio"))
                {
                    int currentProcessId = int.Parse(runningObjectName.Split(':')[1]);

                    if (currentProcessId == processId)
                    {
                        instance = (_DTE)runningObjectVal;
                        return true;
                    }
                }
            }

            instance = null;
            return false;
        }

        #endregion
    }

    #endregion
}

#endif