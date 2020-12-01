// <copyright file="Citation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;

    public record Citation(IReadOnlyCollection<Author> Authors, string Title, string Project, string Uri);

    public record Author(string FirstName, string LastName, string Affiliation, params string[] OtherNames);

    public interface ICiteable
    {
        public IReadOnlyCollection<Citation> Citations => Array.Empty<Citation>();
    }

    public static class Bibliography
    {
        public static Author Anthony => new Author("Anthony", "Truskinger", "QUT Ecoacoustics");

        public static Author Michael => new Author("Michael", "Towsey", "QUT Ecoacoustics");

        public static Author Paul => new Author("Paul", "Roe", "QUT Ecoacoustics");

        public static Author Kristen => new Author("Kristen", "Thompson", "NSW DPI");

        public static Author Brad => new Author("Brad", "Law", "NSW DPI");

        public static IReadOnlyCollection<Author> BuiltByAnthony => new[] { Anthony, Kristen, Michael, Paul, Brad };

        public static IReadOnlyCollection<Author> BuiltByMichael => new[] { Michael, Kristen, Anthony, Paul, Brad };

        public static Citation NswDpiRecognisersProject => new Citation(
            Array.Empty<Author>(),
            string.Empty,
            "Recogniser project with NSW DPI: grant xxxx",
            string.Empty);

        public static Citation QutEcoacousticsProject => new Citation(
            Array.Empty<Author>(),
            string.Empty,
            $"Recognizers devdeloped for various projects by {Meta.GroupName}",
            Meta.Website);
    }
}
