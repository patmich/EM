using System;
using UnityEngine;
using System.Collections;

namespace LLT
{
	[Serializable]
	[TSLayout(typeof(EMTransform), "Transform", 0)]
	[TSLayout(typeof(EMTransform), "LocalToWorld", 1)]
	[TSLayout(typeof(int), "Depth", 2)]
	[TSLayout(typeof(int), "ClipDepth", 3)]
	[TSLayout(typeof(int), "SpriteIndex", 4)]
	[TSLayout(typeof(byte), "UpdateFlag", 5)]
	public sealed partial class EMSprite : TSTreeStreamEntry
	{
		public new int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMSprite;
			}
		}
	}
}
