using System;
using UnityEngine;

namespace LLT
{
	[Serializable]
	[TSLayout(typeof(EMTransform), "Transform", 0)]
	[TSLayout(typeof(EMTransform), "LocalToWorld", 1)]
	[TSLayout(typeof(ushort), "ClipCount", 2)]
	[TSLayout(typeof(byte), "UpdateFlag", 3)]
	public sealed partial class EMSprite : TSTreeStreamEntry
	{
		public new int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMSprite;
			}
		}
		
		internal void InternalAwake()
		{
			
		}
		
		public void Link(EMRoot root)
		{
			var tag = new TSTreeStreamTag(_tree);
			tag.Position = Position - TSTreeStreamTag.TSTreeStreamTagSizeOf;
			
			root.gameObject.SetActive(false); 
			_tree.Link(tag, new EMTreeStreamDFSEnumerator(root));
		}
	}
}
