namespace Acoustics.Shared
{
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    public class MediaTypeExtGroup
    {
        public MediaTypeGroup Group { get; set; }

        public string Extension { get; set; }

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
        Text = 5
    }

    /// <summary>
    /// Utility class for handling media types and file extensions. 
    /// </summary>
    /// <remarks>
    /// see http://en.wikipedia.org/wiki/Internet_media_type#Type_image for more info.
    /// </remarks>
    public static class MediaTypes
    {
        #region Internet Media Types and extensions - audio

        ////public const string MediaTypeBasic = "audio/basic"; //: mulaw audio at 8 kHz, 1 channel; Defined in RFC 2046

        ////public const string MediaTypeL24 = "audio/L24"; //: 24bit Linear PCM audio at 8-48kHz, 1-N channels; Defined in RFC 3190

        public const string MediaTypeMp4Audio = "audio/mp4"; //: MP4 audio

        public const string MediaTypeMp3 = "audio/mpeg"; //: MP3 or other MPEG audio; Defined in RFC 3003

        public const string MediaTypeMp31 = "audio/mp3";

        public const string MediaTypeOggAudio = "audio/ogg"; //: Ogg Vorbis, Speex, Flac and other audio; Defined in RFC 5334

        public const string MediaTypeVorbis = "audio/vorbis"; //: Vorbis encoded audio; Defined in RFC 5215

        public const string MediaTypeWma = "audio/x-ms-wma"; //: Windows Media Audio; Documented in Microsoft KB 288102

        //public const string MediaTypeWax = "audio/x-ms-wax"; //: Windows Media Audio Redirector; Documented in Microsoft help page

        public const string MediaTypeReal = "audio/vnd.rn-realaudio"; //: RealAudio; Documented in RealPlayer Customer Support Answer 2559

        public const string MediaTypeWav = "audio/x-wav";

        public const string MediaTypeWav1 = "audio/wav";

        public const string MediaTypeWav2 = "audio/vnd.wave"; //: WAV audio; Defined in RFC 2361

        public const string MediaTypeWebMAudio = "audio/webm"; //: WebM open media format

        public const string MediaTypeAsf2 = "audio/asf";

        public const string MediaTypeWavpack = "audio/x-wv";

        public const string MediaTypePcm = "audio/L16";

        public const string ExtWma = "wma";

        public const string ExtOgg = "ogg";

        public const string ExtOggAudio = "oga";

        public const string ExtWavpack = "wv";

        public const string ExtMp3 = "mp3";

        public const string ExtWav = "wav";

        public const string ExtWebm = "webm";

        public const string ExtWebmAudio = "webma";

        public const string ExtRa = "ra";

        public const string ExtRm = "rm";

        #endregion

        #region Internet Media Types and extensions - video

        public const string MediaTypeAsf = "video/x-ms-asf";

        public const string MediaTypeAsf1 = "video/asf";

        public const string MediaTypeMpg = "video/mpeg"; //: MPEG-1 video with multiplexed audio; Defined in RFC 2045 and RFC 2046

        public const string MediaTypeMp4Video = "video/mp4"; //: MP4 video; Defined in RFC 4337

        public const string MediaTypeOggVideo = "video/ogg"; //: Ogg Theora or other video (with audio); Defined in RFC 5334

        public const string MediaTypeQt = "video/quicktime"; //: QuickTime video; Registered[10]

        public const string MediaTypeWebMVideo = "video/webm"; //: WebM open media format

        public const string MediaTypeWmv = "video/x-ms-wmv"; //: Windows Media Video; Documented in Microsoft KB 288102

        public const string ExtAsf = "asf";

        public const string ExtMpg = "mpg";

        public const string ExtMp4 = "mp4";

        public const string ExtWmv = "wmv";

        #endregion

        #region Internet Media Types and extensions - image

        public const string MediaTypeGif = "image/gif"; //: GIF image; Defined in RFC 2045 and RFC 2046

        public const string MediaTypeJpeg = "image/jpeg"; //: JPEG JFIF image; Defined in RFC 2045 and RFC 2046

