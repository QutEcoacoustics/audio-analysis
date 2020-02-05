// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionsSystem.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Utilities type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System
{
    using Acoustics.Shared;
    using Collections.Generic;
    using Collections.Specialized;
    using ComponentModel;
    using Diagnostics;
    using IO;
    using Linq;
    using Linq.Expressions;
    using Runtime.InteropServices;
    using Runtime.Serialization;
    using Runtime.Serialization.Formatters.Binary;
    using Text;
    using Text.RegularExpressions;
    using Web;

    public static class ExtensionsGeneral
    {
        /// <summary>Deserialise byte array to object.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <returns>Deserialised object.
        /// </returns>
        public static object BinaryDeserialize(this byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Deserialise byte array to object.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <param name="binder">
        /// The binder.
        /// </param>
        /// <returns>
        /// Deserialised object.
        /// </returns>
        public static object BinaryDeserialize(this byte[] bytes, SerializationBinder binder)
        {
            if (bytes == null || binder == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter { Binder = binder };

            using (var stream = new MemoryStream(bytes))
            {
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Convert an object to it's binary serialised form.
        /// </summary>
        /// <param name="o">
        /// Object to serialise.
        /// </param>
        /// <returns>Serialised object.
        /// </returns>
        public static byte[] BinarySerialize(this object o)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, o);
                stream.Flush();
                return stream.ToArray();
            }
        }

        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">
        /// Unicode Byte Array to be converted to String.
        /// </param>
        /// <returns>
        /// String converted from Unicode Byte Array.
        /// </returns>
        public static string Utf8ByteArrayToString(this byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return constructedString;
        }

        /// <summary>
        /// Converts the string representation of a Guid to its Guid
        /// equivalent. A return value indicates whether the operation
        /// succeeded.
        /// </summary>
        /// <param name="s">
        /// A string containing a Guid to convert.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <value>
        /// <see langword="true"/> if <paramref name="s"/> was converted
        /// successfully; otherwise, <see langword="false"/>.
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <pararef name="s"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The try parse guid.
        /// </returns>
        public static bool TryParseGuidRegex(this string s, out Guid value)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            var format =
                new Regex(
                    "^[A-Fa-f0-9]{32}$|" + "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
                    "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
            var match = format.Match(s);
            if (match != null && match.Success)
            {
                value = new Guid(s);
                return true;
            }

            value = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Get description for enum.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// Description text from DescriptionAttribute.
        /// </returns>
        public static string ToDescriptionString(this Enum value)
        {
            var enumString = value.ToString();
            var enumType = value.GetType();
            var enumFieldInfo = enumType.GetField(enumString);

            if (enumFieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Length > 0)
            {
                var description = new StringBuilder();

                attributes.Aggregate(
                    description,
                    (agg, da) =>
                    da != null && !string.IsNullOrEmpty(da.Description) ? agg.Append(da.Description + ", ") : agg);

                var desc = description.ToString();
                desc = desc.Trim(',', ' ');
                return desc;
            }

            return string.Empty;
        }

        /// <summary>
        /// Convert a byte count to human-readable format.
        /// </summary>
        /// <param name="byteCount">
        /// Number of bytes.
        /// </param>
        /// <returns>
        /// Byte count in human-readable format.
        /// </returns>
        public static string ToByteDisplay(this long byteCount)
        {
            var size = "0 b";

            if (byteCount >= 1099511627776)
            {
                size = $"{byteCount / 1099511627776F:##.#}" + " tb";
            }
            else if (byteCount >= 1073741824)
            {
                size = $"{byteCount / 1073741824F:##.#}" + " gb";
            }
            else if (byteCount >= 1048576)
            {
                size = $"{byteCount / 1048576F:##.#}" + " mb";
            }
            else if (byteCount >= 1024)
            {
                size = $"{byteCount / 1024F:##.#}" + " kb";
            }
            else if (byteCount >= 1)
            {
                size = $"{(float)byteCount:##.#}" + " b";
            }

            return size;
        }

        /// <summary>
        /// Gets ExecutingDirectory.
        /// </summary>
        public static string ExecutingDirectory => Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Check if a dictionary has a value for a key.
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary.
        /// </param>
        /// <param name="key">
        /// The dictionary key.
        /// </param>
        /// <typeparam name="TKey">
        /// Type of key.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// Type of Value.
        /// </typeparam>
        /// <returns>
        /// True if <paramref name="dictionary"/> contains and has a value for <paramref name="key"/>, otherwise false.
        /// </returns>
        public static bool HasValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, string key)
        {
            var dic =
                dictionary.Select(kvp => new KeyValuePair<string, object>(kvp.Key.ToString(), kvp.Value)).ToDictionary(
                    kvp => kvp.Key, kvp => kvp.Value);
            return dic.ContainsKey(key) && dic[key] != null && dic[key].ToString().Length > 0;
        }

        /// <summary>Convert NameValueCollection to a Dictionary.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <returns>Dictionary from NameValueCollection.
        /// </returns>
        public static Dictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            return collection.Cast<string>().ToDictionary(key => key, key => collection[key]);
        }

        public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(
       this IEnumerable<KeyValuePair<TKey, TValue>> l)
        {
            SortedDictionary<TKey, TValue> result = new SortedDictionary<TKey, TValue>();
            foreach (var e in l)
            {
                result[e.Key] = e.Value;
            }

            return result;
        }

        public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            var i = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return i;
                }

                i++;
            }

            return -1;

            throw new ArgumentOutOfRangeException();
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> values)
        {
            if (values != null)
            {
                foreach (var item in values)
                {
                    list.Add(item);
                }
            }
        }

        /// <summary>
        /// Paging for LINQ to SQL.
        /// </summary>
        /// <typeparam name="TSource">IQueryable of 'object type' to page.</typeparam>
        /// <param name="source">IQueryable to page.</param>
        /// <param name="page">Page number (begins at 1).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns></returns>
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Paging for LINQ
        /// </summary>
        /// <typeparam name="TSource">IEnumerable of 'object type' to page.</typeparam>
        /// <param name="source">IEnumerable to page.</param>
        /// <param name="page">Page number (begins at 1).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns></returns>
        public static IEnumerable<TSource> Page<TSource>(this IEnumerable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Page IQueryable object using startIndex and number of items.
        /// </summary>
        /// <typeparam name="T">IQueryable of 'object type' to page.</typeparam>
        /// <param name="value">IQueryable to page.</param>
        /// <param name="startIndex">Index of first item to return.</param>
        /// <param name="length">number of items to return.</param>
        /// <returns></returns>
        public static IQueryable<T> PageByIndex<T>(this IQueryable<T> value, int? startIndex, int? length)
        {
            if (startIndex != null)
            {
                value = value.Skip(startIndex.Value);
            }

            if (length != null)
            {
                value = value.Take(length.Value);
            }

            return value;
        }

        public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return source.MaxOrDefault<TSource, TSource>(s => s);
        }

        public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
        {
            return source.MaxOrDefault<TSource, TSource>(s => s, defaultValue);
        }

        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.MaxOrDefault(selector, default);
        }

        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defaultValue)
        {
            return source.Any() ? source.Max(selector) : defaultValue;
        }

        public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return source.MinOrDefault<TSource, TSource>(s => s);
        }

        public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
        {
            return source.MinOrDefault<TSource, TSource>(s => s, defaultValue);
        }

        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.MinOrDefault(selector, default);
        }

        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defaultValue)
        {
            return source.Any() ? source.Min(selector) : defaultValue;
        }

        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }

    public static class ProcessExtensions
    {
#if DEBUG

        /// <summary>
        /// A utility class to determine a process parent.
        /// http://stackoverflow.com/a/3346055
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ParentProcessUtilities
        {
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;

            internal IntPtr PebBaseAddress;

            internal IntPtr Reserved2_0;

            internal IntPtr Reserved2_1;

            internal IntPtr UniqueProcessId;

            internal IntPtr InheritedFromUniqueProcessId;

            [DllImport("ntdll.dll")]
            private static extern int NtQueryInformationProcess(
                IntPtr processHandle,
                int processInformationClass,
                ref ParentProcessUtilities processInformation,
                int processInformationLength,
                out int returnLength);

            /// <summary>
            /// Gets the parent process of the current process.
            /// </summary>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess()
            {
                return GetParentProcess(Process.GetCurrentProcess().Handle);
            }

            /// <summary>
            /// Gets the parent process of specified process.
            /// </summary>
            /// <param name="id">The process id.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(int id)
            {
                Process process = Process.GetProcessById(id);
                return GetParentProcess(process.Handle);
            }

            /// <summary>
            /// Gets the parent process of a specified process.
            /// </summary>
            /// <param name="handle">The process handle.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(IntPtr handle)
            {
                ParentProcessUtilities pbi = new ParentProcessUtilities();
                int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out var returnLength);
                if (status != 0)
                {
                    throw new Win32Exception(status);
                }

                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (ArgumentException)
                {
                    // not found
                    return null;
                }
            }
        }
#endif
    }
}