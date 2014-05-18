namespace Acoustics.Shared
{
    using System;
    using System.Linq;

    public static class ValidationExtensions
    {
        public static Validation Is(this Validation validation, bool conditional, string message)
        {
            if (!conditional)
                return (validation ?? new Validation()).AddException(new InvalidOperationException(message));
            else
                return validation;
        }

        public static Validation IsNot(this Validation validation, bool conditional, string message)
        {
            if (conditional)
                return (validation ?? new Validation()).AddException(new InvalidOperationException(message));
            else
                return validation;
        }

        public static Validation IsNotNull<T>(this Validation validation, T theObject, string paramName)
            where T : class
        {
            if (theObject == null)
                return (validation ?? new Validation()).AddException(new ArgumentNullException(paramName));
            else
                return validation;
        }

        public static Validation IsNotNull<T>(this Validation validation, Nullable<T> theObject, string paramName)
            where T : struct
        {
            if (theObject == null)
                return (validation ?? new Validation()).AddException(new ArgumentNullException(paramName));
            else
                return validation;
        }

        public static Validation IsNotNull<T>(this Validation validation, T theObject, string paramName, string message)
            where T : class
        {
            if (theObject == null)
                return (validation ?? new Validation()).AddException(new ArgumentNullException(paramName, message));
            else
                return validation;
        }

        public static Validation IsNotNull<T>(this Validation validation, Nullable<T> theObject, string paramName, string message)
            where T : struct
        {
            if (theObject == null)
                return (validation ?? new Validation()).AddException(new ArgumentNullException(paramName, message));
            else
                return validation;
        }

        public static Validation IsNotNullOrEmpty(this Validation validation, string theString, string paramName, string message)
        {
            if (string.IsNullOrEmpty(theString))
                return (validation ?? new Validation()).AddException(new ArgumentNullException(paramName, message));
            else
                return validation;
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

        public static Validation IsPositive(this Validation validation, double value, string paramName)
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

        public static Validation DoesntContain(this Validation validation, string value, char test, string paramName)
        {
            if (value != null && value.IndexOf(test) != -1)
                return (validation ?? new Validation()).AddException(new ArgumentException(paramName, "can not contain '" + test + "'"));
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
}