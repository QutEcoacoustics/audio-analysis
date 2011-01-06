// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LearningDisplayManager.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Learning Display Manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Managers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Learning Display Manager.
    /// </summary>
    public static class LearningDisplayManager
    {
        private const string DataFile = "/UI/Learning/Resources/data.json";

        /// <summary>
        /// Get Animal Data grouped by attributes.
        /// </summary>
        /// <param name="maxItems">
        /// The max Items.
        /// </param>
        /// <param name="startIndex">
        /// The start Index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort Expression.
        /// </param>
        /// <param name="pathToJson">
        /// The path to json.
        /// </param>
        /// <returns>
        /// Data grouped by attributes.
        /// </returns>
        public static IEnumerable<Dictionary<string, string>> GetJsonData(int maxItems, int startIndex, string sortExpression, string pathToJson)
        {
            if (string.IsNullOrEmpty(sortExpression))
            {
                sortExpression = "name";
            }

            bool sortDesc = sortExpression.ToLowerInvariant().Contains("desc");

            ////var json1 = GetJson1(pathToJson);

            // don't know how to get key.
            ////var jsonData1 = json1.Select(d => d.ToDictionary(e => e.???
            
            // this one requires 'sorter' when using OrderBy
            ////var sorter = new AnimalListComparer(true);
            
            var json2 = GetJson2(pathToJson);
            var jsonData = json2.Select(d => d.ToDictionary(e => e.Key, e => e.Value.ToString()));

            jsonData = sortDesc ? jsonData.OrderByDescending(j => j[sortExpression]) : jsonData.OrderBy(j => j[sortExpression]);

            return jsonData.Skip(startIndex).Take(maxItems);
        }

        /// <summary>
        /// Count Json Data.
        /// </summary>
        /// <param name="pathToJson">
        /// The path to json.
        /// </param>
        /// <returns>
        /// Count of items.
        /// </returns>
        public static int CountJsonData(string pathToJson)
        {
            return GetJsonData(int.MaxValue, 0, string.Empty, pathToJson).Count();
        }

        private static IEnumerable<JToken> GetJson1(string pathToJson)
        {
            var json = JObject.Parse(File.ReadAllText(pathToJson));
            var jsonData = json["data"];

            return jsonData.Select(j => j);
        }

        private static IEnumerable<Dictionary<string, object>> GetJson2(string pathToJson)
        {
            var jss = new JavaScriptSerializer();
            var obj = jss.DeserializeObject(File.ReadAllText(pathToJson));
            var jsonData = obj as Dictionary<string, object>;

            if (jsonData == null)
            {
                return null;
            }

            var data = jsonData.First().Value;
            IEnumerable<object> items = data as object[];

            if (items == null)
            {
                return null;
            }

            var attributes = items.Select(z => (Dictionary<string, object>)z);

            return attributes;
        }

        /// <summary>
        /// Comparer for JToken.
        /// </summary>
        private class AnimalListComparer : IComparer<JToken>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AnimalListComparer"/> class.
            /// </summary>
            /// <param name="compareStrings">
            /// true to compare as strings, false to compare as numeric.
            /// </param>
            public AnimalListComparer(bool compareStrings)
            {
                this.CompareStrings = compareStrings;
            }

            /// <summary>
            /// Gets a value indicating whether to Compare as Strings or numeric.
            /// </summary>
            private bool CompareStrings { get; set; }

            #region Implementation of IComparer<JToken>

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <returns>
            /// Value               Condition 
            /// Less than zero      <paramref name="x"/> is less than <paramref name="y"/>.
            /// Zero                <paramref name="x"/> equals <paramref name="y"/>.
            /// Greater than zero   <paramref name="x"/> is greater than <paramref name="y"/>.
            /// </returns>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            public int Compare(JToken x, JToken y)
            {
                return
                    this.CompareStrings ?
                    x.Value<string>().CompareTo(y.Value<string>()) :
                    x.Value<double>().CompareTo(y.Value<double>());
            }

            #endregion
        }
    }
}
