using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace DynamicFontGenerator
{
	internal class Generator : Game
	{
		private readonly GraphicsDeviceManager _graphics;

		internal static Tuple<Texture2DContent, FontPage>[] _pages;

		private static void Main(string[] args)
		{
			DynamicSpriteFontReader.ReadPage orp = (font, pages) =>
			{
				for (var i = 0; i < pages.Length; i++)
				{
					pages[i].Texture.SaveAsPng(File.Create("Test\\" + i + ".png"), pages[i].Texture.Width, pages[i].Texture.Height);
				}
			};

			//DynamicSpriteFontReader.OnReadPage = orp;

			using (var game = new Generator())
			{
				game.Run();
			}
		}

		public Generator()
		{
			_graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();

			LoadContent();

			CompileFonts();
		}

		protected override void LoadContent()
		{
			base.LoadContent();
		}

		private void CompileFonts()
		{
			CompileFont("Mouse_Text.xnb", 15f, 14f);
		}

		private void CompileFont(string fileName, float enSize, float cnSize)
		{
			Console.WriteLine("Start compiling {0} with enSize {1} and cnSize {2}...", fileName, enSize, cnSize);

			var processor = new DynamicFontDescriptionProcessor();
			var context = new DfgContext(this);

			var descriptions = new List<FontDescription>();
			var englishChars = new List<char>();

			var des = new FontDescription("CLINTON", enSize, 0f);

			for (var c = '!'; c < '~'; c++)
			{
				des.Characters.Add(c);
				englishChars.Add(c);
			}

			descriptions.Add(des);

			des = new FontDescription("方正准圆_GBK", cnSize, 0f);

			if(_characters == null)
			{
				_characters = File.ReadAllText("chars.txt").ToCharArray().Except(englishChars).ToArray();
			}

			const int max = 25 * 11;
			var current = 0;

			for (var index = 0; index < _characters.Length; index++)
			{
				var c = _characters[index];
				if (current + 1 > max)
				{
					descriptions.Add(des);
					des = new FontDescription("方正准圆_GBK", cnSize, 0f);
					current = 0;

					Console.WriteLine("Adding page {0}", descriptions.Count);
				}
				des.Characters.Add(c);
				current++;
			}

			if (!descriptions.Contains(des))
			{
				descriptions.Add(des);
			}

			_pages = processor.Process(descriptions.ToArray(), context);

			if(_compiler == null)
			{
				_compiler = new ContentCompiler();
			}

			using (var fs = new FileStream(fileName, FileMode.Create))
			{
				_compiler.Compile(fs, new DynamicSpriteFont(0, 0, ' '), TargetPlatform.Windows, GraphicsProfile.Reach, true, Environment.CurrentDirectory, Environment.CurrentDirectory);
			}
		}

		private ContentCompiler _compiler;

		private char[] _characters;
	}
}
