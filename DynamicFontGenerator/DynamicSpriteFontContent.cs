using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;

namespace DynamicFontGenerator
{
	public sealed class DynamicSpriteFontContent
	{
		public List<Texture2DContent> Textures { get; } = new List<Texture2DContent>();

		public List<FontPage> Pages { get; } = new List<FontPage>();

		public int LineSpacing { get; internal set; }

		public float Spacing { get; internal set; }

		public char DefaultCharacter { get; internal set; }
	}
}
