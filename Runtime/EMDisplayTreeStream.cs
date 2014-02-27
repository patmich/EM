using System;
using UnityEngine;
using System.Collections.Generic;

namespace LLT
{
	public sealed class EMDisplayTreeStream : TSTreeStream<EMObject>
	{
		private EMDisplayTreeStreamDFSEnumerator _iter;
        public byte UpdateFlag { get; set; }
        public EMRoot Root { get; private set; }
        
		public void Init(EMRoot root, byte[] buffer)
		{
            Root = root;
			InitFromBytes(buffer, null, new EMFactory());
			_iter = new EMDisplayTreeStreamDFSEnumerator(Root);

            foreach(var comp in root.GetComponents<EMComponent>())
            {
                comp.InitSerializedComponent(this);
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