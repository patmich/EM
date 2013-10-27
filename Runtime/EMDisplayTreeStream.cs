using System;
using UnityEngine;
using System.Collections.Generic;

namespace LLT
{
	[Serializable]
	public sealed class EMDisplayTreeStream : TSTreeStream<EMObject>
	{
		[SerializeField]
		private List<EMObject> _objects = new List<EMObject>();
		
		protected override List<EMObject> Objects
		{
			get
			{
				return _objects;
			}
		}
	}
}