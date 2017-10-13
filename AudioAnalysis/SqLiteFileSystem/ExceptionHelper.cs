// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

namespace Zio.FileSystems.Community.SqliteFileSystem
{
    using System.IO;
    using Zio;

    /// <summary>
    /// Lifted from https://raw.githubusercontent.com/xoofx/zio/78b66d29c857b450e495c31b38f7ed4021ebec8e/src/Zio/FileSystemExceptionHelper.cs
    /// so excpetions are formatted in a similar manner.
    /// </summary>
    internal static class FileSystemExceptionHelper
    {
        public static FileNotFoundException NewFileNotFoundException(UPath path)
        {
            return new FileNotFoundException($"Could not find file `{path}`.");
        }

        public static DirectoryNotFoundException NewDirectoryNotFoundException(UPath path)
        {
            return new DirectoryNotFoundException($"Could not find a part of the path `{path}`.");
        }

        public static IOException NewDestinationDirectoryExistException(UPath path)
        {
            return new IOException($"The destination path `{path}` is an existing directory");
        }

        public static IOException NewDestinationFileExistException(UPath path)
        {
            return new IOException($"The destination path `{path}` is an existing file");
        }
    }
}