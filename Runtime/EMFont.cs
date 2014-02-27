using UnityEngine;

namespace LLT
{
	[ExecuteInEditMode]
	public sealed class EMFont : MonoBehaviour
	{
		[SerializeField]
		private string _fontName;

		[SerializeField]
		private TextAsset _fontDefinitionRef;

		[SerializeField]
		private Texture _texture;

		private EMFontDefinition _fontDefinition;
		public EMFontDefinition FontDefinition
		{
			get
			{
				if(_fontDefinition == null)
				{
					_fontDefinition = new EMFontDefinition();
					_fontDefinition.Deserialize(_fontDefinitionRef.bytes);
				}
				return _fontDefinition;
			}
		}

		public string FontName
		{
			get
			{
				CoreAssert.Fatal(_fontName == FontDefinition.FontId);
				return _fontName;
			}
		}

		private void Awake()
		{
			EMFontManager.Instance.Push(this);
		}

		private void OnDestroy()
		{
			if(EMFontManager.Instance != null)
			{
				EMFontManager.Instance.Pop(this);
			}
		}
	}
}