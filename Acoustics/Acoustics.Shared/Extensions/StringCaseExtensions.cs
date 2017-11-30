﻿// <copyright file="StringCaseExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

/*
 * Copyright Vadim Orekhov 2017
 * Sourced from https://github.com/vad3x/case-extensions
 * License: MIT
 */

using System;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace System
{
    public static class StringExtensions
    {
        private static readonly char[] Delimeters = { ' ', '-', '_' };

        public static string ToPascalCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SymbolsPipe(
                source,
                '\0',
                (s, i) => new[] { char.ToUpperInvariant(s) });
        }

        public static string ToCamelCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SymbolsPipe(
                source,
                '\0',
                (s, disableFrontDelimeter) =>
                {
                    if (disableFrontDelimeter)
                    {
                        return new[] { char.ToLowerInvariant(s) };
                    }

                    return new[] { char.ToUpperInvariant(s) };
                });
        }

        public static string ToKebabCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SymbolsPipe(
                source,
                '-',
                (s, disableFrontDelimeter) =>
                {
                    if (disableFrontDelimeter)
                    {
                        return new[] { char.ToLowerInvariant(s) };
                    }

                    return new[] { '-', char.ToLowerInvariant(s) };
                });
        }

        public static string ToSnakeCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SymbolsPipe(
                source,
                '_',
                (s, disableFrontDelimeter) =>
                {
                    if (disableFrontDelimeter)
                    {
                        return new[] { char.ToLowerInvariant(s) };
                    }

                    return new[] { '_', char.ToLowerInvariant(s) };
                });
        }

        public static string ToTrainCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SymbolsPipe(
                source,
                '-',
                (s, disableFrontDelimeter) =>
                {
                    if (disableFrontDelimeter)
                    {
                        return new[] { char.ToUpperInvariant(s) };
                    }

                    return new[] { '-', char.ToUpperInvariant(s) };
                });
        }

        private static string SymbolsPipe(
            string source,
            char mainDelimeter,
            Func<char, bool, char[]> newWordSymbolHandler)
        {
            var builder = new StringBuilder();

            bool nextSymbolStartsNewWord = true;
            bool disableFrontDelimeter = true;
            for (var i = 0; i < source.Length; i++)
            {
                var symbol = source[i];
                if (Delimeters.Contains(symbol))
                {
                    if (symbol == mainDelimeter)
                    {
                        builder.Append(symbol);
                        disableFrontDelimeter = true;
                    }

                    nextSymbolStartsNewWord = true;
                }
                else if (!char.IsLetterOrDigit(symbol))
                {
                    builder.Append(symbol);
                    disableFrontDelimeter = true;
                    nextSymbolStartsNewWord = true;
                }
                else
                {
                    if (nextSymbolStartsNewWord || char.IsUpper(symbol))
                    {
                        builder.Append(newWordSymbolHandler(symbol, disableFrontDelimeter));
                        disableFrontDelimeter = false;
                        nextSymbolStartsNewWord = false;
                    }
                    else
                    {
                        builder.Append(symbol);
                    }
                }
            }

            return builder.ToString();
        }
    }
}