        public const string MediaTypeJpeg1 = "image/jpg";

        public const string MediaTypePjpeg = "image/pjpeg";//: JPEG JFIF image; Associated with Internet Explorer; Listed in ms775147(v=vs.85) - Progressive JPEG, initiated before global browser support for progressive JPEGs (Microsoft and Firefox).

        public const string MediaTypePng = "image/png"; //: Portable Network Graphics; Registered,[8] Defined in RFC 2083

        public const string MediaTypeSvg = "image/svg+xml"; //: SVG vector image; Defined in SVG Tiny 1.2 Specification Appendix M

        public const string MediaTypeTiff = "image/tiff"; //: Tag Image File Format (only for Baseline TIFF); Defined in RFC 3302

        public const string MediaTypeIco = "image/vnd.microsoft.icon"; //: ICO image; Registered[9]

        public const string ExtJpeg = "jpg";

        public const string ExtJpeg1 = "jpeg";

        public const string ExtPjpeg = "pjpeg";

        public const string ExtGif = "gif";

        public const string ExtPng = "png";

        public const string ExtSvg = "svg";

        public const string ExtTiff = "tiff";

        public const string ExtIco = "ico";

        #endregion

        #region Internet Media Types and extensions - text

        public const string MediaTypeCmd = "text/cmd"; //: commands; subtype resident in Gecko browsers like Firefox 3.5
        public const string MediaTypeCss = "text/css"; //: Cascading Style Sheets; Defined in RFC 2318
        public const string MediaTypeCsv = "text/csv"; //: Comma-separated values; Defined in RFC 4180
        public const string MediaTypeHtml = "text/html"; //: HTML; Defined in RFC 2854

        public const string MediaTypeTextPlain = "text/plain"; //: Textual data; Defined in RFC 2046 and RFC 3676
        public const string MediaTypeVcard = "text/vcard"; //: vCard (contact information); Defined in RFC 6350
        public const string MediaTypeXml = "text/xml"; //: Extensible Markup Language; Defined in RFC 3023

        public const string MediaTypeJson = "application/json";
        public const string MediaTypeJson1 = "application/x-javascript";
        public const string MediaTypeJson2 = "text/javascript";
        public const string MediaTypeJson3 = "text/x-javascript";
        public const string MediaTypeJson4 = "text/x-json";

        public const string ExtCmd = "cmd";
        public const string ExtCmd1 = "bat";

        public const string ExtCss = "css";

        public const string ExtCsv = "csv";

        public const string ExtHtml = "html";
        public const string ExtHtml1 = "htm";

        public const string ExtTextPlain = "txt";

        public const string ExtVcard = "vcard";

        public const string ExtXml = "xml";

        public const string ExtJson = "json";


        #endregion

        public const string MediaTypeBin = "application/octet-stream";

        public const string ExtUnknown = "unknown";

