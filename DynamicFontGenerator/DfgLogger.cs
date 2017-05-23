using System;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace DynamicFontGenerator
{
	internal sealed class DfgLogger : ContentBuildLogger
	{
		public override void LogMessage(string message, params object[] messageArgs)
		{
			Console.WriteLine("Log: " + string.Format(message, messageArgs));
		}

		public override void LogImportantMessage(string message, params object[] messageArgs)
		{
			Console.WriteLine("Important: " + string.Format(message, messageArgs));
		}

		public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
		{
			Console.WriteLine("Warning: " + string.Format(message, messageArgs));
			Console.WriteLine("Help link: " + helpLink);
			Console.WriteLine("File: " + contentIdentity?.SourceFilename);
		}
	}
}
