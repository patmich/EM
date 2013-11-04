using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

namespace LLT
{
	[CustomEditor(typeof(EMAnimationHead))]
	public class EMAnimationHeadInspector : Editor
	{
		public override void OnInspectorGUI ()
		{
			var animationHead = target as EMAnimationHead;
			animationHead.OnInspectorGUI();
		}
	}
}