        private readonly static List<MediaTypeExtGroup> Mapping = new List<MediaTypeExtGroup>
            {
                new MediaTypeExtGroup{ MediaType = MediaTypeMp4Audio, Extension = ExtMp4, Group = MediaTypeGroup.Audio}, //: MP4 audio
                new MediaTypeExtGroup{ MediaType = MediaTypeMp3, Extension = ExtMp3, Group = MediaTypeGroup.Audio}, //: MP3 or other MPEG audio; Defined in RFC 3003
                new MediaTypeExtGroup{ MediaType = MediaTypeMp31, Extension = ExtMp3, Group = MediaTypeGroup.Audio}, 
                new MediaTypeExtGroup{ MediaType = MediaTypeOggAudio, Extension = ExtOgg, Group = MediaTypeGroup.Audio}, //: Ogg Vorbis, Speex, Flac and other audio; Defined in RFC 5334
                new MediaTypeExtGroup{ MediaType = MediaTypeOggAudio, Extension = ExtOggAudio, Group = MediaTypeGroup.Audio},
                new MediaTypeExtGroup{ MediaType = MediaTypeVorbis, Extension = ExtOgg, Group = MediaTypeGroup.Audio},  //: Vorbis encoded audio; Defined in RFC 5215
                new MediaTypeExtGroup{ MediaType = MediaTypeWma, Extension = ExtWma, Group = MediaTypeGroup.Audio},  //: Windows Media Audio; Documented in Microsoft KB 288102
                new MediaTypeExtGroup{ MediaType = MediaTypeReal, Extension = ExtRa, Group = MediaTypeGroup.Audio},  //: RealAudio; Documented in RealPlayer Customer Support Answer 2559
                new MediaTypeExtGroup{ MediaType = MediaTypeReal, Extension = ExtRm, Group = MediaTypeGroup.Audio},  //: RealAudio; Documented in RealPlayer Customer Support Answer 2559
                new MediaTypeExtGroup{ MediaType = MediaTypeWav, Extension = ExtWav, Group = MediaTypeGroup.Audio}, 
                new MediaTypeExtGroup{ MediaType = MediaTypeWav1, Extension = ExtWav, Group = MediaTypeGroup.Audio}, 
                new MediaTypeExtGroup{ MediaType = MediaTypeWav2 , Extension = ExtWav, Group = MediaTypeGroup.Audio}, //: WAV audio; Defined in RFC 2361
                new MediaTypeExtGroup{ MediaType = MediaTypeWebMAudio , Extension = ExtWebm, Group = MediaTypeGroup.Audio},  //: WebM open media format
                new MediaTypeExtGroup{ MediaType = MediaTypeWebMAudio , Extension = ExtWebmAudio, Group = MediaTypeGroup.Audio},  //: WebM open media format
                new MediaTypeExtGroup{ MediaType = MediaTypeAsf2 , Extension = ExtAsf, Group = MediaTypeGroup.Audio}, 
                new MediaTypeExtGroup{ MediaType = MediaTypeWavpack , Extension = ExtWavpack, Group = MediaTypeGroup.Audio}, 
                new MediaTypeExtGroup{ MediaType = MediaTypePcm , Extension = ExtWav, Group = MediaTypeGroup.Audio}, 

                new MediaTypeExtGroup{ MediaType = MediaTypeAsf, Extension = ExtAsf, Group = MediaTypeGroup.Video},
                new MediaTypeExtGroup{ MediaType = MediaTypeAsf1, Extension = ExtAsf, Group = MediaTypeGroup.Video},
                new MediaTypeExtGroup{ MediaType = MediaTypeMpg, Extension = ExtMpg, Group = MediaTypeGroup.Video}, //: MPEG-1 video with multiplexed audio; Defined in RFC 2045 and RFC 2046
                new MediaTypeExtGroup{ MediaType = MediaTypeMp4Video , Extension =ExtMp4, Group = MediaTypeGroup.Video}, //: MP4 video; Defined in RFC 4337
                new MediaTypeExtGroup{ MediaType = MediaTypeOggVideo , Extension = ExtOgg, Group = MediaTypeGroup.Video}, //: Ogg Theora or other video (with audio); Defined in RFC 5334
                //new MediaTypeExtGroup{ MediaType = MediaTypeQt , Extension = "", Group = MediaTypeGroup.Video}, //: QuickTime video; Registered[10]
                new MediaTypeExtGroup{ MediaType = MediaTypeWebMVideo , Extension = ExtWebm, Group = MediaTypeGroup.Video}, //: WebM open media format
                new MediaTypeExtGroup{ MediaType = MediaTypeWmv , Extension = ExtWmv, Group = MediaTypeGroup.Video}, //: Windows Media Video; Documented in Microsoft KB 288102

                new MediaTypeExtGroup{ MediaType = MediaTypeGif, Extension = ExtGif, Group = MediaTypeGroup.Image},  //: GIF image; Defined in RFC 2045 and RFC 2046
                new MediaTypeExtGroup{ MediaType = MediaTypeJpeg , Extension = ExtJpeg, Group = MediaTypeGroup.Image}, //: JPEG JFIF image; Defined in RFC 2045 and RFC 2046
                new MediaTypeExtGroup{ MediaType = MediaTypeJpeg1 , Extension = ExtJpeg, Group = MediaTypeGroup.Image},
                new MediaTypeExtGroup{ MediaType = MediaTypeJpeg , Extension = ExtJpeg1, Group = MediaTypeGroup.Image},
                new MediaTypeExtGroup{ MediaType = MediaTypeJpeg1 , Extension = ExtJpeg1, Group = MediaTypeGroup.Image},
                new MediaTypeExtGroup{ MediaType = MediaTypePjpeg , Extension = ExtPjpeg, Group = MediaTypeGroup.Image},//: JPEG JFIF image; Associated with Internet Explorer; Listed in ms775147(v=vs.85) - Progressive JPEG, initiated before global browser support for progressive JPEGs (Microsoft and Firefox).
                new MediaTypeExtGroup{ MediaType = MediaTypePng , Extension = ExtPng, Group = MediaTypeGroup.Image}, //: Portable Network Graphics; Registered,[8] Defined in RFC 2083
                new MediaTypeExtGroup{ MediaType = MediaTypeSvg , Extension = ExtSvg, Group = MediaTypeGroup.Image},//: SVG vector image; Defined in SVG Tiny 1.2 Specification Appendix M
                new MediaTypeExtGroup{ MediaType = MediaTypeTiff , Extension = ExtTiff, Group = MediaTypeGroup.Image}, //: Tag Image File Format (only for Baseline TIFF); Defined in RFC 3302
                new MediaTypeExtGroup{ MediaType = MediaTypeIco , Extension = ExtIco, Group = MediaTypeGroup.Image}, //: ICO image; Registered[9]

                new MediaTypeExtGroup{ MediaType = MediaTypeCmd , Extension = ExtCmd, Group = MediaTypeGroup.Text}, //: commands; subtype resident in Gecko browsers like Firefox 3.5
                new MediaTypeExtGroup{ MediaType = MediaTypeCmd , Extension = ExtCmd1, Group = MediaTypeGroup.Text},
                new MediaTypeExtGroup{ MediaType = MediaTypeCss , Extension = ExtCss, Group = MediaTypeGroup.Text}, //: Cascading Style Sheets; Defined in RFC 2318
                new MediaTypeExtGroup{ MediaType = MediaTypeCsv , Extension = ExtCsv, Group = MediaTypeGroup.Text},//: Comma-separated values; Defined in RFC 4180
                new MediaTypeExtGroup{ MediaType = MediaTypeHtml, Extension = ExtHtml, Group = MediaTypeGroup.Text}, //: HTML; Defined in RFC 2854
                new MediaTypeExtGroup{ MediaType = MediaTypeHtml, Extension = ExtHtml1, Group = MediaTypeGroup.Text},
                new MediaTypeExtGroup{ MediaType = MediaTypeTextPlain , Extension = ExtTextPlain, Group = MediaTypeGroup.Text}, //: Textual data; Defined in RFC 2046 and RFC 3676
                new MediaTypeExtGroup{ MediaType = MediaTypeVcard , Extension = ExtVcard, Group = MediaTypeGroup.Text}, //: vCard (contact information); Defined in RFC 6350
                new MediaTypeExtGroup{ MediaType = MediaTypeXml , Extension = ExtXml, Group = MediaTypeGroup.Text}, //: Extensible Markup Language; Defined in RFC 3023
            };

