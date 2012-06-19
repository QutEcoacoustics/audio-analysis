using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcosoundsFeltAdapter
{
    using MQUTeR.FSharp.Shared;

    public class UnavailableFeltAccessor : IFeltAccessor
    {
        private string[] messages;

        public UnavailableFeltAccessor(params string[] messages)
        {
            this.messages = messages;
        }

        public Dictionary<string, int> Search(Dictionary<string, Value> values, int limit)
        {
            throw new InvalidOperationException("No search is possible at this time.");
        }

        public bool IsSearchAvailable
        {
            get
            {
                return false;
            }
        }

        public string[] SearchUnavilabilityMessages
        {
            get
            {
                return this.messages;
            }
        }
    }
}
