using System;
using UnityEditor;
using System.IO;
using System.Linq;

namespace LLT
{
	public static class EMLayoutTool
	{
		[MenuItem("LLT/Generate Layout Code")]
		private static void GenerateCode()
		{
			GenerateCode(typeof(EMSprite));
			GenerateCode(typeof(EMShape));
			GenerateCode(typeof(EMTransform));
			GenerateCode(typeof(TSTreeStreamTag));
			GenerateCode(typeof(EMAnimation));
			GenerateCode(typeof(EMAnimationClip));
			GenerateCode(typeof(EMAnimationKeyframe));
			GenerateCode(typeof(EMAnimationKeyframeValue));
			GenerateCode(typeof(EMRect));
		}
				
		private static void GenerateCode(Type type)
		{
			var files = Directory.GetFiles(Path.GetFullPath("Assets"), type.Name + ".cs", SearchOption.AllDirectories).Where(x=>!(x.Contains("Generated")));
			if(files.Count() != 1)
			{
				throw new Exception("Failed looking for source file: " + type);
			}
			TSLayoutTool.Execute(type, Path.GetDirectoryName(files.First()));
		}
	}
}

