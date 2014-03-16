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
        private EMNewRoot _root;
        
		public EMNewRoot Root
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
			_instance.layer = gameObject.layer;

            _root = _instance.GetComponent<EMNewRoot>();
		}
        
        public static EMResource Load(string path)
        {
            var prefab = Resources.Load(path) as GameObject;
            CoreAssert.Fatal(prefab != null);
            
            var instance = Instantiate(prefab) as GameObject;
            CoreAssert.Fatal(instance != null);
            
            var resource = instance.GetComponent<EMResource>();
            CoreAssert.Fatal(resource != null);

            resource._prefab = prefab;
            resource._instance = instance;

            return resource;
        }
	}
}

