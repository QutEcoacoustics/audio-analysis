// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FeltAccessor.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the FeltAccessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EcosoundsFeltAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using FELT;
    using FELT.Classifiers;
    using FELT.Cleaners;
    using FELT.Selectors;
    using FELT.Trainers;
    using FELT.Transformers;

    using MQUTeR.FSharp.Shared;

    public class FeltAccessor
    {
        /// <summary>
        /// The directory to store the trained caches.
        /// </summary>
        public readonly DirectoryInfo CacheDirectory;

        public const string FeltExt = "feltcache";

        /// <summary>
        /// The Current cache loaded up from storage into memory.
        /// </summary>
        private Data cachedTrainingData;

        private DateTime cachedTrainingDataDate;

        private Version cachedTrainingDataVersion;

        static FeltAccessor()
        {
            // set up the workflow for analysis

            // based off "GlobalZScore"
            var workflow = new[]
                {
                    Workflows.WorkflowItem.NewCleaner(new BasicCleaner()),
                    Workflows.WorkflowItem.NewSelection(new OneForOneSelector()),
                    Workflows.WorkflowItem.NewTransformer(new ZScoreNormalise()),
                    Workflows.WorkflowItem.NewTrainer(new GroupTrainer()),
                    Workflows.WorkflowItem.NewClassifier(new EuclideanClassifier())
                };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeltAccessor"/> class.
        /// </summary>
        public FeltAccessor(DirectoryInfo cacheDirectory)
        {
            Contract.Requires(cacheDirectory.Exists);

            this.CacheDirectory = cacheDirectory;

            if (CheckForNewerVersionOfCache(this))
            {
                DateTime modifiedDate;
                Version analyserVersion;
                var newData = LoadTrainedData(out modifiedDate, out analyserVersion);

                this.cachedTrainingData = newData;
                this.cachedTrainingDataDate = modifiedDate;
                this.cachedTrainingDataVersion = analyserVersion;
            }
        }

        public object Search()
        {
            // adjust cache for use with applicable features

            // construct the test set

            // pass both into the classifier

            // summarise the results

            return null;
        }


        private bool CheckForNewerVersionOfCache(out FileInfo newerVersion)
        {
            // get files in cache dir
                // pick most recent (modified)
                // if most recent > thiss.CachedTrainginDataDate
                    // then return true

            // get files in cache dir, pick most recent (modified)
            var latestCacheFile =
                this.CacheDirectory
                .EnumerateFiles()
                .Where(fi => fi.Extension == FeltExt)
                .M
                .Max(fi => fi.CreationTime);

            if (latestCacheFile.)

            return false;
        }

        private Data LoadTrainedData(out DateTime cachedDate, out Version analyserVersion)
        {


            // deserialise into Data object
            MQUTeR.FSharp.Shared.CacheFormat c;


            cachedDate = c.SaveDate;

            // ensure versions match!
            Version versionFromFile = c.Assembly;
            
            Assembly feltAsbly = Assembly.GetAssembly(typeof(FELT.Workflows));
            Version versionFromAssembly = feltAsbly.GetName().Version;

            if (versionFromAssembly != versionFromFile)
            {
                throw new InvalidOperationException(string.Format("The version of the cache file does not match the version of the FELT assembly. File:{0}, Assembly:{1}", versionFromFile.ToString(), versionFromAssembly));
            }

            analyserVersion = versionFromFile;

            return c.CachedData;
        }
    }
}
