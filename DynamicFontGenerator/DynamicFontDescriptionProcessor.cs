using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DynamicFontGenerator
{
	public sealed class DynamicFontDescriptionProcessor : ContentProcessor<FontDescription[], Tuple<Texture2DContent, FontPage>[]>
	{
		private readonly FontDescriptionProcessor _fdp = new FontDescriptionProcessor();

		public override Tuple<Texture2DContent, FontPage>[] Process(FontDescription[] input, ContentProcessorContext context)
		{
			var pages = new List<Tuple<Texture2DContent, FontPage>>();

			foreach (var fd in input)
			{
				if (fd == null)
					continue;

				var result = _fdp.Process(fd, context);

				pages.Add(new Tuple<Texture2DContent, FontPage>(result.Texture,
						new FontPage(null,
							new List<Rectangle>(result.Glyphs),
							new List<Rectangle>(result.Cropping),
							new List<char>(result.CharacterMap),
							new List<Vector3>(result.Kerning)
						)
					)
				);
			}
			return pages.ToArray();
		}
	}
}
