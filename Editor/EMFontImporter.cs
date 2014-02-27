using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using System;

namespace LLT
{
	public sealed class EMFontImporter
	{
		private string _source;
		private string _destinationFolder;
		private string _destination;
		private string _temporaryFolder;

		private EMFontImporter(string source, string destinationFolder, string temporaryFolder)
		{
			_source = source;
			_destinationFolder = destinationFolder;
			
			Directory.CreateDirectory(_destinationFolder);
			
			_temporaryFolder = string.Format("{0}/{1}/", temporaryFolder, Path.GetFileNameWithoutExtension(source));
			if(Path.GetFullPath(_temporaryFolder).Contains(Directory.GetCurrentDirectory()))
			{
				if (Directory.Exists(_temporaryFolder))
				{
					Directory.GetFiles(_temporaryFolder, "*.png").ToList().ForEach(x => File.Delete(x));
					Directory.Delete(_temporaryFolder, true);
				}
			}
		}
		
		public static IEnumerator Import(string source, string destination, string temporaryFolder)
		{
			var importer = new EMFontImporter(source, destination, temporaryFolder);
			return importer.ImportInternal();
		}
		
		private IEnumerator ImportInternal()
		{
			var invoke = EMADLInvoke.Invoke("Tools/EMSwfText/Bin/EMSwfText_Rasterizer-app.xml", string.Format("{0} {1}", Path.GetFullPath(_source), Path.GetFullPath(_temporaryFolder)));
			while(invoke.MoveNext())
			{
				System.Threading.Thread.Sleep(0);
			}

			int temp;

			var texturePaths = Directory.GetFiles(_temporaryFolder, "*.png", SearchOption.AllDirectories)
				.Where(x=>int.TryParse(Path.GetFileNameWithoutExtension(x), out temp))
					.OrderBy(x=>int.Parse(Path.GetFileNameWithoutExtension(x)))
					.ToList();

			var textures = texturePaths.ConvertAll(x=>new CoreTexture2D(x))
					.ToArray();

			CoreTexture2D atlas = null;
			CoreRect[] uv;
			CoreTexture2D.Pack(textures, 0 , out atlas, out uv);
			atlas.Save(string.Format("{0}/atlas.png", _destinationFolder));

			var xmlSerializer = new XmlSerializer(typeof(EMFontDefinition));
			using(var stream = new FileStream(_temporaryFolder + "definition.xml", FileMode.Open))
			{
				var font = xmlSerializer.Deserialize(stream) as EMFontDefinition;

				foreach(var charInfo in font.Chars)
				{
					var index = texturePaths.FindIndex(x=>int.Parse(Path.GetFileNameWithoutExtension(x)) == charInfo.Code);
					if(index != -1)
					{
						charInfo.PackX = (short)Math.Round(uv[index].X * atlas.Width);
						charInfo.PackY = (short)Math.Round(uv[index].Y * atlas.Height);
					}
				}

				File.WriteAllBytes(_destinationFolder + "Definition.bytes", font.Serialize());
			}
			yield break;
		}
	}
}