using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Reflection;
using ReLogic.Content.Pipeline;
using System.Linq;

namespace DynamicFontGenerator
{
	public sealed class Generator : Game
	{
		private readonly GraphicsDeviceManager _graphics;

		private static void Main()
		{
			using (var game = new Generator())
			{
				game.Run();
			}
		}

		public Generator()
		{
			ReLogicPipeLineAssembly = typeof(DynamicFontDescription).Assembly;

			_graphics = new GraphicsDeviceManager(this);
			_compiler = new ContentCompiler();
			_context = new DfgContext(this);
			_importContext = new DfgImporterContext();
			_importer = (ContentImporter<DynamicFontDescription>)Activator.CreateInstance(ReLogicPipeLineAssembly.GetType("ReLogic.Content.Pipeline.DynamicFontImporter"));
			_processor = new DynamicFontProcessor();

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
			var descFiles = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dynamicfont").ToList();

			Console.WriteLine("Description file detected: {0}", descFiles.Count);

			foreach (var descFilePath in descFiles)
			{
				var descFileName = Path.GetFileName(descFilePath);

				Console.WriteLine("* {0}", descFileName);
			}

			Console.WriteLine();

			foreach (var descFilePath in descFiles)
			{
				var descFileName = Path.GetFileName(descFilePath);

				Console.Write("Start loading description file: {0}", descFileName);

				var description = _importer.Import(descFilePath, _importContext);
				Console.WriteLine(" ..Done!");

				var fileName = Path.GetFileNameWithoutExtension(descFileName) + ".xnb";

				Console.Write("Start compiling font content file: {0}", fileName);

				var content = _processor.Process(description, _context);

				using (var fs = new FileStream(fileName, FileMode.Create))
				{
					_compiler.Compile(fs, content, TargetPlatform.Windows, GraphicsProfile.Reach, true, Environment.CurrentDirectory, Environment.CurrentDirectory);
				}

				Console.WriteLine(" ..Done!");
				Console.WriteLine();
			}
		}

		private readonly ContentCompiler _compiler;

		private readonly DfgContext _context;

		private readonly DfgImporterContext _importContext;

		private readonly ContentImporter<DynamicFontDescription> _importer;

		private readonly DynamicFontProcessor _processor;

		public readonly Assembly ReLogicPipeLineAssembly;
	}
}
