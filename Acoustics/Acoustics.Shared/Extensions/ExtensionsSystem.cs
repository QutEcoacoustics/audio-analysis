// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Utilities type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Linq;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

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
        /// Fills a given array with a supplied value.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the given Array.
        /// </typeparam>
        /// <param name="array">
        /// The array to manipulate.
        /// </param>
        /// <param name="value">
        /// The Value to insert.
        /// </param>
        /// <returns>
        /// Returns a reference to the manipulated array.
        /// </returns>
        public static T[] Fill<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }

            return array;
        }

        /// <summary>
        /// Fills a given array with a supplied value.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the given Array.
        /// </typeparam>
        /// <param name="array">
        /// The array to manipulate.
        /// </param>
        /// <param name="value">
        /// The Value to insert.
        /// </param>
        /// <returns>
        /// Returns a reference to the manipulated array.
        /// </returns>
        public static T[,] Fill<T>(this T[,] array, T value)
        {
            for (int i = 0; i < array.GetUpperBound(0); i++)
            {
                for (int j = 0; j < array.GetUpperBound(1); j++)
                {
                    array[i, j] = value;
                }
            }

            return array;
        }

        /// <summary>
        /// Debug function used to print an array to console (Debug.WriteLine()).
        /// </summary>
        /// <typeparam name="T">The type of the input array.</typeparam>
        /// <param name="array">The array to print.</param>
        /// <returns>The same array that was input.</returns>
        public static T[] Print<T>(this T[] array)
        {
            foreach (T foo in array)
            {
                if (foo is ValueType || foo != null)
                {
                    Debug.WriteLine(foo.ToString());
                }
                else
                {
                    Debug.WriteLine('\u0000');
                }
            }

            return array;
        }

        /// <summary>
        /// Compares two arrays, matching each element in order using the default Equals method for the array type T.
        /// </summary>
        /// <typeparam name="T">The common type of each array.</typeparam>
        /// <param name="arr1">The first array to compare.</param>
        /// <param name="arr2">The second array to compare.</param>
        /// <returns>True: If each element matches; otherwise: False.</returns>
        public static bool Compare<T>(this T[] arr1, T[] arr2)
        {
            if (arr1.Length != arr2.Length)
            {
                return false;
            }

            for (int i = 0; i < arr1.Length; i++)
            {
                T foo = arr1[i];
                T bar = arr2[i];

                if (!(foo is ValueType) && foo == null)
                {
                    if (bar == null)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (!foo.Equals(bar))
                {
                    return false;
                }
            }

            return true;
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
            var attributes =
                enumFieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Length > 0)
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
                size = String.Format("{0:##.#}", (float)byteCount / (float)1099511627776) + " tb";
            }
            else if (byteCount >= 1073741824)
            {
                size = String.Format("{0:##.#}", (float)byteCount / (float)1073741824) + " gb";
            }
            else if (byteCount >= 1048576)
            {
                size = String.Format("{0:##.#}", (float)byteCount / (float)1048576) + " mb";
            }
            else if (byteCount >= 1024)
            {
                size = String.Format("{0:##.#}", (float)byteCount / (float)1024) + " kb";
            }
            else if (byteCount >= 1)
            {
                size = String.Format("{0:##.#}", (float)byteCount) + " b";
            }

            return size;
        }

        /// <summary>Serialised object as Binary.
        /// </summary>
        /// <param name="linqBinary">
        /// The linq binary.
        /// </param>
        /// <returns>Deserialised object.
        /// </returns>
        public static object BinaryDeserialize(this Binary linqBinary)
        {
            return linqBinary.ToArray().BinaryDeserialize();
        }

        /// <summary>Serialised object as Binary.
        /// </summary>
        /// <param name="linqBinary">
        /// The linq binary.
        /// </param>
        /// <param name="binder">
        /// The binder.
        /// </param>
        /// <returns>Deserialised object.
        /// </returns>
        public static object BinaryDeserialize(this Binary linqBinary, SerializationBinder binder)
        {
            return linqBinary.ToArray().BinaryDeserialize(binder);
        }

        /// <summary>
        /// Get querystring representation of dictionary.
        /// Keys will be lower case.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="performUrlEncoding">
        /// The perform url encoding.
        /// </param>
        /// <returns>
        /// Query string.
        /// </returns>
        public static string ToUrlParameterString(this Dictionary<string, string> values, bool performUrlEncoding)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            if (performUrlEncoding)
            {
                return values.Keys
                    .Select(k => HttpUtility.UrlEncode(k.ToLowerInvariant()) + "=" + HttpUtility.UrlEncode(values[k]))
                    .Aggregate((a, b) => a + "&" + b);
            }

            return values.Keys.Select(k => k.ToLowerInvariant() + "=" + values[k]).Aggregate((a, b) => a + "&" + b);
        }

