using System;
using UnityEngine;
using System.Collections.Generic;

namespace LLT
{
	[Serializable]
	public sealed class EMDisplayTreeStream : TSTreeStream<EMObject>
	{
		private EMDisplayTreeStreamDFSEnumerator _iter;
		
		[SerializeField]
		private List<EMObject> _objects = new List<EMObject>();
        public byte UpdateFlag { get; set; }
  
        public EMRoot Root { get; private set; }
        
		public void Init(EMRoot root, byte[] buffer)
		{
            Root = root;
			InitFromBytes(buffer, null, new EMFactory());
			_iter = new EMDisplayTreeStreamDFSEnumerator(Root);
		}
		
		protected override List<EMObject> Objects
		{
			get
			{
				return _objects;
			}
		}
		
		public override ITSTreeStreamDFSEnumerator Iter
		{
			get
			{
				return _iter;
			}
		}
	}
}