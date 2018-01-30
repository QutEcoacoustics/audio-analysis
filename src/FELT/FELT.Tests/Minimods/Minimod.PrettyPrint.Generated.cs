using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Minimod.PrettyDateAndTime;
using Minimod.PrettyText;
using Minimod.PrettyTypeSignatures;

namespace Minimod.PrettyPrint
{
    /// <summary>
    /// <h1>Minimod.PrettyPrint, Version 1.0.0, Copyright © Lars Corneliussen 2011</h1>
    /// <para>Creates nice textual representations of any objects. Mostly meant for debug/informational output.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal static class PrettyPrintMinimod
    {
        #region Settings

        public static Settings DefaultSettings;

        static PrettyPrintMinimod()
        {
            var settings = new Settings();

            // integration with PrettyTypeSignatures
            settings.RegisterFormatterFor<Type>(PrettyTypeSignaturesMinimod.GetPrettyName);
            settings.RegisterFormatterFor<MethodBase>(PrettyTypeSignaturesMinimod.GetPrettyName);
            settings.RegisterFormatterFor<StackFrame>(PrettyTypeSignaturesMinimod.GetPrettyName);

            // integration with PrettyDateAndTime
            settings.RegisterFormatterFor<DateTime>(PrettyDateAndTimeMinimod.GetPrettyString);
            settings.RegisterFormatterFor<DateTimeOffset>(PrettyDateAndTimeMinimod.GetPrettyString);
            settings.RegisterFormatterFor<TimeSpan>(PrettyDateAndTimeMinimod.GetPrettyString);

            settings.RegisterFormatterFor<Guid>(formatter);

            // decimal is not a simple type?
            settings.RegisterFormatterFor<decimal>(d => d + "m");

            GenericFormatter.Register(settings);
            FileSystemInfoFormatter.Register(settings);

            settings.OmitNullMembers(true);

            DefaultSettings = settings;
        }

        private static string formatter(Guid guid)
        {
            if (guid == Guid.Empty) return "<Guid.Empty>";
            return guid.ToString("D").ToUpperInvariant();
        }

        public static Settings CreateCustomSettings()
        {
            return new Settings(DefaultSettings);
        }

        public delegate string CustomMemberFormatter(object memberValue, string memberName, object memberOwner, Settings settings);
        public delegate string CustomMemberFormatter<TOwner, TProp>(TProp memberValue, string memberName, TOwner memberOwner, Settings settings);

        public delegate string CustomMemberErrorFormatter(Exception memberError, string memberName, object memberOwner, Settings settings);
        public delegate string CustomMemberErrorFormatter<TOwner>(Exception memberError, string memberName, TOwner memberOwner, Settings settings);

        public class Settings
        {
            private readonly Settings _inner;


            private Dictionary<Type, Func<object, string>> _customFormatters =
                new Dictionary<Type, Func<object, string>>();

            private Dictionary<Type, string> _customPrependedPropNames =
                new Dictionary<Type, string>();

            /// <summary>
            /// if a member formatter returns null, the member will be ignored
            /// </summary>
            private Dictionary<Type, Dictionary<string, CustomMemberFormatter>> _customMemberFormatters =
                new Dictionary<Type, Dictionary<string, CustomMemberFormatter>>();

            /// <summary>
            /// if a member error formatter returns null, the member error will be ignored
            /// </summary>
            private Dictionary<Type, Dictionary<string, CustomMemberErrorFormatter>> _customMemberErrorFormatters =
                new Dictionary<Type, Dictionary<string, CustomMemberErrorFormatter>>();

            private bool? _prefersMultiline;

            public bool? PrefersMultiline
            {
                get { return _prefersMultiline ?? (_inner != null ? _inner.PrefersMultiline : null); }
                private set { _prefersMultiline = value; }
            }

            private bool? _omitsNullMembers;

            public bool? OmitsNullMembers
            {
                get { return _omitsNullMembers ?? (_inner != null ? _inner.OmitsNullMembers : null); }
                private set { _omitsNullMembers = value; }
            }

            public Settings()
            {
            }

            public Settings(Settings inner)
                : this()
            {
                _inner = inner;
            }

            public Settings RegisterToStringFor<T>()
            {
                return RegisterToStringFor(typeof(T));
            }

            public Settings RegisterToStringFor(Type type)
            {
                _customFormatters.Add(type, o => o.ToString());
                return this;
            }

            public Settings RegisterFormatterFor(Type type, Func<object, string> formatter)
            {
                _customFormatters.Add(type, formatter);
                return this;
            }

            public Settings RegisterFormatterFor<T>(Func<T, string> formatter)
            {
                _customFormatters.Add(typeof(T), o => formatter((T)o));
                return this;
            }

            #region Custom Member Formatting
            public Settings IgnoreMember<T, Prop>(Expression<Func<T, Prop>> member)
            {
                return IgnoreMember(typeof(T), getMemberName(member));
            }

            public Settings IgnoreMember(Type type, string memberName)
            {
                return RegisterMemberFormatterFor(type, memberName, (memberValue, memberName2, memberOwner, settings) => null);
            }

            public Settings RegisterMemberFormatterFor<T, Prop>(Expression<Func<T, Prop>> member,
                                                                Func<Prop, string> formatter)
            {
                return RegisterMemberFormatterFor(typeof(T), getMemberName(member),
                                                  (memberValue, memberName, memberOwner, settings) => formatter((Prop)memberValue));
            }

            public Settings RegisterMemberFormatterFor<TOwner, TProp>(Expression<Func<TOwner, TProp>> member,
                                                                  CustomMemberFormatter<TOwner, TProp> formatter)
            {
                return RegisterMemberFormatterFor(typeof(TOwner), getMemberName(member),
                    (memberValue, memberName, memberOwner, settings) => formatter((TProp)memberValue, memberName, (TOwner)memberOwner, settings));
            }

            public Settings RegisterMemberFormatterFor(Type type, string memberName, Func<object, string> formatter)
            {
                return RegisterMemberFormatterFor(type, memberName, (memberValue, memberName2, memberOwner, settings) => formatter(memberValue));
            }

            public Settings RegisterMemberFormatterFor(Type type, string memberName, CustomMemberFormatter formatter)
            {
                if (type == null) throw new ArgumentNullException("type");
                if (memberName == null) throw new ArgumentNullException("memberName");
                if (formatter == null) throw new ArgumentNullException("formatter");

                Dictionary<string, CustomMemberFormatter> propFormatters;
                if (!_customMemberFormatters.TryGetValue(type, out propFormatters))
                {
                    _customMemberFormatters[type] = (propFormatters = new Dictionary<string, CustomMemberFormatter>());
                }

                propFormatters.Add(memberName, formatter);

                return this;
            }

            public Settings RegisterCustomPrependMember<T, Prop>(Expression<Func<T, Prop>> member)
            {
                return RegisterCustomPrependMember(typeof(T), getMemberName(member));
            }

            public Settings RegisterCustomPrependMember(Type type, string memberName)
            {
                if (type == null) throw new ArgumentNullException("type");
                if (memberName == null) throw new ArgumentNullException("memberName");

                _customPrependedPropNames.Add(type, memberName);

                return this;
            }

            private static string getMemberName(LambdaExpression expression)
            {
                if (expression == null) throw new ArgumentNullException("expression");

                if (expression.Body is MemberExpression)
                {
                    return ((MemberExpression)expression.Body).Member.Name;
                }

                throw new ArgumentException(
                    "Unsupported expression type: " + expression.Body.GetType(), "expression");
            }

            internal bool tryGetCustomMemberFormatter(object anyObject, string memberName,
                                                      out CustomMemberFormatter formatter)
            {
                formatter = null;

                var propsByType = _customMemberFormatters
                    .Where(byType => byType.Key.IsAssignableFrom(anyObject.GetType()))
                    .Select(byType => byType.Value)
                    .SelectMany(dict => dict)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);


                if (propsByType != null && propsByType.TryGetValue(memberName, out formatter))
                {
                    return true;
                }

                if (_inner == null)
                {
                    return false;
                }

                if (_inner.tryGetCustomMemberFormatter(anyObject, memberName, out formatter))
                {
                    return true;
                }

                return false;
            }
            #endregion

            #region Custom MemberError Formatting
            public Settings IgnoreMemberError<TOwner, TMember>(Expression<Func<TOwner, TMember>> member)
            {
                return IgnoreMemberError(typeof(TOwner), getMemberName(member));
            }

            public Settings IgnoreMemberError(Type type, string memberName)
            {
                return RegisterMemberErrorFormatterFor(type, memberName, (memberAccessError, memberName2, memberOwner, settings) => null);
            }

            public Settings RegisterMemberErrorFormatterFor<TOwner, TMember>(Expression<Func<TOwner, TMember>> member,
                                                                Func<Exception, string> formatter)
            {
                return RegisterMemberErrorFormatterFor(typeof(TOwner), getMemberName(member),
                                                  (memberAccessError, memberName, memberOwner, settings) => formatter(memberAccessError));
            }

            public Settings RegisterMemberErrorFormatterFor<TOwner, TMember>(Expression<Func<TOwner, TMember>> member,
                                                                  CustomMemberErrorFormatter<TOwner> formatter)
            {
                return RegisterMemberErrorFormatterFor(typeof(TOwner), getMemberName(member),
                    (memberError, memberName, memberOwner, settings) => formatter(memberError, memberName, (TOwner)memberOwner, settings));
            }

            public Settings RegisterMemberErrorFormatterFor(Type type, string memberName, Func<object, string> formatter)
            {
                return RegisterMemberErrorFormatterFor(type, memberName, (memberAccessError, memberName2, memberOwner, settings) => formatter(memberAccessError));
            }

            public Settings RegisterMemberErrorFormatterFor(Type type, string memberName, CustomMemberErrorFormatter formatter)
            {
                if (type == null) throw new ArgumentNullException("type");
                if (memberName == null) throw new ArgumentNullException("memberName");
                if (formatter == null) throw new ArgumentNullException("formatter");

                Dictionary<string, CustomMemberErrorFormatter> propFormatters;
                if (!_customMemberErrorFormatters.TryGetValue(type, out propFormatters))
                {
                    _customMemberErrorFormatters[type] = (propFormatters = new Dictionary<string, CustomMemberErrorFormatter>());
                }

                propFormatters.Add(memberName, formatter);

                return this;
            }

            public Settings RegisterCustomPrependMemberError<T, Prop>(Expression<Func<T, Prop>> member)
            {
                return RegisterCustomPrependMemberError(typeof(T), getMemberName(member));
            }

            public Settings RegisterCustomPrependMemberError(Type type, string memberName)
            {
                if (type == null) throw new ArgumentNullException("type");
                if (memberName == null) throw new ArgumentNullException("memberName");

                _customPrependedPropNames.Add(type, memberName);

                return this;
            }

            internal bool tryGetCustomMemberErrorFormatter(object anyObject, string memberName,
                                                      out CustomMemberErrorFormatter formatter)
            {
                formatter = null;

                var propsByType = _customMemberErrorFormatters
                    .Where(byType => byType.Key.IsAssignableFrom(anyObject.GetType()))
                    .Select(byType => byType.Value)
                    .SelectMany(dict => dict)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);


                if (propsByType != null && propsByType.TryGetValue(memberName, out formatter))
                {
                    return true;
                }

                if (_inner == null)
                {
                    return false;
                }

                if (_inner.tryGetCustomMemberErrorFormatter(anyObject, memberName, out formatter))
                {
                    return true;
                }

                return false;
            }
            #endregion

