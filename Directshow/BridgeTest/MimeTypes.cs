namespace AudioTools
{
	public static class MimeTypes
	{
		public const string AsfMimeType = "video/x-ms-asf";
		public const string WavMimeType = "audio/x-wav";
		public const string Mp3MimeType = "audio/mpeg";
        public const string BinMimeType = "application/octet-stream";
        public const string WavpackMimeType = "audio/x-wv";

		public static string Canonicalise(string mimeType)
		{
			switch (mimeType)
			{
				case "audio/asf":
					return AsfMimeType;
				case "audio/wav":
					return WavMimeType;
				default:
					return mimeType;
			}
		}

		public static bool IsAsf(string mimeType)
		{
			return mimeType == AsfMimeType || mimeType == "audio/asf";
		}

		public static bool IsWav(string mimeType)
		{
			return mimeType == WavMimeType || mimeType == "audio/wav";
		}

		public static bool IsMp3(string mimeType)
		{
			return mimeType == Mp3MimeType;
		}

		public static string GetExtension(string mimeType)
		{
			switch (mimeType)
			{
				case WavMimeType:
				case "audio/wav":
					return "wav";
				case MimeTypes.AsfMimeType:
				case "audio/asf":
					return "asf";
				case "audio/mpeg":
					return "mp3";
                case WavpackMimeType:
                    return "wv";
				default:
					return "unknown";
			}
		}

        public static string GetMimeTypeFromExtension(string ext)
        {
            switch (ext.ToLower())
            {
                case "asf":
                    return AsfMimeType;
                case "wav":
                    return WavMimeType;
                case "mp3":
                    return Mp3MimeType;
                case "wv":
                    return WavpackMimeType;
                default:
                    return BinMimeType;
            }
        }
	}
}
