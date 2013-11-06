using System;
using UnityEngine;
using System.Collections;

namespace LLT
{
	public sealed class EMResource : MonoBehaviour
	{
		[SerializeField]
		private GameObject _prefab;
		private GameObject _instance;
        private EMRoot _root;
        
        public EMRoot Root
        {
            get
            {
                return _root;
            }
        }
        
		private void Awake()
		{
            CoreAssert.Fatal(_prefab != null);
            
			_instance = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity) as GameObject;
			_instance.transform.parent = transform;
			_instance.transform.localPosition = Vector3.zero;
			_instance.transform.localScale = Vector3.one;
            
            _root = _instance.GetComponent<EMRoot>();
		}
        
        public static EMResource Load(string path)
        {
            var prefab = Resources.Load(path);
            CoreAssert.Fatal(prefab != null);
            
            var go = Instantiate(prefab) as GameObject;
            CoreAssert.Fatal(go != null);
            
            var resource = go.GetComponent<EMResource>();
            CoreAssert.Fatal(resource != null);
            
            return resource;
        }
	}
}

