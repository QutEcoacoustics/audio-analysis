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

    using YamlDotNet.RepresentationModel;

    public class Config
    {
        public Config()
        {
            // no op
        }

        internal Config(TextReader streamReader, string configPath)
        {
            this.ConfigYamlDocument = Yaml.Load(streamReader);
            this.ConfigPath = configPath;
        }

        internal Config(YamlDocument document, string configPath)
        {
            this.ConfigYamlDocument = document;
            this.ConfigPath = configPath;
        }

        public string ConfigPath { get; internal set; }

        public YamlDocument ConfigYamlDocument { get; internal set; }

        public string this[string key] => this.GetStringOrNull(key);

        public string GetString(string path) => this.MustHaveValue(path, this.GetStringOrNull(path));

        public int GetInt(string path) => this.MustHaveValue(path, this.GetIntOrNull(path)).Value;

        public double GetDouble(string path) => this.MustHaveValue(path, this.GetDoubleOrNull(path)).Value;

        public bool GetBool(string path) => this.MustHaveValue(path, this.GetBoolOrNull(path)).Value;

        public TimeSpan GetTimeSpan(string path) => this.MustHaveValue(path, this.GetTimeSpanOrNull(path)).Value;

        public string GetStringOrNull(string path)
        {
            var result = this.TryGetString(path, out var value);

            if (result)
            {
                return value;
            }

            throw new ConfigFileException($"Trying to get value at path {path} failed");
        }

        public int? GetIntOrNull(string path)
        {
            var result = this.TryGetInt(path, out var value);

            if (result)
            {
                return value;
            }

            throw new ConfigFileException($"Trying to get value at path {path} failed");
        }

        public double? GetDoubleOrNull(string path)
        {
            var result = this.TryGetDouble(path, out var value);

            if (result)
            {
                return value;
            }

            throw new ConfigFileException($"Trying to get value at path {path} failed");
        }

        public bool? GetBoolOrNull(string path)
        {
            var result = this.TryGetBool(path, out var value);

            if (result)
            {
                return value;
            }

            throw new ConfigFileException($"Trying to get value at path {path} failed");
        }

        public TimeSpan? GetTimeSpanOrNull(string path)
        {
            var result = this.TryGetTimeSpan(path, out var value);

            if (result)
            {
                return value;
            }

            throw new ConfigFileException($"Trying to get value at path {path} failed");
        }

        public bool TryGetString(string path, out string value)
        {
            value = default(string);
            var node = this.GetNode(path);

            if (node == null)
            {
                return false;
            }

            if (node.Value == null)
            {
                return true;
            }

            value = node.Value;
            return true;
        }

        public bool TryGetInt(string path, out int? value)
        {
            value = default(int);
            var node = this.GetNode(path);

            if (node == null)
            {
                return false;
            }

            if (node.Value == null)
            {
                return true;
            }

            var result = int.TryParse(node.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var scalar);
            value = scalar;
            return result;
        }

        public bool TryGetDouble(string path, out double? value)
        {
            value = default(double);
            var node = this.GetNode(path);

            if (node == null)
            {
                return false;
            }

            if (node.Value == null)
            {
                return true;
            }

            var result = double.TryParse(node.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var scalar);
            value = scalar;
            return result;
        }

        public bool TryGetBool(string path, out bool? value)
        {
            value = default(bool);
            var node = this.GetNode(path);

            if (node == null)
            {
                return false;
            }

            if (node.Value == null)
            {
                return true;
            }

            var result = bool.TryParse(node.Value, out var scalar);
            value = scalar;
            return result;
        }

        public bool TryGetEnum<T>(string path, out T? value)
            where T : struct 
        {
            Contract.Requires(typeof(T).IsSubclassOf(typeof(Enum)), "An Enum must be provided");

            value = null;

            var node = this.GetNode(path);

            if (node == null)
            {
                return false;
            }

            if (node.Value == null)
            {
                return true;
            }

            var result = Enum.TryParse<T>(node.Value, true, out var scalar);
            value = scalar;
            return result;
        }

        public bool TryGetTimeSpan(string path, out TimeSpan? value)
        {
            value = null;

            var node = this.GetNode(path);

            if (node == null)
            {
                return false;
            }

            if (node.Value == null)
            {
                return true;
            }

            var result = TimeSpan.TryParse(node.Value, CultureInfo.InvariantCulture, out var scalar);
            value = scalar;
            return result;
        }

    

        [Obsolete("Any code that depends on this is way out of date!")]
        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();

            if (!(this.ConfigYamlDocument.RootNode is YamlMappingNode root))
            {
                return result;
            }

            this.VisitAll(root, result, string.Empty);

            return result;
        }

        // Recursive!
        private void VisitAll(YamlNode node, Dictionary<string, string> result, string prefix)
        {
            var separator = prefix == string.Empty ? string.Empty : "/";

            switch (node)
            {
                case YamlMappingNode mappingNode:
                    foreach (var child in mappingNode.Children)
                    {
                        var newKey = prefix + separator + ((YamlScalarNode)child.Key);

                        VisitChild(child.Value, newKey);
                    }

                    return;
                case YamlSequenceNode sequenceNode:
                    for (var index = 0; index < sequenceNode.Children.Count; index++)
                    {
                        var child = sequenceNode.Children[index];
                        var newKey = prefix + separator + index;

                        VisitChild(child, newKey);
                    }

                    return;
                default:
                    throw new InvalidOperationException();
            }

            void VisitChild(YamlNode child, string newKey)
            {
                switch (child)
                {
                    case YamlScalarNode scalar:
                        result.Add(newKey, scalar.Value);
                        return;
                    case YamlMappingNode map:
                        // recurse
                        this.VisitAll(map, result, newKey);
                        return;
                    case YamlSequenceNode sequence:
                        // recurse
                        this.VisitAll(sequence, result, newKey);
                        return;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private YamlScalarNode GetNode(string path)
        {
            // path in the form of
            //  /key/list/0/value

            if (path.IsNullOrEmpty())
            {
                return null;
            }

            var fragments = path.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            if (fragments.Length == 0)
            {
                return null;
            }

            YamlNode current = this.ConfigYamlDocument.RootNode;
            for (var i = 0; i < fragments.Length; i++)
            {
                var fragment = fragments[i];
                var isLast = i == fragments.Length - 1;

                if (isLast)
                {
                    if (current is YamlScalarNode node)
                    {
                        return node;
                    }

                    // path not found
                    return null;
                }

                if (current is YamlMappingNode mappingNode)
                {
                    YamlNode newNode = null;
                    foreach (var entry in mappingNode)
                    {
                        if (((YamlScalarNode)entry.Key).Value == fragment)
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
                    return null;
                }

                if (current is YamlSequenceNode sequenceNode)
                {
                    // this part of the fragment must be an index
                    if (int.TryParse(fragment, NumberStyles.None, CultureInfo.InvariantCulture, out var seqIndex))
                    {
                        if (seqIndex > sequenceNode.Children.Count - 1)
                        {
                            // path index out of index range
                            return null;
                        }

                        current = sequenceNode[seqIndex];
                        continue;
                    }

                    // not a valid index
                    return null;
                }
            }

            // A valid value is returned my the yaml scalar node case above.
            // If we get to here something hasn't matched.
            return null;
        }

        [JetBrains.Annotations.NotNull]
        private T MustHaveValue<T>(string path, T value)
        {
            if (value == null)
            {
                throw new ConfigFileException($"The value for {path} was null but a value must be provided.");
            }

            return value;
        }

    }
}