        /// <summary>
        /// Get an extension from a media type.
        /// (Eg. audio/mpeg -&gt; mp3).
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
        /// Get an extension from a media type.
        /// (Eg. audio/mpeg -&gt; mp3).
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

        public static ImageFormat GetImageFormat(string extension)
        {
            ImageFormat format;

            switch (MediaTypes.CanonicaliseExtension(extension))
            {
                //case MediaTypes.ExtBmp:
                //    format = ImageFormat.Bmp;
                //    break;
                //case MediaTypes.ExtEmf:
                //    format = ImageFormat.Emf;
                //    break;
                //case MediaTypes.ExtExif:
                //    format = ImageFormat.Exif;
                //    break;
                //case MediaTypes.ExtGif:
                //    format = ImageFormat.Gif;
                //    break;
                case MediaTypes.ExtIco:
                    format = ImageFormat.Icon;
                    break;
                case MediaTypes.ExtJpeg:
                    format = ImageFormat.Jpeg;
                    break;
                //case MediaTypes.ExtBmp:
                //    format = ImageFormat.MemoryBmp;
                //    break;
                case MediaTypes.ExtPng:
                    format = ImageFormat.Png;
                    break;
                case MediaTypes.ExtTiff:
                    format = ImageFormat.Tiff;
                    break;
                //case MediaTypes.ExtWmf:
                //    format = ImageFormat.Wmf;
                //    break;
                default:
                    format = ImageFormat.Jpeg;
                    break;
            }

            return format;
        }
    }

