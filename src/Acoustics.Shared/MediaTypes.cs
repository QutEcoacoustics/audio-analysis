// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaTypes.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using SixLabors.ImageSharp.Formats;

    /// <summary>
    /// The media type ext group.
    /// </summary>
    public class MediaTypeExtGroup
    {
        /// <summary>
        /// Gets or sets Extension.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets Group.
        /// </summary>
        public MediaTypeGroup Group { get; set; }

        /// <summary>
        /// Gets or sets MediaType.
        /// </summary>
        public string MediaType { get; set; }
    }

    /// <summary>
    /// Media type group.
    /// </summary>
    public enum MediaTypeGroup
    {
        /// <summary>
        /// Media type group is not known.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Media type group can be any group (excluding Any).
        /// </summary>
        Any = 1,

        /// <summary>
        /// Media type group is audio.
        /// </summary>
        Audio = 2,

        /// <summary>
        /// Media type group is image.
        /// </summary>
        Image = 3,

        /// <summary>
        /// Media type group is video.
        /// </summary>
        Video = 4,

        /// <summary>
        /// Media type group is text.
        /// </summary>
        Text = 5,
    }

    /// <summary>
    /// Utility class for handling media types and file extensions.
    /// </summary>
    /// <remarks>
    /// see http://en.wikipedia.org/wiki/Internet_media_type#Type_image for more info.
    /// </remarks>
    public static class MediaTypes
    {
        ////public const string MediaTypeBasic = "audio/basic"; //: mulaw audio at 8 kHz, 1 channel; Defined in RFC 2046

        ////public const string MediaTypeL24 = "audio/L24"; //: 24bit Linear PCM audio at 8-48kHz, 1-N channels; Defined in RFC 3190

        public const string Ext3gp = "3gp";
        public const string Ext3gp1 = "3gpp";

        public const string Ext3g2 = "3g2";
        public const string Ext3g21 = "3gp2";
        public const string Ext3g22 = "3gpp2";

        /// <summary>
        /// The ext asf.
        /// </summary>
        public const string ExtAsf = "asf";

        /// <summary>
        /// The ext aac.
        /// </summary>
        public const string ExtAac = "aac";

        /// <summary>
        /// The ext cmd.
        /// </summary>
        public const string ExtCmd = "cmd";

        /// <summary>
        /// The ext cmd 1.
        /// </summary>
        public const string ExtCmd1 = "bat";

        /// <summary>
        /// The ext css.
        /// </summary>
        public const string ExtCss = "css";

        /// <summary>
        /// The ext csv.
        /// </summary>
        public const string ExtCsv = "csv";

        /// <summary>
        /// The ext gif.
        /// </summary>
        public const string ExtGif = "gif";

        /// <summary>
        /// The ext bmp.
        /// </summary>
        public const string ExtBmp = "bmp";

        /// <summary>
        /// The ext html.
        /// </summary>
        public const string ExtHtml = "html";

        /// <summary>
        /// The ext html 1.
        /// </summary>
        public const string ExtHtml1 = "htm";

        /// <summary>
        /// The ext ico.
        /// </summary>
        public const string ExtIco = "ico";

        /// <summary>
        /// The ext jpeg.
        /// </summary>
        public const string ExtJpeg = "jpg";

        /// <summary>
        /// The ext jpeg 1.
        /// </summary>
        public const string ExtJpeg1 = "jpeg";

        /// <summary>
        /// The ext json.
        /// </summary>
        public const string ExtJson = "json";

        /// <summary>
        /// The ext m4a.
        /// </summary>
        public const string ExtM4a = "m4a";

        /// <summary>
        /// The ext mj2.
        /// </summary>
        public const string ExtMj2 = "mj2";

        /// <summary>
        /// The ext mov.
        /// </summary>
        public const string ExtMov = "mov";

        /// <summary>
        /// The ext mp 3.
        /// </summary>
        public const string ExtMp3 = "mp3";

        /// <summary>
        /// The ext mp4.
        /// </summary>
        public const string ExtMp4 = "mp4";

        /// <summary>
        /// The ext mp4a.
        /// </summary>
        public const string ExtMp4a = "mp4a";

        /// <summary>
        /// The ext mpg.
        /// </summary>
        public const string ExtMpg = "mpg";

        /// <summary>
        /// The ext ogg.
        /// </summary>
        public const string ExtOgg = "ogg";

        /// <summary>
        /// The ext ogg.
        /// </summary>
        public const string ExtFlac = "flac";

        /// <summary>
        /// The ext ogg audio.
        /// </summary>
        public const string ExtOggAudio = "oga";

        /// <summary>
        /// The ext pcm.
        /// </summary>
        public const string ExtPcm = "pcm";

        /// <summary>
        /// The ext pjpeg.
        /// </summary>
        public const string ExtPjpeg = "pjpeg";

        /// <summary>
        /// The ext png.
        /// </summary>
        public const string ExtPng = "png";

        /// <summary>
        /// The ext ra.
        /// </summary>
        public const string ExtRa = "ra";

        /// <summary>
        /// The ext raw.
        /// </summary>
        public const string ExtRaw = "raw";

        /// <summary>
        /// The ext rm.
        /// </summary>
        public const string ExtRm = "rm";

        /// <summary>
        /// The ext svg.
        /// </summary>
        public const string ExtSvg = "svg";

        /// <summary>
        /// The ext text plain.
        /// </summary>
        public const string ExtTextPlain = "txt";

        /// <summary>
        /// The ext tiff.
        /// </summary>
        public const string ExtTiff = "tiff";

        /// <summary>
        /// The ext unknown.
        /// </summary>
        public const string ExtUnknown = "unknown";

        /// <summary>
        /// The ext vcard.
        /// </summary>
        public const string ExtVcard = "vcard";

        /// <summary>
        /// The ext wav.
        /// </summary>
        public const string ExtWav = "wav";

        /// <summary>
        /// The ext wavpack.
        /// </summary>
        public const string ExtWavpack = "wv";

        /// <summary>
        /// The ext webm.
        /// </summary>
        public const string ExtWebm = "webm";

        /// <summary>
        /// The ext webm audio.
        /// </summary>
        public const string ExtWebmAudio = "webma";

        /// <summary>
        /// The ext wma.
        /// </summary>
        public const string ExtWma = "wma";

        /// <summary>
        /// The ext wmv.
        /// </summary>
        public const string ExtWmv = "wmv";

        /// <summary>
        /// The ext xml.
        /// </summary>
        public const string ExtXml = "xml";

        public const string MediaTypeAac = "audio/aac"; // http://en.wikipedia.org/wiki/Advanced_Audio_Coding
        public const string MediaTypeAacp = "audio/aacp";
        public const string MediaType3gppAudio = "audio/3gpp";
        public const string MediaType3gpp2Audio = "audio/3gpp2";

        public const string MediaTypeMp4a = "audio/MP4A-LATM";
        public const string MediaTypeMpeg4 = "audio/mpeg4-generic";

        public const string MediaType3gppVideo = "video/3gpp";
        public const string MediaType3gpp2Video = "video/3gpp2";

        public const string MediaTypeMovVideo = "video/quicktime";

        /// <summary>
        /// The media type asf.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeAsf = "video/x-ms-asf";

        /// <summary>
        /// The media type asf 1.
        /// </summary>
        public const string MediaTypeAsf1 = "video/asf";

        /// <summary>
        /// The media type asf 2.
        /// </summary>
        public const string MediaTypeAsf2 = "audio/asf";

        /// <summary>
        /// The media type bin.
        /// </summary>
        public const string MediaTypeBin = "application/octet-stream";

        /// <summary>
        /// The media type cmd.
        /// </summary>
        public const string MediaTypeCmd = "text/cmd"; // : commands; subtype resident in Gecko browsers like Firefox 3.5

        /// <summary>
        /// The media type css.
        /// </summary>
        public const string MediaTypeCss = "text/css"; // : Cascading Style Sheets; Defined in RFC 2318

        /// <summary>
        /// The media type csv.
        /// </summary>
        public const string MediaTypeCsv = "text/csv"; // : Comma-separated values; Defined in RFC 4180

        /// <summary>
        /// The media type gif.
        /// </summary>
        public const string MediaTypeGif = "image/gif"; // : GIF image; Defined in RFC 2045 and RFC 2046

        /// <summary>
        /// The media type html : HTML; Defined in RFC 2854 : text/html.
        /// </summary>
        public const string MediaTypeHtml = "text/html";

        /// <summary>
        /// The media type ico.
        /// </summary>
        public const string MediaTypeIco = "image/vnd.microsoft.icon"; // : ICO image; Registered[9]

        /// <summary>
        /// The media type jpeg.
        /// </summary>
        public const string MediaTypeJpeg = "image/jpeg"; // : JPEG JFIF image; Defined in RFC 2045 and RFC 2046

        /// <summary>
        /// The media type jpeg 1.
        /// </summary>
        public const string MediaTypeJpeg1 = "image/jpg";

        /// <summary>
        /// The media type json : application/json.
        /// </summary>
        public const string MediaTypeJson = "application/json";

        /// <summary>
        /// The media type json 1 : application/x-javascript.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeJson1 = "application/x-javascript";

        /// <summary>
        /// The media type json 2.
        /// </summary>
        public const string MediaTypeJson2 = "text/javascript";

        /// <summary>
        /// The media type json 3.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeJson3 = "text/x-javascript";

        /// <summary>
        /// The media type json 4: text/x-json.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeJson4 = "text/x-json";

        /// <summary>
        /// The media type mp 3.
        /// </summary>
        public const string MediaTypeMp3 = "audio/mpeg"; // : MP3 or other MPEG audio; Defined in RFC 3003

        /// <summary>
        /// The media type mp 31.
        /// </summary>
        public const string MediaTypeMp31 = "audio/mp3";

        /// <summary>
        /// The media type mp 4 audio.
        /// </summary>
        public const string MediaTypeMp4Audio = "audio/mp4"; // : MP4 audio

        /// <summary>
        /// The media type mp 4 video.
        /// </summary>
        public const string MediaTypeMp4Video = "video/mp4"; // : MP4 video; Defined in RFC 4337

        /// <summary>
        /// The media type mpg.
        /// </summary>
        public const string MediaTypeMpg = "video/mpeg";

                            // : MPEG-1 video with multiplexed audio; Defined in RFC 2045 and RFC 2046

        /// <summary>
        /// The media type ogg audio.
        /// </summary>
        public const string MediaTypeOggAudio = "audio/ogg";

        // : Ogg Vorbis, Speex, Flac and other audio; Defined in RFC 5334

        /// <summary>
        /// The media type for flac audio.
        /// </summary>
        public const string MediaTypeFlacAudio = "audio/flac";

        /// <summary>
        /// The media type for flac audio.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeFlacAudio1 = "audio/x-flac";

        /// <summary>
        /// The media type ogg video.
        /// </summary>
        public const string MediaTypeOggVideo = "video/ogg";

                            // : Ogg Theora or other video (with audio); Defined in RFC 5334

        /// <summary>
        /// The media type pcm.
        /// </summary>
        public const string MediaTypePcm = "audio/L16";

        /// <summary>
        /// The media type for PCM - paritcularly raw bytes in a file with no header.
        /// </summary>
        public const string MediaTypePcmRaw = "audio/pcm";

        /// <summary>
        /// The media type pjpeg.
        /// </summary>
        public const string MediaTypePjpeg = "image/pjpeg";

                            // : JPEG JFIF image; Associated with Internet Explorer; Listed in ms775147(v=vs.85) - Progressive JPEG, initiated before global browser support for progressive JPEGs (Microsoft and Firefox).

        /// <summary>
        /// The media type png.
        /// </summary>
        public const string MediaTypePng = "image/png";

                            // : Portable Network Graphics; Registered,[8] Defined in RFC 2083

        /// <summary>
        /// The media type qt.
        /// </summary>
        public const string MediaTypeQt = "video/quicktime"; // : QuickTime video; Registered[10]

        /// <summary>
        /// The media type real.
        /// </summary>
        public const string MediaTypeReal = "audio/vnd.rn-realaudio";

                            // : RealAudio; Documented in RealPlayer Customer Support Answer 2559

        /// <summary>
        /// The media type svg.
        /// </summary>
        public const string MediaTypeSvg = "image/svg+xml";

                            // : SVG vector image; Defined in SVG Tiny 1.2 Specification Appendix M

        /// <summary>
        /// The media type text plain.
        /// </summary>
        public const string MediaTypeTextPlain = "text/plain"; // : Textual data; Defined in RFC 2046 and RFC 3676

        /// <summary>
        /// The media type tiff.
        /// </summary>
        public const string MediaTypeTiff = "image/tiff";

                            // : Tag Image File Format (only for Baseline TIFF); Defined in RFC 3302

        /// <summary>
        /// The media type vcard.
        /// </summary>
        public const string MediaTypeVcard = "text/vcard"; // : vCard (contact information); Defined in RFC 6350

        /// <summary>
        /// The media type vorbis.
        /// </summary>
        public const string MediaTypeVorbis = "audio/vorbis"; // : Vorbis encoded audio; Defined in RFC 5215

        /// <summary>
        /// The media type wav.
        /// </summary>
        public const string MediaTypeWav = "audio/wav";

        /// <summary>
        /// The media type wav 1.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeWav1 = "audio/x-wav";

        /// <summary>
        /// The media type wav 2.
        /// </summary>
        public const string MediaTypeWav2 = "audio/vnd.wave"; // : WAV audio; Defined in RFC 2361

        /// <summary>
        /// The media type wavpack.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeWavpack = "audio/x-wv";

        /// <summary>
        /// The media type web m audio.
        /// </summary>
        public const string MediaTypeWebMAudio = "audio/webm"; // : WebM open media format

        /// <summary>
        /// The media type web m video.
        /// </summary>
        public const string MediaTypeWebMVideo = "video/webm"; // : WebM open media format

        /// <summary>
        /// The media type wma.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeWma = "audio/x-ms-wma"; // : Windows Media Audio; Documented in Microsoft KB 288102

        /// <summary>
        /// The media type wmv.
        /// </summary>
        [Obsolete("All 'x-' prefixes have been deprecated by https://tools.ietf.org/html/rfc6648")]
        public const string MediaTypeWmv = "video/x-ms-wmv"; // : Windows Media Video; Documented in Microsoft KB 288102

        /// <summary>
        /// The media type xml : Extensible Markup Language; Defined in RFC 3023 : text/xml.
        /// </summary>
        public const string MediaTypeXmlText = "text/xml";

        /// <summary>
        /// The media type xml  : Extensible Markup Language; Defined in RFC 3023 : application/xml.
        /// </summary>
        public const string MediaTypeXmlApplication = "application/xml";

        /// <summary>
        /// Info from ffprobe
        /// STREAM codec_long_name: PCM signed 16-bit little-endian.
        /// </summary>
        public const string CodecWavPcm16BitLe = "PCM signed 16-bit little-endian";

        public const string CodecVorbis = "Vorbis";

        public const string CodecMp3 = "MP3 (MPEG audio layer 3)";

        private static readonly List<MediaTypeExtGroup> Mapping = new List<MediaTypeExtGroup>
            {
                new MediaTypeExtGroup { MediaType = MediaTypeMp4Audio, Extension = ExtMp4, Group = MediaTypeGroup.Audio },

                // : MP4 audio
                new MediaTypeExtGroup { MediaType = MediaTypeMp3, Extension = ExtMp3, Group = MediaTypeGroup.Audio },

                // : MP3 or other MPEG audio; Defined in RFC 3003
                new MediaTypeExtGroup { MediaType = MediaTypeMp31, Extension = ExtMp3, Group = MediaTypeGroup.Audio },
                new MediaTypeExtGroup { MediaType = MediaTypeOggAudio, Extension = ExtOgg, Group = MediaTypeGroup.Audio },

                // : Ogg Vorbis, Speex, Flac and other audio; Defined in RFC 5334
                new MediaTypeExtGroup { MediaType = MediaTypeFlacAudio, Extension = ExtFlac, Group = MediaTypeGroup.Audio },
                new MediaTypeExtGroup { MediaType = MediaTypeFlacAudio1, Extension = ExtFlac, Group = MediaTypeGroup.Audio },
                new MediaTypeExtGroup
                    {
                       MediaType = MediaTypeOggAudio, Extension = ExtOggAudio, Group = MediaTypeGroup.Audio,
                    },
                new MediaTypeExtGroup { MediaType = MediaTypeVorbis, Extension = ExtOgg, Group = MediaTypeGroup.Audio },

                // : Vorbis encoded audio; Defined in RFC 5215
                new MediaTypeExtGroup { MediaType = MediaTypeWma, Extension = ExtWma, Group = MediaTypeGroup.Audio },

                // : Windows Media Audio; Documented in Microsoft KB 288102
                new MediaTypeExtGroup { MediaType = MediaTypeReal, Extension = ExtRa, Group = MediaTypeGroup.Audio },

                // : RealAudio; Documented in RealPlayer Customer Support Answer 2559
                new MediaTypeExtGroup { MediaType = MediaTypeReal, Extension = ExtRm, Group = MediaTypeGroup.Audio },

                // : RealAudio; Documented in RealPlayer Customer Support Answer 2559
                new MediaTypeExtGroup { MediaType = MediaTypeWav, Extension = ExtWav, Group = MediaTypeGroup.Audio },
                new MediaTypeExtGroup { MediaType = MediaTypeWav1, Extension = ExtWav, Group = MediaTypeGroup.Audio },
                new MediaTypeExtGroup { MediaType = MediaTypeWav2, Extension = ExtWav, Group = MediaTypeGroup.Audio },

                // : WAV audio; Defined in RFC 2361
                new MediaTypeExtGroup
                    {
                       MediaType = MediaTypeWebMAudio, Extension = ExtWebm, Group = MediaTypeGroup.Audio,
                    },

                // : WebM open media format
                new MediaTypeExtGroup
                    {
                       MediaType = MediaTypeWebMAudio, Extension = ExtWebmAudio, Group = MediaTypeGroup.Audio,
                    },

                // : WebM open media format
                new MediaTypeExtGroup { MediaType = MediaTypeAsf2, Extension = ExtAsf, Group = MediaTypeGroup.Audio },
                new MediaTypeExtGroup
                    {
                       MediaType = MediaTypeWavpack, Extension = ExtWavpack, Group = MediaTypeGroup.Audio,
                    },
                new MediaTypeExtGroup { MediaType = MediaTypePcmRaw, Extension = ExtRaw, Group = MediaTypeGroup.Audio },
                new MediaTypeExtGroup { MediaType = MediaTypeAsf, Extension = ExtAsf, Group = MediaTypeGroup.Video },
                new MediaTypeExtGroup { MediaType = MediaTypeAsf1, Extension = ExtAsf, Group = MediaTypeGroup.Video },
                new MediaTypeExtGroup { MediaType = MediaTypeMpg, Extension = ExtMpg, Group = MediaTypeGroup.Video },

                // : MPEG-1 video with multiplexed audio; Defined in RFC 2045 and RFC 2046
                new MediaTypeExtGroup { MediaType = MediaTypeMp4Video, Extension = ExtMp4, Group = MediaTypeGroup.Video },

                // : MP4 video; Defined in RFC 4337
                new MediaTypeExtGroup { MediaType = MediaTypeOggVideo, Extension = ExtOgg, Group = MediaTypeGroup.Video },

                // : Ogg Theora or other video (with audio); Defined in RFC 5334
                // new MediaTypeExtGroup{ MediaType = MediaTypeQt , Extension = "", Group = MediaTypeGroup.Video}, //: QuickTime video; Registered[10]
                new MediaTypeExtGroup
                    {
                       MediaType = MediaTypeWebMVideo, Extension = ExtWebm, Group = MediaTypeGroup.Video,
                    },

                // : WebM open media format
                new MediaTypeExtGroup { MediaType = MediaTypeWmv, Extension = ExtWmv, Group = MediaTypeGroup.Video },

                // : Windows Media Video; Documented in Microsoft KB 288102
                new MediaTypeExtGroup { MediaType = MediaTypeGif, Extension = ExtGif, Group = MediaTypeGroup.Image },

                // : GIF image; Defined in RFC 2045 and RFC 2046
                new MediaTypeExtGroup { MediaType = MediaTypeJpeg, Extension = ExtJpeg, Group = MediaTypeGroup.Image },

                // : JPEG JFIF image; Defined in RFC 2045 and RFC 2046
                new MediaTypeExtGroup { MediaType = MediaTypeJpeg1, Extension = ExtJpeg, Group = MediaTypeGroup.Image },
                new MediaTypeExtGroup { MediaType = MediaTypeJpeg, Extension = ExtJpeg1, Group = MediaTypeGroup.Image },
                new MediaTypeExtGroup { MediaType = MediaTypeJpeg1, Extension = ExtJpeg1, Group = MediaTypeGroup.Image },
                new MediaTypeExtGroup { MediaType = MediaTypePjpeg, Extension = ExtPjpeg, Group = MediaTypeGroup.Image },

                // : JPEG JFIF image; Associated with Internet Explorer; Listed in ms775147(v=vs.85) - Progressive JPEG, initiated before global browser support for progressive JPEGs (Microsoft and Firefox).
                new MediaTypeExtGroup { MediaType = MediaTypePng, Extension = ExtPng, Group = MediaTypeGroup.Image },

                // : Portable Network Graphics; Registered,[8] Defined in RFC 2083
                new MediaTypeExtGroup { MediaType = MediaTypeSvg, Extension = ExtSvg, Group = MediaTypeGroup.Image },

                // : SVG vector image; Defined in SVG Tiny 1.2 Specification Appendix M
                new MediaTypeExtGroup { MediaType = MediaTypeTiff, Extension = ExtTiff, Group = MediaTypeGroup.Image },

                // : Tag Image File Format (only for Baseline TIFF); Defined in RFC 3302
                new MediaTypeExtGroup { MediaType = MediaTypeIco, Extension = ExtIco, Group = MediaTypeGroup.Image },

                // : ICO image; Registered[9]
                new MediaTypeExtGroup { MediaType = MediaTypeCmd, Extension = ExtCmd, Group = MediaTypeGroup.Text },

                // : commands; subtype resident in Gecko browsers like Firefox 3.5
                new MediaTypeExtGroup { MediaType = MediaTypeCmd, Extension = ExtCmd1, Group = MediaTypeGroup.Text },
                new MediaTypeExtGroup { MediaType = MediaTypeCss, Extension = ExtCss, Group = MediaTypeGroup.Text },

                // : Cascading Style Sheets; Defined in RFC 2318
                new MediaTypeExtGroup { MediaType = MediaTypeCsv, Extension = ExtCsv, Group = MediaTypeGroup.Text },

                // : Comma-separated values; Defined in RFC 4180
                new MediaTypeExtGroup { MediaType = MediaTypeHtml, Extension = ExtHtml, Group = MediaTypeGroup.Text },

                // : HTML; Defined in RFC 2854
                new MediaTypeExtGroup { MediaType = MediaTypeHtml, Extension = ExtHtml1, Group = MediaTypeGroup.Text },
                new MediaTypeExtGroup
                    {
                       MediaType = MediaTypeTextPlain, Extension = ExtTextPlain, Group = MediaTypeGroup.Text,
                    },

                // : Textual data; Defined in RFC 2046 and RFC 3676
                new MediaTypeExtGroup { MediaType = MediaTypeVcard, Extension = ExtVcard, Group = MediaTypeGroup.Text },

                // : vCard (contact information); Defined in RFC 6350
                new MediaTypeExtGroup { MediaType = MediaTypeXmlText, Extension = ExtXml, Group = MediaTypeGroup.Text },

                // : Extensible Markup Language; Defined in RFC 3023
                new MediaTypeExtGroup { MediaType = MediaTypeJson, Extension = ExtJson, Group = MediaTypeGroup.Text },
                new MediaTypeExtGroup { MediaType = MediaTypeJson1, Extension = ExtJson, Group = MediaTypeGroup.Text },
                new MediaTypeExtGroup { MediaType = MediaTypeJson2, Extension = ExtJson, Group = MediaTypeGroup.Text },
                new MediaTypeExtGroup { MediaType = MediaTypeJson3, Extension = ExtJson, Group = MediaTypeGroup.Text },
                new MediaTypeExtGroup { MediaType = MediaTypeJson4, Extension = ExtJson, Group = MediaTypeGroup.Text },
            };

        /// <summary>
        /// Reduce extension string to the simplest and most significant form possible without loss of generality.
        /// </summary>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <returns>
        /// Canonical extension.
        /// </returns>
        public static string CanonicaliseExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                extension = string.Empty;
            }

            extension = extension.Trim(' ', '.').ToLowerInvariant();

            switch (extension.ToLowerInvariant())
            {
                case ExtJpeg1:
                    return ExtJpeg;
                case ExtCmd1:
                    return ExtCmd;
                case ExtHtml1:
                    return ExtHtml;

                default:
                    return extension;
            }
        }

        /// <summary>
        /// Reduce media type string to the simplest and most significant form possible without loss of generality.
        /// </summary>
        /// <param name="mediaType">
        /// The media type.
        /// </param>
        /// <returns>
        /// Canonical media type.
        /// </returns>
        public static string CanonicaliseMediaType(string mediaType)
        {
            if (string.IsNullOrEmpty(mediaType))
            {
                mediaType = string.Empty;
            }

            mediaType = mediaType.Trim().ToLowerInvariant();

            switch (mediaType.ToLowerInvariant())
            {
                case MediaTypeAsf1:
                case MediaTypeAsf2:
                    return MediaTypeAsf;

                case MediaTypeWav1:
                case MediaTypeWav2:
                    return MediaTypeWav;

                case MediaTypeMp31:
                    return MediaTypeMp3;

                case MediaTypeJpeg1:
                    return MediaTypeJpeg;

                default:
                    return mediaType;
            }
        }

        /// <summary>
        /// Get an extension from a media type. (Eg. audio/mpeg -&gt; mp3).
        /// </summary>
        /// <param name="mediaType">
        /// The media type.
        /// </param>
        /// <returns>
        /// File extension based on media type.
        /// </returns>
        public static string GetExtension(string mediaType)
        {
            return GetExtension(mediaType, MediaTypeGroup.Unknown);
        }

        /// <summary>
        /// Get an extension from a media type. (Eg. audio/mpeg -&gt; mp3).
        /// </summary>
        /// <param name="mediaType">
        /// The media type.
        /// </param>
        /// <param name="mediaTypeGroup">
        /// Restrict to a particular media Type Group.
        /// </param>
        /// <returns>
        /// File extension based on media type.
        /// </returns>
        public static string GetExtension(string mediaType, MediaTypeGroup mediaTypeGroup)
        {
            mediaType = CanonicaliseMediaType(mediaType);

            string ext;

            if (mediaTypeGroup == MediaTypeGroup.Any || mediaTypeGroup == MediaTypeGroup.Unknown)
            {
                ext = Mapping.Where(m => m.MediaType == mediaType).Select(i => i.Extension).FirstOrDefault();
            }
            else
            {
                ext =
                    Mapping.Where(m => m.MediaType == mediaType && m.Group == mediaTypeGroup).Select(i => i.Extension).
                        FirstOrDefault();
            }

            if (string.IsNullOrEmpty(ext))
            {
                ext = ExtUnknown;
            }

            ext = CanonicaliseExtension(ext);
            return ext;
        }

        /// <summary>
        /// The get image format.
        /// </summary>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <returns>
        /// </returns>
        public static IImageFormat GetImageFormat(string extension)
        {
            return SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.FindFormatByFileExtension(extension);
        }

        /// <summary>
        /// Get media type from extension (can be dangerous, as file content is not considered).
        /// </summary>
        /// <param name="extension">
        /// File Extension.
        /// </param>
        /// <returns>
        /// media type based on extension.
        /// </returns>
        public static string GetMediaType(string extension)
        {
            return GetMediaType(extension, MediaTypeGroup.Unknown);
        }

        /// <summary>
        /// Get media type from extension (can be dangerous, as file content is not considered).
        /// </summary>
        /// <param name="extension">
        /// File Extension.
        /// </param>
        /// <param name="mediaTypeGroup">
        /// Restrict to a particular media Type Group.
        /// </param>
        /// <returns>
        /// media type based on extension.
        /// </returns>
        public static string GetMediaType(string extension, MediaTypeGroup mediaTypeGroup)
        {
            extension = CanonicaliseExtension(extension);

            string mediaType;

            if (mediaTypeGroup == MediaTypeGroup.Any || mediaTypeGroup == MediaTypeGroup.Unknown)
            {
                mediaType = Mapping.Where(m => m.Extension == extension).Select(i => i.MediaType).FirstOrDefault();
            }
            else
            {
                mediaType =
                    Mapping.Where(m => m.Extension == extension && m.Group == mediaTypeGroup).Select(i => i.MediaType).
                        FirstOrDefault();
            }

            if (string.IsNullOrEmpty(mediaType))
            {
                mediaType = MediaTypeBin;
            }

            mediaType = CanonicaliseMediaType(mediaType);
            return mediaType;
        }

        /// <summary>
        /// Check if a File extension is recognised.
        /// </summary>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <returns>
        /// True if file extension is recognised, otherwise false.
        /// </returns>
        public static bool IsFileExtRecognised(string extension)
        {
            return IsFileExtRecognised(extension, MediaTypeGroup.Unknown);
        }

        /// <summary>
        /// Check if a File extension is recognised.
        /// </summary>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <param name="mediaTypeGroup">
        /// Restrict to a particular media Type Group.
        /// </param>
        /// <returns>
        /// True if file extension is recognised, otherwise false.
        /// </returns>
        public static bool IsFileExtRecognised(string extension, MediaTypeGroup mediaTypeGroup)
        {
            var mediaType = GetMediaType(extension, mediaTypeGroup);
            return mediaType != MediaTypeBin;
        }

        /// <summary>
        /// Check if a media type is recognised.
        /// </summary>
        /// <param name="mediaType">
        /// The media type.
        /// </param>
        /// <returns>
        /// True if media type is recognised, otherwise false.
        /// </returns>
        public static bool IsMediaTypeRecognised(string mediaType)
        {
            return IsMediaTypeRecognised(mediaType, MediaTypeGroup.Unknown);
        }

        /// <summary>
        /// Check if a media type is recognised.
        /// </summary>
        /// <param name="mediaType">
        /// The media type.
        /// </param>
        /// <param name="mediaTypeGroup">
        /// Restrict to a particular media Type Group.
        /// </param>
        /// <returns>
        /// True if media type is recognised, otherwise false.
        /// </returns>
        public static bool IsMediaTypeRecognised(string mediaType, MediaTypeGroup mediaTypeGroup)
        {
            var ext = GetExtension(mediaType, mediaTypeGroup);
            return ext != ExtUnknown;
        }
    }
}