namespace Acoustics.Shared
{
    using System;
    using System.Configuration;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public static class TempFileHelper
    {
        /// <summary>
        /// Gets a valid temp directory.
        /// Directory will exist.
        /// </summary>
        public static DirectoryInfo TempDir()
        {
            var tempDirString = "TempDir";
            var tempDirSet = ConfigurationManager.AppSettings.AllKeys.Any(i => i == tempDirString);

            var tempDir = string.Empty;

            if (tempDirSet)
            {
                tempDir = ConfigurationManager.AppSettings["TempDir"];
            }

            if (string.IsNullOrEmpty(tempDir))
            {
                tempDir = Path.GetTempPath();
            }

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            return new DirectoryInfo(tempDir);
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.ijk).
        /// File will not exist.
        /// </summary>
        public static FileInfo NewTempFile()
        {
            return new FileInfo(Path.Combine(TempDir().FullName, GetStrongerRandomFileName()));
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.ijk).
        /// File will not exist.
        /// </summary>
        /// <param name="tempDir">Temporary directory.</param>
        /// <returns>Temp file that does not exist.</returns>
        public static FileInfo NewTempFile(DirectoryInfo tempDir)
        {
            return new FileInfo(Path.Combine(tempDir.FullName, GetStrongerRandomFileName()));
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.[given ext]).
        /// File will not exist.
        /// </summary>
        /// <param name="extension">File extension (without dot).</param>
        /// <returns>File with extension.</returns>
        public static FileInfo NewTempFile(string extension)
        {
            return NewTempFile(TempDir(), extension);
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.[given ext]).
        /// File will not exist.
        /// </summary>
        /// <param name="tempDir">Temporary directory.</param>
        /// <param name="ext">File extension (without dot).</param>
        /// <returns>File with extension.</returns>
        public static FileInfo NewTempFile(DirectoryInfo tempDir, string extension)
        {
            // ensure extension is valid (or not present).
            if (string.IsNullOrEmpty(extension))
            {
                // no extension
                extension = string.Empty;
            }
            else if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            var fileNameWithoutExtension = GetStrongerRandomFileName(false);
            return new FileInfo(Path.Combine(tempDir.FullName, fileNameWithoutExtension+extension));
        }

        /// <summary>
        /// Copy from <paramref name="source"/> Stream to Working File.
        /// </summary>
        /// <param name="source">
        /// The source stream.
        /// </param>
        /// <param name="destinationFile">File to write to.</param>
        /// <param name="append">
        /// True to append to existing file, false to replace any existing data in file.
        /// </param>
        public static void CopyFromStream(this Stream source, FileInfo destinationFile, bool append = false)
        {
            FileMode mode = FileMode.OpenOrCreate;

            if (append)
            {
                mode = FileMode.Append;
            }

            using (var target = new FileStream(destinationFile.FullName, mode, FileAccess.Write, FileShare.None))
            {
                source.CopyToStream(target);
            }
        }

        /// <summary>
        /// We suspect the original implementation of GetRandomFileName of
        /// returning colliding filenames because *we* trimmed the extension off the file.
        /// <para>
        /// This is a modification of the original method that can be found here: 
        /// http://referencesource.microsoft.com/#mscorlib/system/io/path.cs,efb113f637a6bb47
        /// </para>
        /// </summary>
        /// <returns></returns>
        private static string GetStrongerRandomFileName(bool extension = true)
        {
            lock (new object())
            {
                // 10 bytes == 80 bits == 80/5 == 16 chars in our (base32) encoding
                // This gives us exactly 32 chars
                byte[] key = new byte[20];

                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(key);

                    // rndCharArray is expected to be 32 chars
                    return ToBase32StringSuitableForDirName(key) + (extension ? ".tmp" : string.Empty);
                }
            }
        }

        private static readonly char[] SBase32Char =
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
                'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                '0', '1', '2', '3', '4', '5'
            };

        /// <summary>
        /// Modified form of http://referencesource.microsoft.com/#mscorlib/system/io/path.cs,78811508e2f49ab8
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        private static string ToBase32StringSuitableForDirName(byte[] buff)
        {
            // This routine is optimised to be used with buffs of length 20
            Contract.Assert((buff.Length % 5) == 0, "Unexpected hash length");

            StringBuilder sb = new StringBuilder(buff.Length * 8 / 5);
            byte b0, b1, b2, b3, b4;
            int l, i;

            l = buff.Length;
            i = 0;

            // Create l chars using the last 5 bits of each byte.  
            // Consume 3 MSB bits 5 bytes at a time.
            do
            {
                b0 = (i < l) ? buff[i++] : (byte)0;
                b1 = (i < l) ? buff[i++] : (byte)0;
                b2 = (i < l) ? buff[i++] : (byte)0;
                b3 = (i < l) ? buff[i++] : (byte)0;
                b4 = (i < l) ? buff[i++] : (byte)0;

                // Consume the 5 Least significant bits of each byte
                sb.Append(SBase32Char[b0 & 0x1F]);
                sb.Append(SBase32Char[b1 & 0x1F]);
                sb.Append(SBase32Char[b2 & 0x1F]);
                sb.Append(SBase32Char[b3 & 0x1F]);
                sb.Append(SBase32Char[b4 & 0x1F]);

                // Consume 3 MSB of b0, b1, MSB bits 6, 7 of b3, b4
                sb.Append(SBase32Char[(((b0 & 0xE0) >> 5) | ((b3 & 0x60) >> 2))]);

                sb.Append(SBase32Char[(((b1 & 0xE0) >> 5) | ((b4 & 0x60) >> 2))]);

                // Consume 3 MSB bits of b2, 1 MSB bit of b3, b4
                b2 >>= 5;

                Contract.Assert((b2 & 0xF8) == 0, "Unexpected set bits");

                if ((b3 & 0x80) != 0)
                {
                    b2 |= 0x08;
                }
                if ((b4 & 0x80) != 0)
                {
                    b2 |= 0x10;
                }

                sb.Append(SBase32Char[b2]);
            }
            while (i < l);

            return sb.ToString();
        }
    }
}
