using Microsoft.Xna.Framework.Content.Pipeline;
using System;

namespace DynamicFontGenerator
{
	public sealed class DfgImporterContext : ContentImporterContext
	{
		public override ContentBuildLogger Logger => throw new NotImplementedException();

		public override string OutputDirectory => throw new NotImplementedException();

		public override string IntermediateDirectory => throw new NotImplementedException();

		public override void AddDependency(string filename) => throw new NotImplementedException();
	}
}
