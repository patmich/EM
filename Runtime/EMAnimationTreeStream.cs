using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLT
{
	[Serializable]
	public sealed class EMAnimationTreeStream : TSTreeStream<TSObject>
	{
		[SerializeField]
		private List<TSObject> _objects = new List<TSObject>();
		
		protected override List<TSObject> Objects
		{
			get
			{
				return _objects;
			}
		}
	}
}
