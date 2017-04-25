#define ORIGIN

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DynamicFontGenerator
{
	public sealed class Generator : Game
	{
		private readonly GraphicsDeviceManager _graphics;

		internal static Tuple<Texture2DContent, FontPage>[] Pages;

		private static void Main()
		{
			using (var game = new Generator())
			{
				game.Run();
			}
		}

		public Generator()
		{
			_graphics = new GraphicsDeviceManager(this);
			_englishChars = new List<char>
			{
				'©'
			};
			for (var c = '!'; c < '~'; c++)
				_englishChars.Add(c);
			_characters = File.ReadAllText("chars.txt")
									.Except(_englishChars)
									.ToArray();
			_compiler = new ContentCompiler();
			_processor = new DynamicFontDescriptionProcessor();
			_context = new DfgContext(this);

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();

			CompileFonts();

			Environment.Exit(0);
		}

		private void CompileFonts()
		{
#if ORIGIN
			CompileFont("Mouse_Text.xnb", "CLINTON", 17f, "方正准圆_GBK", 14.2f);
			CompileFont("Death_Text.xnb", "CLINTON", 32f, "方正准圆_GBK", 31f);
			CompileFont("Item_Stack.xnb", "CLINTON", 14f, "方正准圆_GBK", 12f);
			CompileFont("Combat_Text.xnb", "CLINTON", 20f, "方正准圆_GBK", 18f);
			CompileFont("Combat_Crit.xnb", "CLINTON", 18f, "方正准圆_GBK", 16f);
#else
			CompileFont("Mouse_Text.xnb", "方正准圆_GBK", 14.55f, "方正准圆_GBK", 14.55f);
			CompileFont("Death_Text.xnb", "方正准圆_GBK", 31f, "方正准圆_GBK", 31f);
			CompileFont("Item_Stack.xnb", "方正准圆_GBK", 13.2f, "方正准圆_GBK", 13.2f);
			CompileFont("Combat_Text.xnb", "方正准圆_GBK", 18f, "方正准圆_GBK", 18f);
			CompileFont("Combat_Crit.xnb", "方正准圆_GBK", 16f, "方正准圆_GBK", 16f);
#endif
		}

		private void CompileFont(string fileName, string enFontName, float enSize, string cnFontName, float cnSize)
		{
			Console.WriteLine("Start compiling {0} with enFont {1}({2}) and cnFont {3}({4})...", fileName, enFontName, enSize, cnFontName, cnSize);

			var descriptions = new List<FontDescription>();

			var des = new FontDescription(enFontName, enSize, 0f);
			foreach (var c in _englishChars)
			{
				des.Characters.Add(c);
			}
			descriptions.Add(des);

			var current = 0;
			des = new FontDescription(cnFontName, cnSize, 0f);
			for (var index = 0; index < _characters.Length; index++)
			{
				var c = _characters[index];
				if (current + 1 > MaxCharsPerPage)
				{
					descriptions.Add(des);
					des = new FontDescription(cnFontName, cnSize, 0f);
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

			Pages = _processor.Process(descriptions.ToArray(), _context);

			using (var fs = new FileStream(fileName, FileMode.Create))
			{
				_compiler.Compile(fs, new DynamicSpriteFont(0, 0, ' '), TargetPlatform.Windows, GraphicsProfile.Reach, true, Environment.CurrentDirectory, Environment.CurrentDirectory);
			}
		}

		private readonly ContentCompiler _compiler;

		private readonly DynamicFontDescriptionProcessor _processor;

		private readonly DfgContext _context;

		private readonly char[] _characters;

		private readonly List<char> _englishChars;

		private const int MaxCharsPerPage = 25 * 11;
	}
}
