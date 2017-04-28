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
		// ReSharper disable once NotAccessedField.Local
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
			XnaPipeLineAssembly = typeof(ContentCompiler).Assembly;

			var type = XnaPipeLineAssembly.GetType("Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentCompiler");

			var constructor = type
				.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
				.First();
			_compiler = (ContentCompiler)constructor.Invoke(null);
			_compileMethod = type.GetMethod("Compile", BindingFlags.NonPublic | BindingFlags.Instance);
			_graphics = new GraphicsDeviceManager(this);
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

				Console.Write("Start compiling font.");
				var content = _processor.Process(description, _context);
				Console.WriteLine(".Done!");

				Console.Write("Start compiling font content file: {0}", fileName);

				using (var fs = new FileStream(fileName, FileMode.Create))
				{
					_compileMethod.Invoke(_compiler,
						new object[]
						{
							fs, content, TargetPlatform.Windows, GraphicsProfile.Reach, true, Environment.CurrentDirectory,
							Environment.CurrentDirectory
						});
				}

				Console.WriteLine(" ..Done!");
				Console.WriteLine();
			}
		}

		private readonly ContentCompiler _compiler;

		private readonly MethodInfo _compileMethod;

		private readonly DfgContext _context;

		private readonly DfgImporterContext _importContext;

		private readonly ContentImporter<DynamicFontDescription> _importer;

		private readonly DynamicFontProcessor _processor;

		public readonly Assembly ReLogicPipeLineAssembly;

		public readonly Assembly XnaPipeLineAssembly;
	}
}
