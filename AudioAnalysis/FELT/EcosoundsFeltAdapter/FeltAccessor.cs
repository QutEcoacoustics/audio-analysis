// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FeltAccessor.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace EcosoundsFeltAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using FELT;
    using FELT.Classifiers;
    using FELT.Cleaners;
    using FELT.Results;
    using FELT.Selectors;
    using FELT.Trainers;
    using FELT.Transformers;

    using Microsoft.FSharp.Collections;
    using Microsoft.FSharp.Core;

    using MQUTeR.FSharp.Shared;

    using log4net;

    /// <summary>
    /// The felt accessor.
    /// </summary>
    public class FeltAccessor : IFeltAccessor
    {
        #region Constants and Fields

        /// <summary>
        /// The felt ext.
        /// </summary>
        public const string FeltExt = ".feltcache";

        /// <summary>
        /// The directory to store the trained caches.
        /// </summary>
        public readonly DirectoryInfo CacheDirectory;

        /// <summary>
        /// The Current cache loaded up from storage into memory.
        /// </summary>
        private readonly Data cachedTrainingData;

        private readonly DateTime currentCacheFileWriteDate;

        private DateTime cachedTrainingDataDate;

        private Version cachedTrainingDataVersion;

        private readonly EuclideanClassifier classifier;

        private readonly WebsiteComputation resultComputation;

        private readonly ZScoreNormalise zScoreNormaliser;

        private readonly FSharpMap<string, object> cachedExtraData;

        private static ILog logger;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="FeltAccessor"/> class.
        /// </summary>
        static FeltAccessor()
        {
            logger = LogManager.GetLogger(typeof(FeltAccessor));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeltAccessor"/> class.
        /// </summary>
        /// <param name="cacheDirectory">
        /// The cache Directory.
        /// </param>
        public FeltAccessor(DirectoryInfo cacheDirectory)
        {
            Contract.Requires(cacheDirectory.Exists);
            logger.Info("Loading FELT cache");
            Stopwatch s = null;
            if (logger.IsDebugEnabled)
            {
                s = new Stopwatch();
                s.Start();
            }


            this.CacheDirectory = cacheDirectory;
            FileInfo newFile;
            CacheFormat cacheFormat;
            if (this.CheckForNewerVersionOfCache(out newFile))
            {
                Version analyserVersion;
                var newData = this.LoadTrainedData(newFile, out cacheFormat, out analyserVersion);

                this.cachedTrainingData = newData;
                this.cachedTrainingDataDate = cacheFormat.SaveDate;
                this.cachedTrainingDataVersion = analyserVersion;
                this.currentCacheFileWriteDate = newFile.LastWriteTime;
                this.cachedExtraData = cacheFormat.ExtraData;
            }
            else
            {
                throw new InvalidOperationException("Can't create a new FeltAccessor without a cache file");
            }

            // based off "GlobalZScore"
            this.zScoreNormaliser = new ZScoreNormalise();
            this.classifier = new EuclideanClassifier(FSharpOption<bool>.Some(true));
            this.resultComputation = new WebsiteComputation(cacheFormat);

            if (logger.IsDebugEnabled)
            {
                s.Stop();
                logger.DebugFormat("FELT constructor loaded ({0})", s.Elapsed);
            }
        }

        #endregion

        #region Public Properties

        public DateTime CachedTrainingDataDate
        {
            get
            {
                return this.cachedTrainingDataDate;
            }
        }

        /// <summary>
        /// Gets DataSet.
        /// </summary>
        public Data DataSet
        {
            get
            {
                return this.cachedTrainingData;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The search.
        /// </summary>
        /// <returns>
        /// The search.
        /// </returns>
        public System.Collections.Generic.SortedDictionary<int, string> Search(Dictionary<string, Value> values, int limit)
        {
            Contract.Requires(limit > 0);

            // adjust cache for use with applicable features
            Data dtraining = this.cachedTrainingData;
            var hdrs = dtraining.Headers;
            var instances = dtraining.Instances;
            foreach (var kvp in dtraining.Headers)
            {
                if (!values.ContainsKey(kvp.Key))
                {
                    hdrs = hdrs.Remove(kvp.Key);
                    instances = instances.Remove(kvp.Key);
                }
            }

            dtraining = new Data(dtraining.DataSet, hdrs, instances, dtraining.ClassHeader, dtraining.Classes);

            // if there a no features left in the training set by now there is nothing to compare with, return no results
            if (dtraining.Headers.Count == 0 || dtraining.Instances.Count == 0)
            {
                return new SortedDictionary<int, string>();
            }


            // construct the test set
            var items = new List<Tuple<Tuple<string, DataType>, Tuple<string, Value[]>>>();
            foreach (var key in values.Keys)
            {
                items.Add(
                    Tuple.Create(Tuple.Create(key, hdrs[key]), Tuple.Create(key, new[] { values[key] })));
            }

            var testHdrs = new FSharpMap<string, DataType>(items.Select(Microsoft.FSharp.Core.Operators.Fst));
            var testInstances =
                new FSharpMap<string, Value[]>(items.Select(Microsoft.FSharp.Core.Operators.Snd));
            Data d = new Data(MQUTeR.FSharp.Shared.DataSet.Test, testHdrs, testInstances, "UnknownWebsiteTag", new[] { string.Empty });

            // z-score processing
            var meanStdDevMap =
                (FSharpMap<string,Tuple<double, double>>)
                this.cachedExtraData["ZScoreNormalise"];
            d = this.zScoreNormaliser.NormaliseWithValues(d, meanStdDevMap);

            // pass both into the classifier
            var lazyClassifier = this.classifier.Classify(dtraining, d);

            var computationResults = this.resultComputation.Calculate(dtraining, d, lazyClassifier, limit);

            // summarise the results
            return computationResults;
        }

        public bool IsSearchAvailable
        {
            get
            {
                return true;
            }
        }

        public string[] SearchUnavilabilityMessages
        {
            get
            {
                return new string[0];
            }
        }

        #endregion

        #region Methods

        private bool CheckForNewerVersionOfCache(out FileInfo newerVersion)
        {
            DateTime max = DateTime.MinValue;
            FileInfo maxFile = null;
            foreach (var file in this.CacheDirectory.EnumerateFiles())
            {
                if (file.Extension == FeltExt)
                {
                    if (file.LastWriteTime > max)
                    {
                        max = file.LastWriteTime;
                        maxFile = file;
                    }
                }
            }

            if (maxFile == null || max <= this.currentCacheFileWriteDate)
            {
                newerVersion = null;
                return false;
            }

            newerVersion = maxFile;
            return true;
        }

        private Data LoadTrainedData(FileInfo cachedFile, out CacheFormat cacheFormat, out Version analyserVersion)
        {
            Contract.Requires(
                this.DataSet == null, "Currently loading of a data set may only be done on construction of object");

            Stopwatch deserTimer = new Stopwatch();
            deserTimer.Start();

            // deserialise into Data object
            cacheFormat = Serialization.deserializeBinaryStream<CacheFormat>(cachedFile.OpenRead());

            deserTimer.Stop();
            logger.DebugFormat("Deserialization of FELT cache took {0}", deserTimer.Elapsed);

            // ensure versions match!
            Version versionFromFile = cacheFormat.Assembly.Version;

            Assembly feltAsbly = Assembly.GetAssembly(typeof(Workflows));
            Version versionFromAssembly = feltAsbly.GetName().Version;

            if (!(versionFromAssembly.Major == versionFromFile.Major && versionFromAssembly.Minor == versionFromFile.Minor))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The version of the cache file does not match the version of the FELT assembly. File:{0}, Assembly:{1}",
                        versionFromFile,
                        versionFromAssembly));
            }

            analyserVersion = versionFromFile;

            return cacheFormat.CachedData;
        }

        #endregion
    }
}