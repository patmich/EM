using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using System.Reflection;

namespace LLT
{
	public sealed class EMPostprocessor : AssetPostprocessor
	{
		public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			CoreTexture2D.PngDecoder = PngDecode;
			CoreTexture2D.PngEncoder = PngEncode;
			
			foreach(var swf in importedAssets.Where(x=>Path.GetExtension(x) == ".swf"))
			{
				var import = EMSwfImporter.Import(swf, Path.GetDirectoryName(Path.GetDirectoryName(swf)) + "/EM/", "Temp/", "/Users/patricemichaud/flex_sdk_4.6/bin/adl");
				while(import.MoveNext());
			}
		}
		
		private static CoreTexture2D PngDecode(string path)
		{
			var texture2D = new Texture2D(0,0,TextureFormat.ARGB32, false);
			texture2D.LoadImage(File.ReadAllBytes(path));
			return new CoreTexture2D(texture2D.width, texture2D.height, Array.ConvertAll(texture2D.GetPixels32(), x=>(x.a << 24) + (x.r << 16) + (x.g << 8) + x.b));
		}
		
		private static void PngEncode(string path, CoreTexture2D coreTexture2D)
		{
			var texture2D = new Texture2D(coreTexture2D.Width, coreTexture2D.Height, TextureFormat.ARGB32, false);
			
			var rgba = new Color32[coreTexture2D.Width * coreTexture2D.Height];
			
			for(var i = 0; i < coreTexture2D.ARGB.Length; i++)
			{
				rgba[i].a = (byte)((coreTexture2D.ARGB[i] & 0xFF000000) >> 24);
				rgba[i].r = (byte)((coreTexture2D.ARGB[i] & 0x00FF0000) >> 16);
				rgba[i].g = (byte)((coreTexture2D.ARGB[i] & 0x0000FF00) >> 8);
				rgba[i].b = (byte)((coreTexture2D.ARGB[i] & 0x000000FF) >> 0);
			}
			
			texture2D.SetPixels32(rgba);
			texture2D.Apply(false);
			File.WriteAllBytes(path, texture2D.EncodeToPNG());
		}
	}
}