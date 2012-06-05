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
        public static readonly string CacheDirectory = "";

        public static Data CachedTrainingData;

        public static DateTime CachedTrainingDataDate;

        static FeltAccessor()
        {
            // set up the workflow for analysis

            //based off "GlobalZScore"
            var workflow = new[]
                {
                    Workflows.WorkflowItem.NewCleaner(new BasicCleaner()),
                    Workflows.WorkflowItem.NewSelection(new OneForOneSelector()),
                    Workflows.WorkflowItem.NewTransformer(new ZScoreNormalise()),
                    Workflows.WorkflowItem.NewTrainer(new GroupTrainer()),
                    Workflows.WorkflowItem.NewClassifier(new EuclideanClassifier())
                };
        }

        public FeltAccessor()
        {
            if (CheckForNewerVersionOfCache(this))
            {
                DateTime modifiedDate;
                var newData = LoadTrainedData(out modifiedDate);

                lock (this)
                {
                    CachedTrainingData = newData;
                    CachedTrainingDataDate = modifiedDate;
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
