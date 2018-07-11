// <copyright file="CustomHelpTextGenerator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//https://raw.githubusercontent.com/natemcmaster/CommandLineUtils/d1b6b5b91bcffd41d8e848b270f288baf72ae955/src/CommandLineUtils/HelpText/DefaultHelpTextGenerator.cs

namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.CommandLineUtils.HelpText;

    /// <summary>
    /// A default implementation of help text generation.
    /// </summary>
    public class CustomHelpTextGenerator : DefaultHelpTextGenerator
    {
        public CustomHelpTextGenerator()
        {
        }

        public Dictionary<string, string> EnvironmentOptions { get; set; } = null;

        public void FormatCommands(TextWriter output, List<CommandLineApplication> commands)
        {
            output.WriteLine();
            output.WriteLine("Commands:");
            var maxCmdLen = commands.Max(c => c.Name?.Length ?? 0);
            var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxCmdLen + 2);

            var newLineWithMessagePadding = Environment.NewLine + new string(' ', maxCmdLen + 4);

            foreach (var cmd in commands.OrderBy(c => c.Name))
            {
                var message = string.Format(outputFormat, cmd.Name, cmd.Description);
                message = message.Replace(Environment.NewLine, newLineWithMessagePadding);

                output.Write(message);
                output.WriteLine();
            }
        }

        protected override void GenerateFooter(CommandLineApplication application, TextWriter output)
        {
            if (this.EnvironmentOptions.Any())
            {
                output.Write("\nEnvironment variables: \n" + this.FormatEnvironmentVariables());
            }

            base.GenerateFooter(application, output);

        }

        protected override void GenerateHeader(CommandLineApplication application, TextWriter output)
        {
            // separate our header from the rest of the content
            output.WriteLine();
            base.GenerateHeader(application, output);
        }

        private string FormatEnvironmentVariables()
        {
            var result = string.Empty;
            int varLength = this.EnvironmentOptions.Keys.Max(s => s.Length) + 2;
            var format = $"  {{0, -{varLength}}}{{1}}\n";
            foreach (var envOption in this.EnvironmentOptions)
            {
                result += string.Format(format, envOption.Key, envOption.Value);
            }

            return result;
        }
    }
}