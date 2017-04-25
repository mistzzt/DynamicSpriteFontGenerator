using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace DynamicFontGenerator
{
	[ContentTypeWriter]
	public sealed class DynamicSpriteFontWriter : ContentTypeWriter<DynamicSpriteFontContent>
	{
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "ReLogic.Graphics.DynamicSpriteFontReader, ReLogic, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		}

		protected override void Write(ContentWriter output, DynamicSpriteFontContent value)
		{
			output.Write(value.Spacing);
			output.Write(value.LineSpacing);
			output.Write(value.DefaultCharacter);

			output.Write(value.Pages.Count);

			for (var index = 0; index < value.Pages.Count; index++)
			{
				var page = value.Pages[index];

				output.WriteObject(value.Textures[index]);
				output.WriteObject(page.Glyphs);
				output.WriteObject(page.Padding);
				output.WriteObject(page.Characters);
				output.WriteObject(page.Kerning);
			}
		}
	}
}
