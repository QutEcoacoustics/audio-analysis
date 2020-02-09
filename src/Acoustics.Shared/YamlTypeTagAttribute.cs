// <copyright file="YamlTypeTagAttribute.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;

    /// <summary>
    /// Registers a type and a tag name that will be emitted in a YAML document when
    /// serializing, and will allow for unambiguous parsing when deserializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class YamlTypeTagAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YamlTypeTagAttribute"/> class.
        /// Registers a type and a tag name that will be emitted in a YAML document when
        /// serializing, and will allow for unambiguous parsing when deserializing.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="name">The tag name to use.</param>
        public YamlTypeTagAttribute(Type type, string name = null)
        {
            this.Type = type ?? throw new ArgumentNullException(nameof(type));

            // yml type tags must be prefixed with an "!" character.
            if (name == null)
            {
                this.Name = "!" + type.Name;
            }
            else
            {
                if (name[0] != '!')
                {
                    this.Name = "!" + name;
                }
            }
        }

        /// <summary>
        /// Gets the name associated with this tag mapping.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type associated with this tag mapping.
        /// </summary>
        public Type Type { get; }
    }
}