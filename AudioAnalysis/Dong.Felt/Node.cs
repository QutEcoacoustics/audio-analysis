using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt
{
    public class Node
    {
        public int Value { get; set; }

        public Node Previous { get; set; }

        public Node Next { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Node()
        {
        }

        public Node(int value, Node previous, Node next)
        {
            this.Value = value;
            this.Previous = previous;
            this.Next = next;
        }
    }
}
