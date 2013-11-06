using System;

namespace LLT
{
	public sealed class EMTreeStreamDFSEnumerator : TSTreeStreamDFSEnumerator<System.Object, TSTreeStreamEntry, EMTreeStreamDFSEnumerator>
	{
		public EMTreeStreamDFSEnumerator(ITSTreeStream tree) : base(null, tree)
		{
		}
	}
}

