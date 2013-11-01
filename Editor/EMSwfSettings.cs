using UnityEngine;
using System.Xml.Serialization;
using System.IO;

namespace LLT
{
	public sealed class EMSwfSettings
	{
		public static EMSwfSettings Get(string swfPath)
		{
			var xmlPath = Path.ChangeExtension(swfPath, ".xml");
			if(File.Exists(xmlPath))
			{
				using(var stream = new FileStream(xmlPath, FileMode.Open))
				{
					var serializer = new XmlSerializer(typeof(EMSwfSettings));
					return serializer.Deserialize(stream) as EMSwfSettings;
				}
			}
			return new EMSwfSettings();
		}
			
		public static string GetDestinationFolder(string swfPath)
		{
			return Path.GetDirectoryName(swfPath) + "/" + Path.GetFileNameWithoutExtension(swfPath) + "/";
		}
	}
}