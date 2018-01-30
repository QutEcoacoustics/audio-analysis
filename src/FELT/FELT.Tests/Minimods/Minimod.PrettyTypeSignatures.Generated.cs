using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Minimod.PrettyTypeSignatures
{
    /// <summary>
    /// Minimod.PrettyTypeSignatures, Version 1.1.0
    /// <para>A minimod with reflection extensions, printing nice type and type member names.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal static class PrettyTypeSignaturesMinimod
    {

        public static string GetPrettyName(this Type type)
        {
            return getDisplayName(type, null);
        }

        public static string GetPrettyName(this MethodBase method)
        {
            if (method == null) throw new ArgumentNullException("method");

            var sb = new StringBuilder();

            // dynamic methods don't have a declaring type
            if (method.DeclaringType != null)
            {
                sb.Append(method.DeclaringType.GetPrettyName());
                sb.Append(".");
            }

            sb.Append(method.Name);

            if (method.IsGenericMethod)
            {
                sb.Append("<");
                sb.Append(string.Join(",", method.GetGenericArguments().Select(t => t.GetPrettyName()).ToArray()));
                sb.Append(">");
            }

            sb.Append("(");

            string[] parameters = method.GetParameters()
                .Select(p => (p.IsOut ? "out " : string.Empty) + p.ParameterType.GetPrettyName() + " " + p.Name)
                .ToArray();

            sb.Append(String.Join(", ", parameters));
            sb.Append(")");
            if (method is MethodInfo)
            {
                sb.Append(" : " + ((MethodInfo)method).ReturnType.GetPrettyName());
            }

            return sb.ToString();
        }

        private static string getDisplayName(Type type, Type[] genericArguments)
        {
            if (type == null) throw new ArgumentNullException("type");

            var sb = new StringBuilder();

            int declaringGenArgsCount = 0;
            if (type.DeclaringType != null && !type.IsGenericParameter)
            {
                if (type.DeclaringType.IsGenericType)
                {
                    declaringGenArgsCount = type.DeclaringType.GetGenericArguments().Length;
                    Type[] leadingGenericArguments = type.GetGenericArguments()
                        .Take(declaringGenArgsCount)
                        .ToArray();
                    sb.Append(getDisplayName(type.DeclaringType, leadingGenericArguments));
                }
                else
                {
                    sb.Append(type.DeclaringType.GetPrettyName());
                }

                sb.Append("+");
            }

            Type[] trailingGenericArguments = genericArguments
                                              ?? type.GetGenericArguments()
                                                     .Skip(declaringGenArgsCount)
                                                     .ToArray();

            var simpleTypeName = csharpNameOrNull(type) ?? type.Name;
            if (checkIfAnonymousType(type))
            {
                simpleTypeName = "Anonymous";
            }
            else if (trailingGenericArguments.Length > 0)
            {
                simpleTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            }

            sb.Append(simpleTypeName);

            if (trailingGenericArguments.Length > 0)
            {
                IEnumerable<string> displayNames = trailingGenericArguments.Select(t => t.GetPrettyName());

                sb.Append("<");
                sb.Append(string.Join(",", displayNames.ToArray()));
                sb.Append(">");
            }

            return sb.ToString();
        }

        private static bool checkIfAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        private static string csharpNameOrNull(Type type)
        {
            var map = new Dictionary<Type, string>
                          {
                              {typeof (bool), "bool"},
                              {typeof (byte), "byte"},
                              {typeof (sbyte), "sbyte"},
                              {typeof (char), "char"},
                              {typeof (decimal), "decimal"},
                              {typeof (double), "double"},
                              {typeof (float), "float"},
                              {typeof (int), "int"},
                              {typeof (uint), "uint"},
                              {typeof (long), "long"},
                              {typeof (ulong), "ulong"},
                              {typeof (object), "object"},
                              {typeof (short), "short"},
                              {typeof (ushort), "ushort"},
                              {typeof (string), "string"},
                              {typeof (void), "void"}
                          };

            string shortName;
            map.TryGetValue(type, out shortName);
            return shortName;
        }

        public static string GetPrettyName(this StackFrame frame)
        {
            if (frame == null) throw new ArgumentNullException("frame");

            var sb = new StringBuilder();

            MethodBase method = frame.GetMethod();
            sb.Append(method.GetPrettyName());

            string fileName = frame.GetFileName();
            if (fileName != null)
            {
                sb.Append(" in ");
                sb.Append(fileName);
                sb.Append(":");
                sb.Append(frame.GetFileLineNumber());
                sb.Append(":");
                sb.Append(frame.GetFileColumnNumber());
            }

            return sb.ToString();
        }
    }
}
