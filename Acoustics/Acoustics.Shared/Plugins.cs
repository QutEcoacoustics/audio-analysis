namespace Acoustics.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;


    public class Plugins
    {
        /// <summary>
        /// Get plugins from directories.
        /// </summary>
        /// <param name="interfaceName">
        /// The interface Name.
        /// </param>
        /// <param name="directories">
        /// The directories.
        /// </param>
        /// <param name="searchPatterns">
        /// The search Patterns.
        /// </param>
        /// <typeparam name="T">
        /// The type of the plugins.
        /// </typeparam>
        /// <returns>
        /// Enumerable of plugins.
        /// </returns>
        public static IEnumerable<T> GetPlugins<T>(string interfaceName, IEnumerable<DirectoryInfo> directories, params string[] searchPatterns) where T : class
        {
            var files = searchPatterns.SelectMany(s => directories.SelectMany(d => d.GetFiles(s)));

            foreach (var file in files)
            {
                Assembly assembly = Assembly.LoadFile(file.FullName);
                foreach (Type assemblyType in assembly.GetTypes())
                {
                    if (assemblyType.GetInterface(interfaceName) != null)
                    {
                        T plugin = Activator.CreateInstance(assemblyType) as T;
                        yield return plugin;
                    }
                }
            }
        }

        /// <summary>
        /// Get plugins from executing assembly.
        /// </summary>
        /// <param name="interfaceName">
        /// The interface name.
        /// </param>
        /// <typeparam name="T">
        /// The type of the plugins.
        /// </typeparam>
        /// <returns>
        /// Enumerable of plugins.
        /// </returns>
        public static IEnumerable<T> GetPlugins<T>(string interfaceName) where T : class
        {
            foreach (Type assemblyType in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (assemblyType.GetInterface(interfaceName) != null)
                {
                    T plugin = Activator.CreateInstance(assemblyType) as T;
                    yield return plugin;
                }
            }
        }
    }
}
