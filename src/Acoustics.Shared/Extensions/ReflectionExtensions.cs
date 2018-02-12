// ReSharper disable once CheckNamespace
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ReflectionExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System
{
    using System;
    using Collections.Generic;
    using Linq.Expressions;
    using Reflection;

    public static class ReflectionExtensions
    {
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
            {
                throw new ArgumentException(
                    string.Format(
                        "Expresion '{0}' refers to a property that is not from type {1}.",
                        propertyLambda.ToString(),
                        type));
            }

            return propInfo;
        }

        /// <summary>
        /// Gets the property from the expression.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>The <see cref="PropertyInfo"/> for the expression.</returns>
        public static PropertyInfo GetProperty<TModel>(this Expression<Func<TModel, object>> expression)
        {
            var member = GetMemberExpression(expression).Member;
            var property = member as PropertyInfo;
            if (property == null)
            {
                throw new Exception(string.Format("'{0}' is not a property.", member.Name));
            }

            return property;
        }

        /// <summary>
        /// Gets the member expression.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private static MemberExpression GetMemberExpression<TModel, T>(this Expression<Func<TModel, T>> expression)
        {
            // This method was taken from FluentNHibernate.Utils.ReflectionHelper.cs and modified.
            // http://fluentnhibernate.org/

            MemberExpression memberExpression = null;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression.Body;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException("Not a member access", "expression");
            }

            return memberExpression;
        }

        public static bool PropertyInfoMetaDataEquality(this PropertyInfo propertyInfoA, PropertyInfo propertyInfoB)
        {
            return propertyInfoA.Module == propertyInfoB.Module
                   && propertyInfoA.MetadataToken == propertyInfoB.MetadataToken;
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
        public static string[] GetGetterNames<TBase>()
        {
            Type thisType = typeof(TBase);

            var props = thisType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new string[props.Length];

            props.ForEach((p,i) => result[i] = p.Name);
            
            return result;
        }

        public static Dictionary<string, Action<TBase, TType>> GetSetters<TBase, TType>()
        {
            Type thisType = typeof(TBase);

            var props = thisType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new Dictionary<string, Action<TBase, TType>>(props.Length);

            foreach (var propertyInfo in props)
            {

                if (propertyInfo.PropertyType != typeof(TType))
                {
                    continue;
                }

                var methodInfo = propertyInfo.GetSetMethod();

                var setDelegate = (Action<TBase, TType>)Delegate.CreateDelegate(typeof(Action<TBase, double[]>), methodInfo);

                var name = propertyInfo.Name;

                result.Add(name, setDelegate);
            }

            return result;
        }

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
    }
}
