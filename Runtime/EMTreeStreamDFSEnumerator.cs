using System;

namespace LLT
{
	public sealed class EMTreeStreamDFSEnumerator : TSTreeStreamDFSEnumerator<TSTreeStreamEntry, EMTreeStreamDFSEnumerator>
	{
		public EMTreeStreamDFSEnumerator(ITSTreeStream tree) : base(tree)
		{
		}
	}
}

