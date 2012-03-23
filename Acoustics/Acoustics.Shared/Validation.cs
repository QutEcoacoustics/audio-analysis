namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    public sealed class Validation
    {
        public static Validation Begin() { return null; }

        private List<Exception> exceptions;

        public IEnumerable<Exception> Exceptions { get { return this.exceptions; } }

        public Validation AddException(Exception ex)
        {
            lock (exceptions)
                exceptions.Add(ex);

            return this;
        }

        public Validation()
        {
            exceptions = new List<Exception>(1); // optimize for only having 1 exception
        }
    }

    [Serializable]
    public sealed class MultiException : Exception
    {
        private Exception[] innerExceptions;

        public IEnumerable<Exception> InnerExceptions
        {
            get
            {
                if (this.innerExceptions != null)
                {
                    for (int i = 0; i < this.innerExceptions.Length; ++i)
                    {
                        yield return this.innerExceptions[i];
                    }
                }
            }
        }

        public MultiException()
            : base()
        {
        }

        public MultiException(string message)
            : base()
        {
        }

        public MultiException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.innerExceptions = new Exception[1] { innerException };
        }

        public MultiException(IEnumerable<Exception> innerExceptions)
            : this(null, innerExceptions)
        {
        }

        public MultiException(Exception[] innerExceptions)
            : this(null, (IEnumerable<Exception>)innerExceptions)
        {
        }

        public MultiException(string message, Exception[] innerExceptions)
            : this(message, (IEnumerable<Exception>)innerExceptions)
        {
        }

        public MultiException(string message, IEnumerable<Exception> innerExceptions)
            : base(message, innerExceptions.FirstOrDefault())
        {
            if (innerExceptions.Any(o => o == null))
                throw new ArgumentNullException();

            this.innerExceptions = innerExceptions.ToArray();
        }

        private MultiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Get a string representation of this MultiException.
        /// </summary>
        /// <returns>
        /// A string representation of this MultiException.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder("MultiException: " + Environment.NewLine);

            if (InnerExceptions.Count() > 0)
            {
                foreach (var ie in InnerExceptions.Where(ie => ie != null))
                {
                    sb.AppendLine("---> " + ie);
                }
            }

            return sb.ToString();
        }
    }
}