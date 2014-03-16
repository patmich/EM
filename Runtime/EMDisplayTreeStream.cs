using System;
using UnityEngine;
using System.Collections.Generic;

namespace LLT
{
	public sealed class EMDisplayTreeStream : TSTreeStream<EMObject>, IEMRoot
	{
		private IEMRoot _root;
		private EMDisplayTreeStreamDFSEnumerator _iter;

		public EMDisplayTreeStream(IEMRoot root, ITSTextAsset textAsset, List<EMObject> objects) : base(textAsset, null, objects)
		{
			_root = root;
			_iter = new EMDisplayTreeStreamDFSEnumerator(this);
		}

		public override ITSTreeStreamDFSEnumerator Iter
		{
			get
			{
				return _iter;
			}
		}

		#region IEMRoot implementation
		
		public EMTransformStructLayout Transform 
		{
			get 
			{
				return _root.Transform;
			}
		}
		
		#endregion
	}
}