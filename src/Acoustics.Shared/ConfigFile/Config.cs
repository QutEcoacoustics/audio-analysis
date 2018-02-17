// <copyright file="Config.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    using Acoustics.Shared.Contracts;

    public class Config
    {
        private static readonly Convert<bool> BoolConverter =
            (string value, out bool convertedValue) => bool.TryParse(value, out convertedValue);

        private static readonly Convert<double> DoubleConverter = (string value, out double convertedValue) =>
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out convertedValue);

        private static readonly Convert<int> IntConverter = (string value, out int convertedValue) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out convertedValue);

        private static readonly Convert<string> StringConverter = (string s, out string convertedValue) =>
            {
                convertedValue = s;
                return true;
            };

        private static readonly Convert<TimeSpan> TimeSpanConverter = (string value, out TimeSpan convertedValue) =>
            TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out convertedValue);

        public Config()
        {
            // no op
        }

        internal Config(TextReader streamReader, string configPath)
        {
            this.Initialize(streamReader, configPath);
        }

        internal Config(string configPath)
        {
            using (var fileStream = File.OpenText(configPath))
            {
                this.Initialize(fileStream, configPath);
            }
        }

        private delegate bool Convert<T>(string value, out T convertedValue);

        public event Action<Config> Loaded;

        private enum MatchType
        {
            NotFound,
            NullValue,
            Found,
        }

        public string ConfigPath { get; internal set; }

        public object GenericConfig { get; internal set; }

        public string this[string key] => this.GetStringOrNull(key);

        public string GetString(string path) => this.Get(path, StringConverter);

        public int GetInt(string path) => this.Get(path, IntConverter);

        public double GetDouble(string path) => this.Get(path, DoubleConverter);

        public bool GetBool(string path) => this.Get(path, BoolConverter);

        public TimeSpan GetTimeSpan(string path) => this.Get(path, TimeSpanConverter);

        public string GetStringOrNull(string path)
        {
            this.TryGetString(path, out var value);
            return value;
        }

        public int? GetIntOrNull(string path)
        {
            var result = this.TryGetInt(path, out var value);
            return result ? value : default(int?);
        }

        public double? GetDoubleOrNull(string path)
        {
            var result = this.TryGetDouble(path, out var value);
            return result ? value : default(double?);
        }

        public bool? GetBoolOrNull(string path)
        {
            var result = this.TryGetBool(path, out var value);
            return result ? value : default(bool?);
        }

        public TimeSpan? GetTimeSpanOrNull(string path)
        {
            var result = this.TryGetTimeSpan(path, out var value);
            return result ? value : default(TimeSpan?);
        }

        public bool TryGetString(string path, out string value) => this.TryGet(path, out value, StringConverter);

        public bool TryGetInt(string path, out int value) => this.TryGet(path, out value, IntConverter);

        public bool TryGetDouble(string path, out double value) => this.TryGet(path, out value, DoubleConverter);

        public bool TryGetBool(string path, out bool value) => this.TryGet(path, out value, BoolConverter);

        public bool TryGetEnum<T>(string path, out T value)
            where T : struct
        {
            Contract.Requires(typeof(T).IsSubclassOf(typeof(Enum)), "An Enum must be provided");

            return this.TryGet(path, out value, GetEnumConverter<T>());
        }

        public bool TryGetTimeSpan(string path, out TimeSpan value) => this.TryGet(path, out value, TimeSpanConverter);

        [Obsolete("Any code that depends on this is way out of date!")]
        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();

            this.VisitAll(this.GenericConfig, result, string.Empty);

            return result;
        }

        internal void InvokeLoaded()
        {
            this.Loaded?.Invoke(this);
        }

        private static Convert<T> GetEnumConverter<T>()
            where T : struct
        {
            return (string value, out T convertedValue) => Enum.TryParse(value, true, out convertedValue);
        }

        private void Initialize(TextReader streamReader, string configPath)
        {
            this.GenericConfig = Yaml.Deserialize<object>(streamReader);
            this.ConfigPath = configPath;
        }

        // Recursive!
        private void VisitAll(object node, Dictionary<string, string> result, string prefix)
        {
            var separator = prefix == string.Empty ? string.Empty : "/";

            switch (node)
            {
                case Dictionary<object, object> mappingNode:
                    foreach (var child in mappingNode)
                    {
                        var newKey = prefix + separator + child.Key;

                        VisitChild(child.Value, newKey);
                    }

                    return;
                case List<object> sequenceNode:
                    for (var index = 0; index < sequenceNode.Count; index++)
                    {
                        var child = sequenceNode[index];
                        var newKey = prefix + separator + index;

                        VisitChild(child, newKey);
                    }

                    return;
                default:
                    // this case should only happen when the root of the document is a sclar (for some reason)
                    VisitChild(node, prefix);
                    break;
            }

            void VisitChild(object child, string newKey)
            {
                switch (child)
                {
                    case Dictionary<object, object> map:
                        // recurse
                        this.VisitAll(map, result, newKey);
                        return;
                    case List<object> sequence:
                        // recurse
                        this.VisitAll(sequence, result, newKey);
                        return;
                    case object scalar:
                        result.Add(newKey, scalar.ToString());
                        return;
                    case null:
                        result.Add(newKey, null);
                        return;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private bool TryGet<T>(string path, out T value, Convert<T> converter)
        {
            value = default;

            var (match, foundValue) = this.GetNode(path);
            switch (match)
            {
                case MatchType.NotFound:
                    return false;
                case MatchType.NullValue:
                    return false;
                case MatchType.Found:
                    return converter(foundValue, out value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private T Get<T>(string path, Convert<T> converter)
        {
            var (match, foundValue) = this.GetNode(path);
            switch (match)
            {
                case MatchType.NotFound:
                    throw new ConfigFileException($"The value for {path} was not found and a value is required.");
                case MatchType.NullValue:
                    throw new ConfigFileException($"The value for {path} was null or empty and a value is required.");
                case MatchType.Found:
                    var success = converter(foundValue, out var value);
                    if (success)
                    {
                        return value;
                    }

                    throw new ConfigFileException(
                        $"The value for {path} was not the expected type:"
                        + $" {foundValue} could not by interpreted as {typeof(T).Name}.");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private (MatchType Match, string Value) GetNode(string path)
        {
            // path in the form of
            //  /key/list/0/value

            if (path.IsNullOrEmpty())
            {
                return (MatchType.NotFound, null);
            }

            var fragments = path.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            if (fragments.Length == 0)
            {
                return (MatchType.NotFound, null);
            }

            var current = this.GenericConfig;
            foreach (var fragment in fragments)
            {
                if (current is Dictionary<object, object> mappingNode)
                {
                    object newNode = null;
                    foreach (var entry in mappingNode)
                    {
                        if ((string)entry.Key == fragment)
                        {
                            newNode = entry.Value;
                            break;
                        }
                    }

                    if (newNode != null)
                    {
                        current = newNode;
                        continue;
                    }

                    // fragment of path not found
                    return (MatchType.NotFound, null);
                }

                if (current is List<object> sequenceNode)
                {
                    // this part of the fragment must be an index
                    if (int.TryParse(fragment, NumberStyles.None, CultureInfo.InvariantCulture, out var seqIndex))
                    {
                        if (seqIndex > sequenceNode.Count - 1)
                        {
                            return (MatchType.NotFound, null);
                        }

                        current = sequenceNode[seqIndex];
                        continue;
                    }

                    // not a valid index
                    return (MatchType.NotFound, null);
                }

                // if the current value is not map or list we're in an invalid state
                // or the path requested is too long and we've hit a scalar
                return (MatchType.NotFound, null);
            }

            // returned value is either a scalar (we found it) or a map or list (we didn't find it)
            switch (current)
            {
                case null:
                    return (MatchType.NullValue, null);
                case Dictionary<object, object> _:
                case List<object> _:
                    // path not found
                    // If we get to here something hasn't matched.
                    return (MatchType.NotFound, null);
                case string s:
                    return (MatchType.Found, s);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}