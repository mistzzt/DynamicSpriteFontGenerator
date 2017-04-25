using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

namespace DynamicFontGenerator
{
	public sealed class DynamicFontDescriptionProcessor : ContentProcessor<FontDescription[], DynamicSpriteFontContent>
	{
		private readonly FontDescriptionProcessor _fdp = new FontDescriptionProcessor();

		public override DynamicSpriteFontContent Process(FontDescription[] input, ContentProcessorContext context)
		{
			var content = new DynamicSpriteFontContent
			{
				Spacing = input.Select(d => d.Spacing).Max(),
				DefaultCharacter = input.Select(d => d.DefaultCharacter).First(f => f != null).Value
			};

			var lineSpacings = new List<int>(input.Length);

			foreach (var fd in input)
			{
				if (fd == null)
					continue;

				var result = _fdp.Process(fd, context);

				lineSpacings.Add(result.LineSpacing);

				content.Pages.Add(
					new FontPage(
						null,
						new List<Rectangle>(result.Glyphs),
						new List<Rectangle>(result.Cropping),
						new List<char>(result.CharacterMap),
						new List<Vector3>(result.Kerning)
					)
				);

				content.Textures.Add(result.Texture);
			}

			content.LineSpacing = lineSpacings.Max();

			return content;
		}
	}
}
