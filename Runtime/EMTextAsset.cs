using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace LLT
{
	[Serializable]
	public abstract class EMTextAsset : ITSTextAsset
	{
		[SerializeField]
		private TextAsset _textAsset;
		private byte[] _buffer;
		private IntPtr _ptr;
		private GCHandle _handle;

		#region ITSTextAsset implementation

		public System.IntPtr AddrOfPinnedObject ()
		{
			return _ptr;
		}

		public abstract ITSTextAsset GetInstance ();

		protected void Init(EMTextAsset textAsset)
		{
			_textAsset = textAsset._textAsset;

			if(Shared)
			{
				_buffer = textAsset.Bytes;
			}
			else
			{
				var buffer = textAsset.Bytes;
				_buffer = new byte[buffer.Length];
				System.Buffer.BlockCopy(buffer, 0, _buffer, 0, buffer.Length);
			}

			_handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
			_ptr = _handle.AddrOfPinnedObject();
		}

		public void Offset (int offset)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				_ptr = new IntPtr((byte*)_ptr + offset);
				return;
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public byte[] Bytes 
		{
			get 
			{
				if(_buffer == null)
				{
					_buffer = _textAsset.bytes;
				}
				return _buffer;
			}
		}

		#endregion

		#region ICoreTextAsset implementation

		public int ReadInt32 (int position)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				return *(int*)((byte*)AddrOfPinnedObject() + position);
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public float ReadSingle (int position)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				return *(float*)((byte*)AddrOfPinnedObject() + position);
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public byte ReadByte (int position)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				return *(byte*)((byte*)AddrOfPinnedObject() + position);
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public ushort ReadUInt16 (int position)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				return *(ushort*)((byte*)AddrOfPinnedObject() + position);
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public void Write (int position, int val)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				*(int*)((byte*)AddrOfPinnedObject() + position) = val;
				return;
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public void Write (int position, byte val)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				*(byte*)((byte*)AddrOfPinnedObject() + position) = val;
				return;
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public void Write (int position, float val)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				*(float*)((byte*)AddrOfPinnedObject() + position) = val;
				return;
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public void Write (int position, ushort val)
		{
			#if ALLOW_UNSAFE
			unsafe
			{
				*(ushort*)((byte*)AddrOfPinnedObject() + position) = val;
				return;
			}
			#endif
			throw new System.NotImplementedException ();
		}

		public abstract bool Shared { get; }

		public int Length {
			get {
				throw new System.NotImplementedException ();
			}
		}

		#endregion


	}
}