    public static class FileTypeHelper
    {
        public static string GetContentType(string sourceFileName)
        {
            var extension = Path.GetExtension(sourceFileName).ToLower();
            switch (extension)
            {
                case ".ai": return "application/postscript";
                case ".aif": return "audio/x-aiff";
                case ".aifc": return "audio/x-aiff";
                case ".aiff": return "audio/x-aiff";
                case ".asc": return "text/plain";
                case ".au": return "audio/basic";
                case ".avi": return "video/x-msvideo";
                case ".bcpio": return "application/x-bcpio";
                case ".bin": return "application/octet-stream";
                case ".c": return "text/plain";
                case ".cc": return "text/plain";
                case ".ccad": return "application/clariscad";
                case ".cdf": return "application/x-netcdf";
                case ".class": return "application/octet-stream";
                case ".cpio": return "application/x-cpio";
                case ".cpp": return "text/plain";
                case ".cpt": return "application/mac-compactpro";
                case ".cs": return "text/plain";
                case ".csh": return "application/x-csh";
                case ".css": return "text/css";
                case ".dcr": return "application/x-director";
                case ".dir": return "application/x-director";
                case ".dms": return "application/octet-stream";
                case ".doc": return "application/msword";
                case ".drw": return "application/drafting";
                case ".dvi": return "application/x-dvi";
                case ".dwg": return "application/acad";
                case ".dxf": return "application/dxf";
                case ".dxr": return "application/x-director";
                case ".eps": return "application/postscript";
                case ".etx": return "text/x-setext";
                case ".exe": return "application/octet-stream";
                case ".ez": return "application/andrew-inset";
                case ".f": return "text/plain";
                case ".f90": return "text/plain";
                case ".fli": return "video/x-fli";
                case ".flv": return "video/x-flv";
                case ".gif": return "image/gif";
                case ".gtar": return "application/x-gtar";
                case ".gz": return "application/x-gzip";
                case ".h": return "text/plain";
                case ".hdf": return "application/x-hdf";
                case ".hh": return "text/plain";
                case ".hqx": return "application/mac-binhex40";
                case ".htm": return "text/html";
                case ".html": return "text/html";
                case ".ice": return "x-conference/x-cooltalk";
                case ".ief": return "image/ief";
                case ".iges": return "model/iges";
                case ".igs": return "model/iges";
                case ".ips": return "application/x-ipscript";
                case ".ipx": return "application/x-ipix";
                case ".jpe": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".jpg": return "image/jpeg";
                case ".js": return "application/x-javascript";
                case ".kar": return "audio/midi";
                case ".latex": return "application/x-latex";
                case ".lha": return "application/octet-stream";
                case ".lsp": return "application/x-lisp";
                case ".lzh": return "application/octet-stream";
                case ".m": return "text/plain";
                case ".man": return "application/x-troff-man";
                case ".me": return "application/x-troff-me";
                case ".mesh": return "model/mesh";
                case ".mid": return "audio/midi";
                case ".midi": return "audio/midi";
                case ".mime": return "www/mime";
                case ".mov": return "video/quicktime";
                case ".movie": return "video/x-sgi-movie";
                case ".mp2": return "audio/mpeg";
                case ".mp3": return "audio/mpeg";
                case ".mpe": return "video/mpeg";
                case ".mpeg": return "video/mpeg";
                case ".mpg": return "video/mpeg";
                case ".mpga": return "audio/mpeg";
                case ".ms": return "application/x-troff-ms";
                case ".msh": return "model/mesh";
                case ".nc": return "application/x-netcdf";
                case ".oda": return "application/oda";
                case ".pbm": return "image/x-portable-bitmap";
                case ".pdb": return "chemical/x-pdb";
                case ".pdf": return "application/pdf";
                case ".pgm": return "image/x-portable-graymap";
                case ".pgn": return "application/x-chess-pgn";
                case ".png": return "image/png";
                case ".pnm": return "image/x-portable-anymap";
                case ".pot": return "application/mspowerpoint";
                case ".ppm": return "image/x-portable-pixmap";
                case ".pps": return "application/mspowerpoint";
                case ".ppt": return "application/mspowerpoint";
                case ".ppz": return "application/mspowerpoint";
                case ".pre": return "application/x-freelance";
                case ".prt": return "application/pro_eng";
                case ".ps": return "application/postscript";
                case ".qt": return "video/quicktime";
                case ".ra": return "audio/x-realaudio";
                case ".ram": return "audio/x-pn-realaudio";
                case ".ras": return "image/cmu-raster";
                case ".rgb": return "image/x-rgb";
                case ".rm": return "audio/x-pn-realaudio";
                case ".roff": return "application/x-troff";
                case ".rpm": return "audio/x-pn-realaudio-plugin";
                case ".rtf": return "text/rtf";
                case ".rtx": return "text/richtext";
                case ".scm": return "application/x-lotusscreencam";
                case ".set": return "application/set";
                case ".sgm": return "text/sgml";
                case ".sgml": return "text/sgml";
                case ".sh": return "application/x-sh";
                case ".shar": return "application/x-shar";
                case ".silo": return "model/mesh";
                case ".sit": return "application/x-stuffit";
                case ".skd": return "application/x-koan";
                case ".skm": return "application/x-koan";
                case ".skp": return "application/x-koan";
                case ".skt": return "application/x-koan";
                case ".smi": return "application/smil";
                case ".smil": return "application/smil";
                case ".snd": return "audio/basic";
                case ".sol": return "application/solids";
                case ".spl": return "application/x-futuresplash";
                case ".src": return "application/x-wais-source";
                case ".step": return "application/STEP";
                case ".stl": return "application/SLA";
                case ".stp": return "application/STEP";
                case ".sv4cpio": return "application/x-sv4cpio";
                case ".sv4crc": return "application/x-sv4crc";
                case ".swf": return "application/x-shockwave-flash";
                case ".t": return "application/x-troff";
                case ".tar": return "application/x-tar";
                case ".tcl": return "application/x-tcl";
                case ".tex": return "application/x-tex";
                case ".tif": return "image/tiff";
                case ".tiff": return "image/tiff";
                case ".tr": return "application/x-troff";
                case ".tsi": return "audio/TSP-audio";
                case ".tsp": return "application/dsptype";
                case ".tsv": return "text/tab-separated-values";
                case ".txt": return "text/plain";
                case ".unv": return "application/i-deas";
                case ".ustar": return "application/x-ustar";
                case ".vcd": return "application/x-cdlink";
                case ".vda": return "application/vda";
                case ".vrml": return "model/vrml";
                case ".wav": return "audio/x-wav";
                case ".wrl": return "model/vrml";
                case ".xbm": return "image/x-xbitmap";
                case ".xlc": return "application/vnd.ms-excel";
                case ".xll": return "application/vnd.ms-excel";
                case ".xlm": return "application/vnd.ms-excel";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlw": return "application/vnd.ms-excel";
                case ".xml": return "text/xml";
                case ".xpm": return "image/x-xpixmap";
                case ".xwd": return "image/x-xwindowdump";
                case ".xyz": return "chemical/x-pdb";
                case ".zip": return "application/zip";
                default: return string.Format("application/{0}", extension);
            }
        }
    }

}