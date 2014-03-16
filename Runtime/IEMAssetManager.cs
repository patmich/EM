namespace LLT
{
	public interface IEMAssetManagerTexture2D<Texture2D>
	{
		void RegisterTexture(ITSTextAsset textAsset, int textureIndex, Texture2D texture);
		void UnregisterTexture(ITSTextAsset textAsset, int textureIndex, Texture2D texture);
		Texture2D GetTexture(ITSTextAsset textAsset, int textureIndex);
	}
	public interface IEMAssetManager<Texture2D> : IEMAssetManagerTexture2D<Texture2D> 
	{
	}
}