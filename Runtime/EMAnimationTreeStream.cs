using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLT
{
	[Serializable]
	public sealed class EMAnimationTreeStream : TSTreeStream<TSObject>
	{
		private EMTreeStreamDFSEnumerator  _iter;
		
		public override ITSTreeStreamDFSEnumerator Iter
		{
			get
			{
				return _iter;
			}
		}
		
		public void Init(byte[] buffer)
		{
			InitFromBytes(buffer, null, new EMFactory());
			_iter = new EMTreeStreamDFSEnumerator(this);
		}
	}
}
