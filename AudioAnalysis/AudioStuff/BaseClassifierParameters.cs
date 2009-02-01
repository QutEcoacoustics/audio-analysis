using System;
using TowseyLib;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;

namespace AudioStuff
{
	[Serializable]
	public abstract class BaseClassifierParameters
	{
		public static BaseClassifierParameters Deserialize(Stream stream)
		{
			var formatter = new BinaryFormatter();
			return formatter.Deserialize(stream) as BaseClassifierParameters;
		}

		public static BaseClassifierParameters Deserialize(byte[] data)
		{
			var formatter = new BinaryFormatter();
			using (var stream = new MemoryStream(data))
				return formatter.Deserialize(stream) as BaseClassifierParameters;
		}

		[SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
		public byte[] Serialize()
		{
			var formatter = new BinaryFormatter();
			using (var stream = new MemoryStream())
			{
				formatter.Serialize(stream, this);
				return stream.ToArray();
			}
		}

		public BaseClassifierParameters()
		{
		}

		public BaseClassifierParameters(Configuration config)
		{
			Name = config.GetString("CALL_NAME");
			Description = config.GetString("COMMENT");
		}

		public virtual void Save(TextWriter writer)
		{
			writer.WriteConfigValue("CALL_NAME", Name);
			writer.WriteConfigValue("COMMENT", Name);
		}

		public string Name { get; set; }
		public string Description { get; set; }
	}

	[Serializable]
	public class TemplateParameters : BaseClassifierParameters
	{
		public TemplateParameters(Configuration config) : base(config)
		{
			SourcePath = config.GetString("WAV_FILE_PATH");
			var duration = config.GetDoubleNullable("WAV_DURATION");
			if (duration != null)
				SourceDuration = TimeSpan.FromSeconds(duration.Value);
			SampleRate = config.GetInt("WAV_SAMPLE_RATE");
		}

		public override void Save(TextWriter writer)
		{
			writer.WriteConfigValue("WAV_FILE_PATH", SourcePath);
			writer.WriteConfigValue("WAV_DURATION", SourceDuration.TotalSeconds);
			writer.WriteConfigValue("WAV_SAMPLE_RATE", SampleRate);
		}

		#region Properties
		public string SourcePath { get; set; } // Path to original audio recording used to generate the template
		public TimeSpan SourceDuration { get; set; }

		public int SampleRate { get; set; } // Sample rate of the original source
		#endregion
	}
}