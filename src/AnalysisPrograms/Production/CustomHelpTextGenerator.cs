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
    public class CustomHelpTextGenerator : IHelpTextGenerator
    {
        public CustomHelpTextGenerator()
        {
        }

        public Dictionary<string, string> EnvironmentOptions { get; set; } = null;

        /// <inheritdoc />
        public void Generate(CommandLineApplication application, TextWriter output)
        {
            var nameAndVersion = application.GetFullNameAndVersion();
            if (!string.IsNullOrEmpty(nameAndVersion))
            {
                output.WriteLine(nameAndVersion);
                output.WriteLine();
            }

            output.Write("Usage:");
            var stack = new Stack<string>();
            for (var cmd = application; cmd != null; cmd = cmd.Parent)
            {
                stack.Push(cmd.Name);
            }

            while (stack.Count > 0)
            {
                output.Write(' ');
                output.Write(stack.Pop());
            }

            var arguments = application.Arguments.Where(a => a.ShowInHelpText).ToList();
            var options = application.GetOptions().Where(o => o.ShowInHelpText).ToList();
            var commands = application.Commands.Where(c => c.ShowInHelpText).ToList();

            if (arguments.Any())
            {
                output.Write(" [arguments]");
            }

            if (options.Any())
            {
                output.Write(" [options]");
            }

            if (commands.Any())
            {
                output.Write(" [command]");
            }

            if (application.AllowArgumentSeparator)
            {
                output.Write(" [[--] <arg>...]");
            }

            output.WriteLine();

            if (this.EnvironmentOptions.Any())
            {
                output.WriteLine();
                output.WriteLine("Environment variables: ");
                output.Write(this.FormatEnvironmentVariables());
            }

            if (arguments.Any())
            {
                output.WriteLine();
                output.WriteLine("Arguments:");
                var maxArgLen = arguments.Max(a => a.Name.Length);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxArgLen + 2);
                foreach (var arg in arguments)
                {
                    output.Write(outputFormat, arg.Name, arg.Description);
                    output.WriteLine();
                }
            }

            if (options.Any())
            {
                output.WriteLine();
                output.WriteLine("Options:");
                var maxOptLen = options.Max(o => o.Template?.Length ?? 0);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxOptLen + 2);
                foreach (var opt in options)
                {
                    output.Write(outputFormat, opt.Template, opt.Description);
                    output.WriteLine();
                }
            }

            if (commands.Any())
            {
                this.FormatCommands(output, commands);

                if (application.OptionHelp != null)
                {
                    output.WriteLine();
                    output.WriteLine($"Use \"{application.Name} [command] --{application.OptionHelp.LongName}\" for more information about a command.");
                }
            }

            output.Write(application.ExtendedHelpText);
        }

        public void FormatCommands(TextWriter output, List<CommandLineApplication> commands)
        {
            output.WriteLine();
            output.WriteLine("Commands:");
            var maxCmdLen = commands.Max(c => c.Name?.Length ?? 0);
            var outputFormat = $"  {{0, -{maxCmdLen + 2}}}{{1}}";
            foreach (var cmd in commands.OrderBy(c => c.Name))
            {
                output.Write(outputFormat, cmd.Name, cmd.Description);
                output.WriteLine();
            }
        }

        public string FormatEnvironmentVariables()
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