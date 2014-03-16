using UnityEngine;
using System;

namespace LLT
{
	[Serializable]
	public sealed class EMAnimationTreeTextAsset : EMTextAsset, IEMAnimationTreeTextAsset
	{
		public override ITSTextAsset GetInstance ()
		{
			var animationTreeTextAsset = new EMAnimationTreeTextAsset();
			animationTreeTextAsset.Init(this);
			return animationTreeTextAsset;
		}

		public override bool Shared 
		{
			get 
			{
				return true;
			}
		}
	}
}