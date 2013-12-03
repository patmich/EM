using System;
using UnityEngine;
using System.Collections;

namespace LLT
{
	[Serializable]
	[TSLayout(typeof(EMTransform), "Transform", 0)]
	[TSLayout(typeof(EMTransform), "LocalToWorld", 1)]
	[TSLayout(typeof(ushort), "Depth", 2)]
	[TSLayout(typeof(ushort), "ClipDepth", 3)]
    [TSLayout(typeof(ushort), "SpriteIndex", 4)]
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
		
		internal void InternalAwake()
		{
			
		}
		
		public IEnumerator Link(EMResource resource)
		{
            var displayTree = _tree as EMDisplayTreeStream;
            
            resource.transform.parent = displayTree.Root.transform;
            resource.transform.localPosition = Vector3.zero;
            resource.transform.localRotation = Quaternion.identity;
            resource.transform.localScale = Vector3.one;
                
            var link = resource.Root.Link(displayTree);
            while(link.MoveNext())yield return null;
            
            displayTree.UpdateFlag |= EMUpdateFlag.Flag(EMUpdateFlag.Flags.InitMesh, EMUpdateFlag.Flags.UpdateDrawCalls);
            
            var tag = new TSTreeStreamTag(_tree);
            tag.Position = Position - TSTreeStreamTag.TSTreeStreamTagSizeOf;
            
			_tree.Link(tag, link.Current);
		}
		
        public IEnumerator Link(GameObject prefab)
        {
            var displayTree = _tree as EMDisplayTreeStream;
            
            var go = GameObject.Instantiate(prefab) as GameObject;
           
            go.transform.parent = displayTree.Root.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var root = go.GetComponent<EMRoot>();

            if(root != null)
            {
                var link = root.Link(displayTree);
                while(link.MoveNext())yield return null;
                
                displayTree.UpdateFlag |= EMUpdateFlag.Flag(EMUpdateFlag.Flags.InitMesh, EMUpdateFlag.Flags.UpdateDrawCalls);
                
                var tag = new TSTreeStreamTag(_tree);
                tag.Position = Position - TSTreeStreamTag.TSTreeStreamTagSizeOf;
                
                _tree.Link(tag, link.Current);
            }


        }
        
				
		public Vector2 Pos
		{
			get
			{
				return new Vector2(LocalToWorld.M02, LocalToWorld.M12);
			}
		}
	}
}
