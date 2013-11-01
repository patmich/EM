using System;
using UnityEngine;
using System.Collections;

namespace LLT
{
	[ExecuteInEditMode]
	public sealed class EMResource : MonoBehaviour
	{
		[SerializeField]
		private GameObject _prefab;
		private GameObject _instance;

		private IEnumerator Start()
		{
			while(_prefab == null)yield return null;
			
			_instance = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity) as GameObject;
			_instance.hideFlags = HideFlags.DontSave;
			
			_instance.transform.parent = transform;
			_instance.transform.localPosition = Vector3.zero;
			_instance.transform.localScale = Vector3.one;
		}
		
		private void OnDestroy()
		{
			GameObject.DestroyImmediate(_instance, true);
		}
	}
}

