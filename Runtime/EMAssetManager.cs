using UnityEngine;
using System.Collections.Generic;

namespace LLT
{
	public sealed class EMAssetManager : IEMAssetManager<Texture2D>
	{
		public static readonly EMAssetManager Instance = new EMAssetManager();

		public void RegisterTexture (ITSTextAsset textAsset, int textureIndex, Texture2D texture)
		{
		}

		public void UnregisterTexture (ITSTextAsset textAsset, int textureIndex, Texture2D texture)
		{
		}

		public Texture2D GetTexture (ITSTextAsset textAsset, int textureIndex)
		{
			return null;
		}
	}
}

