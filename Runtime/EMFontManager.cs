using System.Collections.Generic;
using System;
using UnityEngine;

namespace LLT
{
	[ExecuteInEditMode]
	public sealed class EMFontManager : MonoBehaviour
	{
		[Serializable]
		private sealed class MasterInstance : IDisposable
		{
			[SerializeField]
			private EMFont _font;

			[SerializeField]
			private string _fontName;

			public EMFont Font
			{
				get
				{
					return _font;
				}
			}

			public string FontName
			{
				get
				{
					return _fontName;
				}
			}

			private int _refCount;

			public void AddRef()
			{
				_refCount++;
			}

			public bool RemoveRef()
			{
				// Ref 1 since template instance will register.
				if(--_refCount == 1)
				{
					return true;
				}
				return false;
			}

			public MasterInstance(string fontName)
			{
				_fontName = fontName;
			}

			public void Init(EMFont template)
			{
				var go = EMFont.Instantiate(template.gameObject) as GameObject;
				//GameObject.DontDestroyOnLoad(go);

				_font = go.GetComponent<EMFont>();
				_font.gameObject.name = template.name;
				_font.transform.parent = EMFontManager.Instance.transform;
			}

			public void Dispose()
			{
				CoreAssert.Fatal(_font != null);
				GameObject.DestroyImmediate(_font.gameObject);
			}
		}

		public static EMFontManager Instance
		{	
			get
			{
				return _instance;
			}
		}

		private static EMFontManager _instance;

		[SerializeField]
		private List<MasterInstance> _fonts = new List<MasterInstance>();

		private void Awake()
		{
			CoreAssert.Fatal(_instance == null);
			_instance = this;

			for(var i = 0; i < _fonts.Count; i++)
			{
				_fonts[i].Dispose();
			}
			_fonts.Clear();

			foreach(Transform child in transform)
			{
				GameObject.DestroyImmediate(child.gameObject);
			}
		}

		public EMFont Get(string fontName)
		{
			var index = _fonts.FindIndex(x => x.FontName == fontName);
			CoreAssert.Fatal(index != -1);
			return _fonts[index].Font;
		}

		public void Push(EMFont font)
		{
			var index = _fonts.FindIndex(x => x.FontName == font.FontName);
			if(index == -1)
			{
				var masterInstance = new MasterInstance(font.FontName);
				_fonts.Add(masterInstance);
				masterInstance.Init(font);

				index = _fonts.Count - 1;
			}
			_fonts[index].AddRef();
		}

		public void Pop(EMFont font)
		{
			var index = _fonts.FindIndex(x => x.FontName == font.FontName);
			CoreAssert.Fatal(index != -1);

			if(index != 0)
			{
				MasterInstance masterInstance = _fonts[index];
				if(masterInstance.RemoveRef())
				{
					_fonts.RemoveAt(index);
					masterInstance.Dispose();
				}
			}
		}
	}
}