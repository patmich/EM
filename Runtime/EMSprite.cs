using System;
using UnityEngine;
using System.Collections;

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
		
		public IEnumerator Link(EMRoot root)
		{
            var displayTree = _tree as EMDisplayTreeStream;
            var link = root.Link(displayTree);
            while(link.MoveNext())yield return null;
            
            displayTree.UpdateFlag |= EMUpdateFlag.Flag(EMUpdateFlag.Flags.InitMesh, EMUpdateFlag.Flags.UpdateDrawCalls);
            
            var tag = new TSTreeStreamTag(_tree);
            tag.Position = Position - TSTreeStreamTag.TSTreeStreamTagSizeOf;
            
			_tree.Link(tag, link.Current);
		}
	}
}
