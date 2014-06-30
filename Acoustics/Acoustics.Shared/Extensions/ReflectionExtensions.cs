// ReSharper disable once CheckNamespace
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the ReflectionExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ReflectionExtensions
    {
        internal static bool HasAttr<T>(this MemberInfo info)
        {
            return info.GetCustomAttributes(typeof(T), true).Length > 0;
        }


        internal static T Attr<T>(this MemberInfo info)
        {
            if (info.HasAttr<T>())
            {
                return (T)info.GetCustomAttributes(typeof(T), true)[0];
            }
            else
            {
                return default(T);
            }
        }

        public static bool IsInstanceOfGenericType(this Type genericType, object instance, out Type matchedType)
        {
            Type type = instance.GetType();
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                {
                    matchedType = type;
                    return true;
                }
                type = type.BaseType;
            }

            matchedType = null;
            return false;
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(
            this Type type,
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(
                    string.Format("Expression '{0}' refers to a method, not a property.", propertyLambda.ToString()));
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(
                    string.Format("Expression '{0}' refers to a field, not a property.", propertyLambda.ToString()));
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(
                    string.Format(
                        "Expresion '{0}' refers to a property that is not from type {1}.",
                        propertyLambda.ToString(),
                        type));

            return propInfo;
        }


        public static Dictionary<string, Func<TBase, TType>> GetGetters<TBase, TType>()
        {
            Type thisType = typeof(TBase);

            var props = thisType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new Dictionary<string, Func<TBase, TType>>(props.Length);

            foreach (var propertyInfo in props)
            {

                if (propertyInfo.PropertyType != typeof(TType))
                {
                    continue;
                }

                var methodInfo = propertyInfo.GetGetMethod();

                var getDelegate = (Func<TBase, TType>)Delegate.CreateDelegate(typeof(Func<TBase, double[]>), methodInfo);

                var name = propertyInfo.Name;

                result.Add(name, getDelegate);
            }

            return result;
        } 
    }
}
