namespace EcosoundsFeltAdapter
{
    using System;
    using System.Collections.Generic;

    using MQUTeR.FSharp.Shared;

    public interface IFeltAccessor
    {
        /// <summary>
        /// The search.
        /// </summary>
        /// <returns>
        /// The search.
        /// </returns>
        Dictionary<string, int> Search(Dictionary<string, Value> values, int limit);

        bool IsSearchAvailable { get; }

        string[] SearchUnavilabilityMessages { get; }

        DateTime CachedTrainingDataDate { get; }
    }
}