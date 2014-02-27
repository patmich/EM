
namespace LLT
{
	public sealed class EMSwfText
	{
		public string Name { get; private set; }
		public int Frame { get; private set; }
		public int Size { get; private set; }
		public string Content { get; private set; }
		public string FontId { get; private set; }
		public int MaxCharCount { get; private set; }
		public override string ToString ()
		{
			return string.Format ("[EMSwfText: Name={0}, Frame={1}, Size={2}, Content={3}, FontId={4}]", Name, Frame, Size, Content, FontId);
		}
	}
}