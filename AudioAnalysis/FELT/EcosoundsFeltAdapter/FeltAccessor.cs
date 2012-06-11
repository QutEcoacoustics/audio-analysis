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
    using System.Linq;
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
        public static readonly string CacheDirectory = string.Empty;

        /// <summary>
        /// The Current cache loaded up from storage into memory.
        /// </summary>
        private static Data cachedTrainingData;

        private static DateTime cachedTrainingDataDate;

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
        public FeltAccessor()
        {
            if (CheckForNewerVersionOfCache(this))
            {
                DateTime modifiedDate;
                var newData = LoadTrainedData(out modifiedDate);

                lock (this)
                {
                    cachedTrainingData = newData;
                    cachedTrainingDataDate = modifiedDate;
                }
            }
        }

        public object Search()
        {
            
        }


        private static bool CheckForNewerVersionOfCache(FeltAccessor thiss)
        {
            // get files in cache dir
                // pick most recent (modified)
                // if most recent > thiss.CachedTrainginDataDate
                    // then return true
            return false;
        }

        private static Data LoadTrainedData(out DateTime cachedDate)
        {
            // get files in cache dir
                // pick most recent (modified)
                // parse csv into Data object
            cachedDate = DateTime.Now;
            return null;
        }
    }
}
