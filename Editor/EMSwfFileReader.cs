using System.IO;
using System.Collections;
using System.IO.Compression;
using System.Collections.Generic;
using System;

namespace LLT
{
	public sealed class EMSwfFileReader : IEnumerable
	{
	    private EMSwfImporter _importer;
	    private string _path;
	
	    private static readonly List<KeyValuePair<uint, Type>> _parsers = new List<KeyValuePair<uint, Type>>(new KeyValuePair<uint, Type>[]
	    {
	        new KeyValuePair<uint, Type>(0, typeof(EMSwfEnd)),
	        new KeyValuePair<uint, Type>(1, typeof(EMSwfShowFrame)),
	        new KeyValuePair<uint, Type>(2, typeof(EMSwfDefineShape)),
	        new KeyValuePair<uint, Type>(9, typeof(EMSwfSetBackgroundColor)),
			new KeyValuePair<uint, Type>(20, typeof(EMSwfDefineBitsLossless)),
	        new KeyValuePair<uint, Type>(22, typeof(EMSwfDefineShape)),
	        new KeyValuePair<uint, Type>(26, typeof(EMSwfPlaceObject2)),
	        new KeyValuePair<uint, Type>(28, typeof(EMSwfRemoveObject2)),
	        new KeyValuePair<uint, Type>(32, typeof(EMSwfDefineShape)),
			new KeyValuePair<uint, Type>(35, typeof(EMSwfDefineBitsJPEG3)),
			new KeyValuePair<uint, Type>(36, typeof(EMSwfDefineBitsLossless2)),
	        new KeyValuePair<uint, Type>(39, typeof(EMSwfDefineSprite)),
			new KeyValuePair<uint, Type>(43, typeof(EMSwfFrameLabel)),
	        new KeyValuePair<uint, Type>(69, typeof(EMSwfFileAttributes)),
	        new KeyValuePair<uint, Type>(76, typeof(EMSwfSymbolClass)),
	        new KeyValuePair<uint, Type>(83, typeof(EMSwfDefineShape)),
	    });
	    private static Dictionary<uint, Type> _mapping;
	
	    private EMSwfFileReader (EMSwfImporter importer, string path)
		{
	        _importer = importer;
	        _path = path;
			if(_mapping == null)
			{
				_mapping = new Dictionary<uint, Type>();
	        	_parsers.ForEach(x => _mapping.Add(x.Key, x.Value));
			}
		}
	
		public static EMSwfFileReader Open(EMSwfImporter importer, string path)
		{
	        return new EMSwfFileReader(importer, path);
		}
	
		public IEnumerator GetEnumerator ()
		{
			return ReadNextTag ();
		}
	
		private IEnumerator ReadNextTag()
		{
	        using (var stream = new EMSwfMemoryStream(_path))
	        {
	            var binaryReader = new EMSwfBinaryReader(stream);
	            
	            var header = new EMSwfHeader(binaryReader);
	            if (header.IsCompressed())
	            {
	                using (var memoryStream = new MemoryStream())
	                {
						var copyTo = new byte[stream.Length - stream.Position];
						stream.Read(copyTo, 0, copyTo.Length);
						
	                    memoryStream.Write(copyTo, 0, copyTo.Length);
	                    memoryStream.Position = 0;
		
	                    using (var zlibStream = new Ionic.Zlib.ZlibStream(memoryStream, Ionic.Zlib.CompressionMode.Decompress))
	                    {
	                        var bytes = new List<byte>();
	                        int b;
	                        while ((b = zlibStream.ReadByte())  != -1)
	                        {
	                            bytes.Add((byte)b);
	                        }
	                        stream.Init(bytes.ToArray());
	                    }
	                }
	            }
	
	            header.Read(binaryReader);
	            yield return header;
	
	            EMSwfDefineSprite sprite = null;
	            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
	            {
	                var tag = new EMSwfTag(binaryReader);
	                var endPosition = binaryReader.BaseStream.Position + tag.Length;
	
	                Type parserType = null; 
	                if (!_mapping.TryGetValue(tag.TagType, out parserType))
	                {
	                    parserType = typeof(EMSwfUnhandledObject);
	                }
	                
	                var obj = Activator.CreateInstance(parserType) as EMSwfObject;
	                obj.Read(_importer, tag, binaryReader);
	
	                if (sprite != null)
	                {
	                    if (obj is EMSwfEnd)
	                    {
	                        yield return sprite;
	                        sprite = null;
	                    }
	                    else
	                    {
	                        sprite.AddControlTag(obj);
	                    }
	
	                    binaryReader.BaseStream.Position = endPosition;
	                }
	                else if (obj is EMSwfDefineSprite)
	                {
	                    sprite = obj as EMSwfDefineSprite;
	                }
	                else
	                {
	                    binaryReader.BaseStream.Position = endPosition;
	                    yield return obj;
	                }
	            }
	        }
		}
	}
}