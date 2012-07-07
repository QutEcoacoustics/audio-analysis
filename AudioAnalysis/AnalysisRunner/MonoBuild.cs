namespace AnalysisRunner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Create, Build and Delete Mono 2.6.7 compatible analysis assemblies.
    /// </summary>
    public class MonoBuild
    {
        public void CopyMonoProjectFiles(DirectoryInfo baseDirectory)
        {
            foreach (var file in baseDirectory.GetFiles("*.*proj", SearchOption.AllDirectories))
            {
                var name = Path.GetFileNameWithoutExtension(file.FullName) + "_mono" + file.Extension;
                var destFile = Path.Combine(Path.GetDirectoryName(file.FullName), name);
                if (!File.Exists(destFile))
                {
                    File.Copy(file.FullName, destFile);
                }
            }
        }

        public void CopyMonoSolutionFiles(DirectoryInfo baseDirectory)
        {
            foreach (var file in baseDirectory.GetFiles("*.sln", SearchOption.AllDirectories))
            {
                var name = Path.GetFileNameWithoutExtension(file.FullName) + "_mono" + file.Extension;
                var destFile = Path.Combine(Path.GetDirectoryName(file.FullName), name);
                if (!File.Exists(destFile))
                {
                    File.Copy(file.FullName, destFile);
                }
            }
        }

        public void DeleteMonoProjectFiles(DirectoryInfo baseDirectory)
        {
            foreach (var file in baseDirectory.GetFiles("*_mono.*proj", SearchOption.AllDirectories))
            {
                File.Delete(file.FullName);
            }
        }

        public void DeleteMonoSolutionFiles(DirectoryInfo baseDirectory)
        {
            foreach (var file in baseDirectory.GetFiles("*_mono.sln", SearchOption.AllDirectories))
            {
                File.Delete(file.FullName);
            }
        }

        public void UpdateMonoProjectFiles(DirectoryInfo baseDirectory)
        {
            // ToolsVersion="4.0"

        }
    }
}