            public Func<object, string> GetCustomFormatter(object anyObject)
            {
                return _customFormatters
                           .Where(t => t.Key.IsAssignableFrom(anyObject.GetType()))
                           .Select(kv => kv.Value)
                           .FirstOrDefault()
                       ?? (_inner != null ? _inner.GetCustomFormatter(anyObject) : null);
            }


            public string GetCustomPrependedPropName(Type actualType)
            {
                return _customPrependedPropNames
                           .Where(t => t.Key.IsAssignableFrom(actualType))
                           .Select(kv => kv.Value)
                           .FirstOrDefault()
                       ?? (_inner != null ? _inner.GetCustomPrependedPropName(actualType) : null);
            }

            public Settings PreferMultiline(bool multiline)
            {
                PrefersMultiline = multiline;
                return this;
            }

            public Settings OmitNullMembers(bool omit)
            {
                OmitsNullMembers = omit;
                return this;
            }
        }

        #endregion

        #region Public Extensions

        public static string PrettyPrint<T>(this T anyObject)
        {
            return anyObject.PrettyPrint(typeof(T), DefaultSettings);
        }

        public static string PrettyPrint<T>(this T anyObject, Settings settings)
        {
            return anyObject.PrettyPrint(typeof(T), settings);
        }

        public static string PrettyPrint<T>(this T anyObject, Func<Settings, Settings> customize)
        {
            return anyObject.PrettyPrint(typeof(T), customize(CreateCustomSettings()));
        }

        public static string PrettyPrint(this object anyObject, Type declaredType)
        {
            return anyObject.PrettyPrint(declaredType, DefaultSettings);
        }

        public static string PrettyPrint(this object anyObject, Type declaredType, Func<Settings, Settings> customize)
        {
            return anyObject.PrettyPrint(declaredType, customize(DefaultSettings));
        }

        public static string PrettyPrint(this object anyObject, Type declaredType, Settings settings)
        {
            if (anyObject == null)
            {
                return "<null" + (declaredType != typeof(object) ? ", " + declaredType.GetPrettyName() : "") + ">";
            }

            var formatter = settings.GetCustomFormatter(anyObject);
            if (formatter != null)
            {
                return formatter(anyObject);
            }

            var actualType = anyObject.GetType();

            if (anyObject is string)
            {
                var s = (string)anyObject;
                return s == String.Empty
                           ? "<String.Empty>"
                           : s;
            }

            if (actualType.IsPrimitive)
            {
                return anyObject.ToString();
            }

            if (anyObject is IEnumerable)
            {
                return enumerable(anyObject as IEnumerable, declaredType, settings);
            }

            return GenericFormatter.Format(actualType, anyObject, settings);
        }

        #endregion

        private static string enumerable(IEnumerable objects, Type declaredType, Settings settings)
        {
            string[] items = objects.Cast<object>().Select(_ => _.PrettyPrint(settings)).ToArray();

            if (settings.PrefersMultiline
                ?? (
                       (items.Length > 1 && items.Any(i => i.Length > 30))
                       || (items.Length > 10)
                       || items.Any(i => i.Contains(Environment.NewLine)))
                )
            {
                return "["
                       + Environment.NewLine
                       + String.Join("," + Environment.NewLine, items.Select(i => i.IndentLinesBy(2)).ToArray())
                       + Environment.NewLine
                       + "]";
            }

            return "[" + String.Join(", ", items) + "]";
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

        /// <summary>
        /// Tries to figure out a nice format on its own.
        /// </summary>
        public class GenericFormatter
        {
            public static void Register(Settings settings)
            {
                settings.RegisterFormatterFor<MemberDetails>(Format);
            }

            public class MemberDetails
            {
                public string Name { get; set; }

                public Type Type { get; set; }

                public object Value { get; set; }

                public string Pretty { get; set; }
            }

            public static string Format(Type actualType, object anyObject, Settings settings)
            {
                if (actualType == null) throw new ArgumentNullException("actualType");
                if (anyObject == null) throw new ArgumentNullException("anyObject");
                if (settings == null) throw new ArgumentNullException("settings");

                var members =
                    findAndFormatMembers(anyObject, settings, actualType)
                        .Where(m => m.Value != null || (settings.OmitsNullMembers ?? false))
                        .ToArray();

                string result;

                if (mayFormatKeyValuePairs(members, out result))
                    return result;

                return formatMemberList(actualType, members, settings);
            }

            private static bool mayFormatKeyValuePairs(MemberDetails[] members, out string format)
            {
                var keyMember = members.Where(_ => _.Name == "Key").FirstOrDefault();
                var valueMember = members.Where(_ => _.Name == "Value").FirstOrDefault();

                if (keyMember != null && valueMember != null && members.Length == 2)
                {
                    {
                        format = keyMember.Pretty + " => " +
                                 valueMember.Pretty;
                        return true;
                    }
                }
                format = null;
                return false;
            }

            private static IEnumerable<MemberDetails> findAndFormatMembers(object anyObject, Settings settings,
                                                                           Type actualType)
            {
                var properties =
                    from prop in actualType.GetMembers().OfType<PropertyInfo>()
                    where !prop.GetGetMethod().IsStatic && prop.GetIndexParameters().Length == 0
                    let safeValue = getValueOrException(anyObject, () => prop.GetValue(anyObject, new object[0]))
                    select
                        new
                            {
                                name = prop.Name,
                                type = prop.PropertyType,
                                value = safeValue.Value,
                                error = safeValue.Error
                            };

                var fields =
                    from prop in actualType.GetMembers().OfType<FieldInfo>()
                    where !prop.IsStatic
                    let safeValue = getValueOrException(anyObject, () => prop.GetValue(anyObject))
                    select new
                    {
                        name = prop.Name,
                        type = prop.FieldType,
                        value = safeValue.Value,
                        error = safeValue.Error
                    };

                foreach (var m in fields.Union(properties))
                {
                    string pretty = null;

                    CustomMemberFormatter propFormatter;
                    bool hasCustomFormatter = settings.tryGetCustomMemberFormatter(anyObject, m.name,
                                                                                   out propFormatter);

                    CustomMemberErrorFormatter propErrorFormatter;
                    bool hasCustomErrorFormatter = settings.tryGetCustomMemberErrorFormatter(anyObject, m.name,
                                                                                   out propErrorFormatter);
                    if (m.error == null)
                    {
                        pretty = hasCustomFormatter
                                     ? propFormatter(m.value, m.name, anyObject, settings)
                                     : m.value.PrettyPrint(m.type, settings);
                    }
                    else
                    {
                        pretty = hasCustomErrorFormatter
                            ? propErrorFormatter(m.error, m.name, anyObject, settings)
                            : new { Name = "threw ", Exception = m.error }.PrettyPrint(settings);
                    }

                    if (pretty != null)
                    {
                        // if formatter returned null, the member should be ignored
                        yield return new MemberDetails { Name = m.name, Type = m.type, Value = m.value, Pretty = pretty };
                    }
                }
            }

            private class PropertyExecution
            {
                public object Value { get; set; }
                public Exception Error { get; set; }
            }

            private static PropertyExecution getValueOrException(object anyObject, Func<object> getValue)
            {
                PropertyExecution result = new PropertyExecution();
                try
                {
                    result.Value = getValue();
                }
                catch (Exception e)
                {
                    result.Error = e;
                }
                return result;
            }

            private static string formatMemberList(Type actualType, MemberDetails[] members, Settings settings)
            {
                var prependedPropName = settings.GetCustomPrependedPropName(actualType) ?? "Name";
                var prependedProp = members.Where(_ => _.Name == prependedPropName).FirstOrDefault();
                var contentProps = members.Where(_ => _.Name != prependedPropName).ToArray();

                List<string> parts = new List<string>();
                if (prependedProp != null && prependedProp.Value != null)
                {
                    parts.Add(prependedProp.Pretty);
                }

                string typeName = checkIfAnonymousType(actualType)
                                  && contentProps.Length > 0
                                      ? null
                                      : actualType.GetPrettyName();

                if (typeName != null)
                {
                    parts.Add("<" + actualType.GetPrettyName() + ">");
                }

                if (contentProps.Length != 0)
                {
                    var printedProps = contentProps
                        .Select(prop => prop.PrettyPrint());


                    if (settings.PrefersMultiline
                        ?? (printedProps.Any(s => s.Length > 30
                            || s.Contains(Environment.NewLine))))
                    {
                        StringBuilder contentBuilder = new StringBuilder();
                        contentBuilder.AppendLine("{");
                        contentBuilder.AppendLine(printedProps.JoinLines());
                        contentBuilder.Append("}");
                        parts.Add(contentBuilder.ToString());
                    }
                    else
                    {
                        parts.Add("{ " + String.Join(", ", printedProps.Select(s => s.Trim()).ToArray()) + " }");
                    }
                }

                return String.Join(" ", parts.Select(s => s.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToArray());
            }

            public static string Format(MemberDetails prop)
            {
                string value = prop.Pretty;

                if (prop.Type == typeof(string)
                    && value.Contains(Environment.NewLine))
                {
                    value = Environment.NewLine + value.IndentLinesBy(2);
                }

                return (prop.Name + " = " + value).IndentLinesBy(2);
            }
        }


        /// <summary>
        /// Tries to figure out a nice format on its own.
        /// </summary>
        public class FileSystemInfoFormatter
        {
            public static void Register(Settings settings)
            {
                //settings.RegisterMemberFormatterFor((DirectoryInfo fs) => fs.Parent, p => p == null ? null : p.FullName);
                settings.RegisterCustomPrependMember((DirectoryInfo fs) => fs.FullName);
                settings.IgnoreMember((DirectoryInfo fs) => fs.Name);
                settings.IgnoreMember((DirectoryInfo fs) => fs.Parent);
                settings.IgnoreMember((DirectoryInfo fs) => fs.Root);

                settings.RegisterMemberFormatterFor((FileInfo fs) => fs.Directory, dir => dir.FullName);
                settings.IgnoreMember((FileInfo fs) => fs.DirectoryName);
                settings.IgnoreMember((FileInfo fs) => fs.IsReadOnly);
                settings.IgnoreMember((FileInfo fs) => fs.FullName);

                settings.RegisterMemberFormatterFor((FileInfo fs) => fs.Length, ignoreIfFileOrDirDoesNotExist);
                settings.IgnoreMemberError((FileInfo fs) => fs.Length);


                settings.RegisterMemberFormatterFor((FileSystemInfo fs) => fs.CreationTime, ignoreIfFileOrDirDoesNotExist);
                settings.RegisterMemberFormatterFor((FileSystemInfo fs) => fs.LastWriteTime, ignoreIfFileOrDirDoesNotExist);
                settings.RegisterMemberFormatterFor((FileSystemInfo fs) => fs.LastAccessTime, ignoreIfFileOrDirDoesNotExist);

                settings.IgnoreMember((FileSystemInfo fs) => fs.Extension);
                settings.IgnoreMember((FileSystemInfo fs) => fs.CreationTimeUtc);
                settings.IgnoreMember((FileSystemInfo fs) => fs.LastAccessTimeUtc);
                settings.IgnoreMember((FileSystemInfo fs) => fs.LastWriteTimeUtc);
                settings.IgnoreMember((FileSystemInfo fs) => fs.Attributes);
            }

            private static string ignoreIfFileOrDirDoesNotExist<TMember, TFsType>(TMember memberValue, string memberName, TFsType memberOwner, Settings settings)
                where TFsType : FileSystemInfo
            {
                return memberOwner.Exists ? memberValue.PrettyPrint(settings) : null;
            }
        }
    }
}