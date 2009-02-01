using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AudioStuff
{
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

	public static class ValidationExtensions
	{
		public static Validation IsNotNull<T>(this Validation validation, T theObject, string paramName)
			where T : class
		{
			if (IsNullable(typeof(T)) && theObject == null)
				return (validation ?? new Validation()).AddException(new ArgumentNullException(paramName));
			else
				return validation;
		}

		public static Validation IsNotNull<T>(this Validation validation, Nullable<T> theObject, string paramName)
			where T : struct
		{
			if (!theObject.HasValue)
				return (validation ?? new Validation()).AddException(new ArgumentNullException(paramName));
			else
				return validation;
		}

		static bool IsNullable(Type t)
		{
			if (t.IsGenericType)
			{
				var t2 = t.GetGenericTypeDefinition();
				return t2.Equals(typeof(Nullable<>));
			}
			return false;
		}

		public static Validation IsStateNotNull<T>(this Validation validation, T theObject, string message)
			where T : class
		{
			if (theObject == null)
				return (validation ?? new Validation()).AddException(new InvalidOperationException(message));
			else
				return validation;
		}

		public static Validation IsPositive(this Validation validation, long value, string paramName)
		{
			if (value < 0)
				return (validation ?? new Validation()).AddException(new ArgumentOutOfRangeException(paramName, "must be positive, but was " + value.ToString()));
			else
				return validation;
		}

		public static Validation IsOfType<T>(this Validation validation, T o, Type type, string paramName)
		{
			if (o != null && o.GetType() != type)
				return (validation ?? new Validation()).AddException(new ArgumentException(paramName, "must be of type " + type.ToString()));
			else
				return validation;
		}

		public static Validation Check(this Validation validation)
		{
			if (validation == null)
				return validation;
			else
			{
				if (validation.Exceptions.Take(2).Count() == 1)
					throw new MultiException("Validation failed", validation.Exceptions.First()); // ValidationException is just a standard Exception-derived class with the usual four constructors
				else
					throw new MultiException("Validation failed", new MultiException(validation.Exceptions)); // implementation shown below
			}
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
	}
}