using UnityEngine;

namespace LLT
{
	[TSLayout(typeof(EMTransform), "Transform", 0)]
	[TSLayout(typeof(EMTransform), "LocalToWorld", 1)]
	[TSLayout(typeof(ushort), "Depth", 2)]
	[TSLayout(typeof(ushort), "ClipDepth", 3)]
	[TSLayout(typeof(ushort), "FontIdIndex", 4)]
	[TSLayout(typeof(ushort), "ContentIndex", 5)]
	[TSLayout(typeof(ushort), "MaxCharCount", 6)]
	public sealed partial class EMText : TSTreeStreamEntry
	{
		private EMFont _font;

		public new int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMText;
			}
		}



		public void Awake()
		{
			var fontId = _tree.GetString(FontIdIndex);

			UnityEngine.Debug.Log("Get");
			_font = EMFontManager.Instance.Get(fontId);
			CoreAssert.Fatal(_font != null);

			UnityEngine.Debug.Log(MaxCharCount);
		}
	}
}