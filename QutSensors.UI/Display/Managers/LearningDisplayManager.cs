using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutSensors.UI.Display.Managers
{
    using System.IO;

    using Newtonsoft.Json.Linq;

    public static class LearningDisplayManager
    {
        private const string DataFile = "/UI/Learning/Resources/data.json";

        /// <summary>
        /// Get Animal List Items.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <param name="pathToJson">
        /// The path to json.
        /// </param>
        /// <returns>
        /// List of items.
        /// </returns>
        public static IEnumerable<JToken> GetAnimalListItems(int maxItems, int startIndex, string sortExpression, string pathToJson)
        {
            var json = JObject.Parse(File.ReadAllText(pathToJson));

            return json["data"];
        }

        /// <summary>
        /// Count Animal List Items.
        /// </summary>
        /// <param name="pathToJson">
        /// The path to json.
        /// </param>
        /// <returns>
        /// Count of items.
        /// </returns>
        public static int CountAnimalListItems(string pathToJson)
        {
            var json = JObject.Parse(File.ReadAllText(pathToJson));

            return json["data"].Count();
        }
    }
}
