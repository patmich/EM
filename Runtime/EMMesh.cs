using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LLT
{
	public sealed class EMMesh : IDisposable
	{
		[StructLayout(LayoutKind.Explicit, Size=20)]
		public struct EMMeshStructLayout
		{
			[FieldOffset(0)]
			public IntPtr Vertices;

			[FieldOffset(4)]
			public IntPtr UV;

			[FieldOffset(8)]
			public IntPtr ColorAdd;

			[FieldOffset(12)]
			public IntPtr ColorMult;
			
			[FieldOffset(16)]
			public int ShapeCount;
		}

		private IntPtr _meshStructLayout;

		private Vector2[] _vertices;
		private GCHandle _verticesHandle;

		private Vector2[] _uv;
		private GCHandle _uvHandle;

		private Color32[] _colorAdd;
		private GCHandle _colorAddHandle;

		private Vector4[] _colorMult;
		private GCHandle _colorMultHandle;

		public Vector2[] Vertices
		{
			get
			{
				return _vertices;
			}
		}

		public Vector2[] Uv
		{
			get
			{
				return _uv;
			}
		}

		public Color32[] ColorAdd
		{
			get
			{
				return _colorAdd;
			}
		}

		public Vector4[] ColorMult
		{
			get
			{
				return _colorMult;
			}
		}

		public EMMesh()
		{
			_meshStructLayout = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(EMMeshStructLayout)));
		}

		public void Init(int shapeCount)
		{
			FreeHandles();

			_vertices = new Vector2[shapeCount * 4];

			CoreAssert.Fatal(!_verticesHandle.IsAllocated);
			_verticesHandle = GCHandle.Alloc(_vertices, GCHandleType.Pinned);

			_uv = new Vector2[shapeCount * 4];

			CoreAssert.Fatal(!_uvHandle.IsAllocated);
			_uvHandle = GCHandle.Alloc(_uv, GCHandleType.Pinned);

			_colorAdd = new Color32[shapeCount * 4];

			CoreAssert.Fatal(!_colorAddHandle.IsAllocated);
			_colorAddHandle = GCHandle.Alloc(_colorAdd, GCHandleType.Pinned);

			_colorMult = new Vector4[shapeCount * 4];

			CoreAssert.Fatal(!_colorMultHandle.IsAllocated);
			_colorMultHandle = GCHandle.Alloc(_colorMult, GCHandleType.Pinned);

			EMMeshStructLayout meshStructLayout;
			meshStructLayout.ShapeCount = shapeCount;
			meshStructLayout.Vertices = _verticesHandle.AddrOfPinnedObject();
			meshStructLayout.UV = _uvHandle.AddrOfPinnedObject();
			meshStructLayout.ColorAdd = _colorAddHandle.AddrOfPinnedObject();
			meshStructLayout.ColorMult = _colorMultHandle.AddrOfPinnedObject();

#if ALLOW_UNSAFE
			unsafe
			{
				*(EMMeshStructLayout*)_meshStructLayout = meshStructLayout;
			}
#endif
		}

		private void FreeHandles()
		{
			if(_verticesHandle.IsAllocated)
			{
				_verticesHandle.Free();
			}
			if(_uvHandle.IsAllocated)
			{
				_uvHandle.Free();
			}
			if(_colorAddHandle.IsAllocated)
			{
				_colorAddHandle.Free();
			}
			if(_colorMultHandle.IsAllocated)
			{
				_colorMultHandle.Free();
			}
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal(_meshStructLayout);
			FreeHandles();
		}

		~EMMesh()
		{
			Dispose();
		}
	}
}