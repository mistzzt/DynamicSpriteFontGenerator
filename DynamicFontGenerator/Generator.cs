using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

using GdiColor = System.Drawing.Color;
using XnaColor = Microsoft.Xna.Framework.Color;

using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using System.Windows.Forms;

namespace DynamicFontGenerator
{
	internal class Generator : Game
	{
		private readonly GraphicsDeviceManager _graphics;

		internal static Tuple<Texture2DContent, FontPage>[] _pages;

		private static FontFamily _cnFont;

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
			LoadFonts();

			base.LoadContent();
		}

		private void CompileFonts()
		{
			Console.WriteLine("Start compiling...");

			CompileFont(new Font(_cnFont, 17.55F, GraphicsUnit.Pixel), "Mouse_Text.xnb");
			CompileFont(new Font(_cnFont, 16.2F, GraphicsUnit.Pixel), "Item_Stack.xnb");
			CompileFont(new Font(_cnFont, 17.55F, GraphicsUnit.Pixel), "Death_Text.xnb");
			CompileFont(new Font(_cnFont, 20.25F, GraphicsUnit.Pixel), "Combat_Crit.xnb");
			CompileFont(new Font(_cnFont, 17.55F, GraphicsUnit.Pixel), "Combat_Text.xnb");
		}

		private void CompileFont(Font font, string fileName)
		{
			var compiler = new ContentCompiler();

			_pages = CompileFontPages(font);

			using (var fs = new FileStream(fileName, FileMode.Create))
			{
				compiler.Compile(fs, new DynamicSpriteFont(0, 0, ' '), TargetPlatform.Windows, GraphicsProfile.Reach, false, Environment.CurrentDirectory, Environment.CurrentDirectory);
			}
		}

		private static Tuple<Texture2DContent, FontPage>[] CompileFontPages(Font font)
		{
			var bitMap = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
			var graphics = Graphics.FromImage(bitMap);

			graphics.Clear(GdiColor.Empty);
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

			var chars = File.ReadAllText("chars.txt").ToCharArray();

			int maxHeight = 0, currentY = 0;
			int currentX = 0;
			int charHeight = 0, charWidth = 0;

			var pages = new List<Tuple<Texture2DContent, FontPage>>();

			List<XnaRectangle> glyphs = new List<XnaRectangle>(),
								padding = new List<XnaRectangle>();
			var characters = new List<char>();
			var kerning = new List<Vector3>();

			foreach (var c in chars.Distinct())
			{
				var str = c.ToString();

				var size = graphics.MeasureString(str, font, PointF.Empty, StringFormat.GenericTypographic);
				charHeight = (int)Math.Ceiling(size.Height);
				charWidth = (int)Math.Ceiling(size.Width);

				maxHeight = Math.Max(charHeight, maxHeight);

				if (currentX + charWidth > 256)
				{
					InternalNewLine();
				}

				graphics.DrawString(str, font, Brushes.White, currentX, currentY);

				glyphs.Add(new XnaRectangle(currentX + 3, currentY, charWidth, charHeight));
				padding.Add(new XnaRectangle(0, 0, charWidth, 30));
				characters.Add(c);
				kerning.Add(new Vector3(0f, charWidth, 0f));

				currentX += charWidth;
			}

			return pages.ToArray();

			void InternalNewLine()
			{
				currentX = 0;
				currentY += maxHeight;
				maxHeight = charHeight;
				if (currentY + maxHeight > 256)
				{
					var t2d = ToT2DContent(bitMap);

					pages.Add(new Tuple<Texture2DContent, FontPage>(t2d, new FontPage(null, new List<XnaRectangle>(glyphs), new List<XnaRectangle>(padding), new List<char>(characters), new List<Vector3>(kerning))));

					maxHeight = currentX = currentY = 0;

					glyphs.Clear();
					padding.Clear();
					characters.Clear();
					kerning.Clear();
					graphics.Clear(GdiColor.Empty);
				}
			}
		}

		private static Texture2DContent ToT2DContent(Bitmap bitMap)
		{
			var content = new Texture2DContent();

			var mipmap = new PixelBitmapContent<XnaColor>(bitMap.Width, bitMap.Height);

			for (var i = 0; i < bitMap.Width; i++)
			{
				for (var j = 0; j < bitMap.Height; j++)
				{
					var gdiColor = bitMap.GetPixel(i, j);
					var color = new XnaColor(gdiColor.R, gdiColor.G, gdiColor.B, gdiColor.A);

					var num = color.A / 255f;
					var a = (int)color.A;
					var r = (int)(num * color.R);
					var g = (int)(num * color.G);
					var b = (int)(num * color.B);
					color.PackedValue = (uint)((a << 24) + r + (g << 8) + (b << 16));

					mipmap.SetPixel(i, j, color);
				}
			}

			content.Mipmaps.Add(mipmap);

			content.ConvertBitmapType(typeof(Dxt3BitmapContent));

			return content;
		}

		private static void LoadFonts()
		{
			const string fontFileName = "Font.tt*";

			var collection = new PrivateFontCollection();
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), fontFileName);
			for (var index = 0; index < files.Length; index++)
			{
				collection.AddFontFile(files[index]);
			}
			if (collection.Families.Length != 1)
			{
				MessageBox.Show("加载字体失败; 请确保游戏目录下有且只有一个 Font.ttf 或 Font.ttc 字体文件!", "DSFC");
				Environment.Exit(1);
			}

			var fontName = collection.Families.First().Name;
			var font = new FontFamily(fontName, collection);

			_cnFont = font;

			//Main.fontMouseText = new SpriteFontCn(new Font(_cnFont, 17.55F, GraphicsUnit.Pixel));
			//Main.fontItemStack = new SpriteFontCn(new Font(_cnFont, 16.2F, GraphicsUnit.Pixel));
			//Main.fontDeathText = new SpriteFontCn(new Font(_cnFont, 33.75F, GraphicsUnit.Pixel));
			//Main.fontCombatText[1] = new SpriteFontCn(new Font(_cnFont, 20.25F, GraphicsUnit.Pixel));
			//Main.fontCombatText[0] = new SpriteFontCn(new Font(_cnFont, 17.55F, GraphicsUnit.Pixel));
		}
	}
}
