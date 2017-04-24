using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ReLogic.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace DynamicFontGenerator
{
	[ContentTypeWriter]
	public class DynamicSpriteFontWriter : ContentTypeWriter<DynamicSpriteFont>
	{
		private const int LineSpacing = 21;

		private const float CharacterSpacing = 0;

		private const char DefaultCharacter = '*';

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "ReLogic.Graphics.DynamicSpriteFontReader, ReLogic, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		}

		protected override void Write(ContentWriter output, DynamicSpriteFont value)
		{
			output.Write(CharacterSpacing);
			output.Write(LineSpacing);
			output.Write(DefaultCharacter);

			output.Write(Generator._pages.Length);

			foreach (var page in Generator._pages)
			{
				output.WriteObject(page.Item1);
				output.WriteObject(page.Item2.Glyphs);
				output.WriteObject(page.Item2.Padding);
				output.WriteObject(page.Item2.Characters);
				output.WriteObject(page.Item2.Kerning);
			}
		}
	}
}
