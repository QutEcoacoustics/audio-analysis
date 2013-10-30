using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Production
{
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text.RegularExpressions;

    using PowerArgs;

    using ServiceStack.Text;

    /// <summary>
    /// Validates that if the user specifies a value for a property that the value represents a directory that exists
    /// as determined by System.IO.Directory.Exists(directory).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgExistingDirectory : ArgValidator
    {
        private readonly bool createIfNotExists;

        private readonly bool shouldExist;

        public ArgExistingDirectory(bool createIfNotExists = false, bool shouldExist = true)
        {
            Contract.Requires(shouldExist || createIfNotExists == false);

            this.createIfNotExists = createIfNotExists;
            this.shouldExist = shouldExist;
        }

        /// <summary>
        /// Validates that the given directory exists and cleans up the argument so that the application has access
        /// to the full path.
        /// </summary>
        /// <param name="name">the name of the property being populated.  This validator doesn't do anything with it.</param>
        /// <param name="arg">The value specified on the command line</param>
        public override void Validate(string name, ref string arg)
        {
            if (Directory.Exists(arg))
            {
                if (!shouldExist)
                {
                    throw new ValidationArgException("The specified directory ({0}) for argument {1} exists and should not".Format2(arg, name));
                }
            }
            else
            {
                if (createIfNotExists)
                {
                    Directory.CreateDirectory(arg);
                }
                else
                {
                    throw new ValidationArgException(
                        "The specified directory ({0}) for argument {1}{2}, but was not found."
                            .Format2(arg, name, this.shouldExist ? " was expected" : string.Empty),
                        new DirectoryNotFoundException());
                }
            }

            arg = Path.GetFullPath(arg);
        }
    }

    public class ArgNotExistingDirectory : ArgExistingDirectory
    {
        public ArgNotExistingDirectory()
            : base(false, true)
        {
        }
    }

    /// <summary>
    /// Validates that if the user specifies a value for a property that the value represents a file that exists
    /// as determined by System.IO.File.Exists(directory).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgExistingFile : ArgValidator
    {
        private readonly bool createIfNotExists;

        private readonly bool shouldExist;

        public ArgExistingFile(bool createIfNotExists = false, bool shouldExist = true)
        {
            Contract.Requires(shouldExist || createIfNotExists == false);

            this.createIfNotExists = createIfNotExists;
            this.shouldExist = shouldExist;
        }

        public string Extension { get; set; }

        /// <summary>
        /// Validates that the given file exists and cleans up the argument so that the application has access
        /// to the full path.
        /// </summary>
        /// <param name="name">the name of the property being populated.  This validator doesn't do anything with it.</param>
        /// <param name="arg">The value specified on the command line</param>
        public override void Validate(string name, ref string arg)
        {
            if (File.Exists(arg))
            {
                if (!shouldExist)
                {
                    throw new ValidationArgException("The specified file ({0}) for argument {1} exists and should not".Format2(arg, name));
                }
            }
            else
            {
                if (createIfNotExists)
                {
                    File.Create(arg);
                }
                else
                {
                    throw new ValidationArgException(
                        "The specified file ({0}) for argument {1}{2}, but was not found."
                            .Format2(arg, name, this.shouldExist ? " was expected" : string.Empty),
                        new FileNotFoundException());
                }
            }

            arg = Path.GetFullPath(arg);

            if (this.Extension != null)
            {
                var extension = Path.GetExtension(arg);
                if (this.Extension[0] != '.')
                {
                    this.Extension = "." + this.Extension;
                }

                if (extension == null || !string.Equals(extension, this.Extension, StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ValidationArgException(
                        "Expected an input file with an extensions of {1}. Instead got {2} for argument {3}".Format2(
                            this.Extension,
                            arg,
                            name));
                }
            }
        }
    }

    public class ArgNotExistingFile : ArgExistingFile
    {
        public ArgNotExistingFile()
            : base(false, true)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ArgValidFilename : ArgValidator
    {

        public ArgValidFilename()
        {
        }

        bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regFixFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (regFixFileName.IsMatch(testName)) { return false; };


            return true;
        }

        public override void Validate(string name, ref string arg)
        {

            if (!this.IsValidFilename(arg))
            {

                throw new ValidationArgException(
                    "Not a valid filename: '" + arg + "' (for argument " + name + ")");

            }



        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class ArgOneOfThese : ArgValidator
    {
        public string[] ValidItems { get; set; }

        public string ExceptionMessage { get; set; }

        public ArgOneOfThese(params string[] validItems)
        {
            ValidItems = validItems;
        }

        public override void Validate(string name, ref string arg)
        {
            if (ValidItems == null || ValidItems.Length == 0)
            {
                return;
            }

            string s = arg;
            var matches = this.ValidItems.Count(x => string.Equals(x, s, StringComparison.InvariantCultureIgnoreCase));

            if (matches != 1)
            {
                var valids = "{" + ValidItems.Join(", ") + "}";
                throw new ValidationArgException(
                    this.ExceptionMessage
                    + "Supplied value {1} for argument {0} not match any of the allowed values: ".Format2(
                        arg,
                        name,
                        valids));
            }
        }
    }



    public static class CustomRevivers
    {

        [ArgReviver]
        public static DirectoryInfo DirectoryInfoReviver(string property, string value)
        {
            var di = new DirectoryInfo(value);
            return di;
        }

        [ArgReviver]
        public static FileInfo FileInfoReviver(string property, string value)
        {
            var fi = new FileInfo(value);
            return fi;
        }
    }
}
