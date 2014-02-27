using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace LLT
{
	public sealed class EMSwfComponents
	{
		// Union
		private readonly List<EMSwfText> _texts = new List<EMSwfText>();

		public EMSwfComponents(string xml)
		{
			if(File.Exists(xml))
			{
				using(var stream = new FileStream(xml, FileMode.Open))
				{
					var doc = new XmlDocument();
					doc.Load(stream);

					foreach(XmlNode child in doc.SelectSingleNode("entries").ChildNodes)
					{
						using(var memoryStream = new MemoryStream())
						{
							var streamWriter = new StreamWriter(memoryStream);
							streamWriter.Write(child.OuterXml);
							streamWriter.Flush();
						
							memoryStream.Position = 0;

							if(child.Name == typeof(EMSwfText).Name)
							{
								var serializer = new XmlSerializer(typeof(EMSwfText));
								_texts.Add(serializer.Deserialize(memoryStream) as EMSwfText);
							}
						}
					}
				}
			}
		}

		public bool TryGetSwfText(string name, out EMSwfText text)
		{
			var index = _texts.FindIndex(x=>x.Name == name);
			if(index == -1)
			{
				text = null;
				return false;
			}
			text = _texts[index];
			return true;
		}
	}
}