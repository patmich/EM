using UnityEngine;
using System;

namespace LLT
{
	[Serializable]
	public sealed class EMDisplayTreeTextAsset : EMTextAsset, IEMDisplayTreeTextAsset
	{
		public override ITSTextAsset GetInstance ()
		{
			var displayTreeTextAsset = new EMDisplayTreeTextAsset();
			displayTreeTextAsset.Init(this);
			return displayTreeTextAsset;
		}

		public override bool Shared 
		{
			get 
			{
				return false;
			}
		}
	}
}