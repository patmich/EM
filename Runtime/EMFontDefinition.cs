using System.Collections.Generic;
using System.IO;

namespace LLT
{
	public sealed class EMFontDefinition
	{
		public sealed class CharInfo
		{
			public int Code { get; set; }
			public short OffsetLeft { get; set; }
			public short OffsetRight { get; set; }
			public short OffsetTop { get; set; }
			public short Width { get; set; }
			public short Height { get; set; }
			public short PackX { get; set; }
			public short PackY { get; set; }

		}

		public string FontId { get; set; }
		public int LineHeight { get; set; }
		public List<CharInfo> Chars { get; set; }

		private Dictionary<int, CharInfo> _chars;

		public byte[] Serialize()
		{
			using(var stream = new MemoryStream())
			{
				var binaryWriter = new BinaryWriter(stream);

				binaryWriter.Write(FontId);
				binaryWriter.Write(LineHeight);
				binaryWriter.Write((int)Chars.Count);

				for(var i = 0; i < Chars.Count; i++)
				{
 					binaryWriter.Write(Chars[i].Code);
					binaryWriter.Write(Chars[i].OffsetLeft);
					binaryWriter.Write(Chars[i].OffsetRight);
					binaryWriter.Write(Chars[i].OffsetTop);
					binaryWriter.Write(Chars[i].Width);
					binaryWriter.Write(Chars[i].Height);
					binaryWriter.Write(Chars[i].PackX);
					binaryWriter.Write(Chars[i].PackY);
				}

				var retVal = new byte[stream.Position];
				stream.Position = 0;
				stream.Read(retVal, 0, retVal.Length);
				return retVal;
			}
		}

		public void Deserialize(byte[] bytes)
		{
			_chars = new Dictionary<int, CharInfo>();
			using(var stream = new MemoryStream(bytes))
			{
				var binaryReader = new BinaryReader(stream);

				FontId = binaryReader.ReadString();
				LineHeight = binaryReader.ReadInt32();
				var count = binaryReader.ReadInt32();
				
				for(var i = 0; i < count; i++)
				{
					var charInfo = new CharInfo()
					{
						Code = binaryReader.ReadInt32(),
						OffsetLeft = binaryReader.ReadInt16(),
						OffsetRight = binaryReader.ReadInt16(),
						OffsetTop = binaryReader.ReadInt16(),
						Width = binaryReader.ReadInt16(),
						Height = binaryReader.ReadInt16(),
						PackX = binaryReader.ReadInt16(),
						PackY = binaryReader.ReadInt16(),
					};

					_chars.Add(charInfo.Code, charInfo);
				}
			}
		}
	}
}