#if ! SILVERLIGHT
        /// <summary>
        /// Gets Date Format for Readings.
        /// </summary>
        public static string ReadingsDateFormat
        {
            get
            {
                var retVal = System.Configuration.ConfigurationManager.AppSettings["ReadingsDateFormat"];
                if (string.IsNullOrEmpty(retVal))
                {
                    retVal = "yyyyMMddTHHmmss";
                }

                return retVal;
            }
        }

        /// <summary>
        /// Gets ExecutingDirectory.
        /// </summary>
        public static string ExecutingDirectory
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }

        /// <summary>
        ///   Creates a parent DataTable within the DataSet using distinct rows from
        ///   an existing "source" DataTable, based on the column(s) specified.
        ///   The source table then becomes the "child" table of the newly created
        ///   parent. A DataRelation is also created between the parent table and the
        ///   child table, using the column(s) specified.
        /// </summary>
        /// <param name="sourceTable">
        ///    The source DataTable, which must be within a DataSet.  
        ///    This source table will become the "child" table.
        /// </param>
        /// <param name="parentTableName">
        ///    This name will be assigned to the parent table once it is created.
        /// </param>
        /// <param name="relationColumns">
        ///    Specify the columns used to relate the parent table to the child table.
        /// </param>
        /// <param name="additionalColumns">
        ///    Any additional column(s) in the source table that will be extracted to the
        ///    parent table.  These columns will be removed from the source table.
        /// </param>
        /// <param name="relationName">
        ///    The name of the relation that will be created between the parent and
        ///    the child table.
        /// </param>
        /// <remarks>from: http://weblogs.sqlteam.com/jeffs/archive/2007/11/02/parent-child-datatable-nested-repeaters.aspx </remarks>
        /// <exception cref="Exception">The source DataTable must be contained in a DataSet</exception>
        public static void AddParentTable(
            DataTable sourceTable,
            string parentTableName,
            string[] relationColumns,
            string[] additionalColumns,
            string relationName)
        {
            var dataSet = sourceTable.DataSet;

            if (dataSet == null)
            {
                throw new Exception("The source DataTable must be contained in a DataSet");
            }

            // generate the set of columns to use to create the Parent table:
            var cols = new string[relationColumns.Length + additionalColumns.Length];
            relationColumns.CopyTo(cols, 0);
            additionalColumns.CopyTo(cols, relationColumns.Length);

            // create the parent table, copying unique rows from the Child table:
            var parent = sourceTable.DefaultView.ToTable(parentTableName, true, cols);

            // add the parent table to the DataSet:
            dataSet.Tables.Add(parent);

            // remove the additional columns from the child table that were
            // copied to the parent table:
            foreach (var s in additionalColumns)
            {
                sourceTable.Columns.Remove(s);
            }

            // create the relation between the new parent table and the child table:
            var parentColumns = new DataColumn[relationColumns.Length];
            var childColumns = new DataColumn[relationColumns.Length];

            for (var i = 0; i < relationColumns.Length; i++)
            {
                parentColumns[i] = parent.Columns[relationColumns[i]];
                childColumns[i] = sourceTable.Columns[relationColumns[i]];
            }

            // And, finally, add the relation to the parent table:
            parent.ChildRelations.Add(relationName, parentColumns, childColumns);
        }

        public static void SaveDoubleArrayToCSV(double[,] arr, string outputDest)
        {
            outputDest = Path.Combine(Path.GetDirectoryName(outputDest), Path.GetFileNameWithoutExtension(outputDest) + DateTime.Now.ToString("yyMMdd-hhmmss") + Path.GetExtension(outputDest));
            using (var fs = new StreamWriter(outputDest, false))
            {
                for (int i = 0; i < arr.GetLength(0); i++)
                {
                    for (int j = 0; j < arr.GetLength(1); j++)
                    {
                        fs.Write(arr[i, j] + ",");
                    }
                    fs.Write("\r\n");
                }
            }
        }

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

        public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            var i = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                    return i;
                i++;
            }
            throw new ArgumentOutOfRangeException();
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> values)
        {
            if (values != null)
                foreach (var item in values)
                    list.Add(item);
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
                value = value.Skip(startIndex.Value);
            if (length != null)
                value = value.Take(length.Value);
            return value;
        }

        public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source) { return source.MaxOrDefault<TSource, TSource>(s => s); }
        public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) { return source.MaxOrDefault<TSource, TSource>(s => s, defaultValue); }
        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) { return source.MaxOrDefault(selector, default(TResult)); }
        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defaultValue) { return source.Any() ? source.Max(selector) : defaultValue; }

        public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source) { return source.MinOrDefault<TSource, TSource>(s => s); }
        public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) { return source.MinOrDefault<TSource, TSource>(s => s, defaultValue); }
        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) { return source.MinOrDefault(selector, default(TResult)); }
        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defaultValue) { return source.Any() ? source.Min(selector) : defaultValue; }



        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

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
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
#endif
    }
}

#if ! SILVERLIGHT
namespace PInvoke
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>PInvoke extras.
    /// </summary>
    public class ObjBase
    {
        /// <summary>
        /// This function converts a string generated by the StringFromCLSID function back into the original class identifier.
        /// </summary>
        /// <param name="sz">String that represents the class identifier.</param>
        /// <param name="clsid">On return will contain the class identifier.</param>
        /// <returns>
        /// Positive or zero if class identifier was obtained successfully.
        /// Negative if the call failed.
        /// </returns>
        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = true)]
        public static extern int CLSIDFromString(string sz, out Guid clsid);
    }
}
#endif