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

		public EMAnimationTreeStream(ITSTextAsset textAsset) : base(textAsset, null, new List<TSObject>())
		{
			_iter = new EMTreeStreamDFSEnumerator(this);
		}
	}
}
