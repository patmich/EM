using System;
using System.Collections.Generic;

namespace LLT
{
	public sealed class EMSwfCurveKey : IComparer<EMSwfCurveKey>
	{
		private int _childIndex;
		private int _offset;
		private TSPropertyType _type;
		
		public int ChildIndex
		{
			get
			{
				return _childIndex;
			}
		}
		
		public int Offset
		{
			get
			{
				return _offset;
			}
		}
		
		public TSPropertyType PropertyType
		{
			get
			{
				return _type;
			}
		}
		
		public EMSwfCurveKey(int childIndex, int offset, TSPropertyType propertyType)
		{
			_childIndex = childIndex;
			_offset = offset;
			_type = propertyType;
		}
		
		public EMSwfCurveKey(int childIndex, int offset)
		{
			_childIndex = childIndex;
			_offset = offset;
			_type = TSPropertyType._float;
		}
		
		public override int GetHashCode ()
		{
			return _childIndex * 1000 + _offset;
		}
		
		public override bool Equals (object obj)
		{
			var curveKey = (EMSwfCurveKey)obj;
			var retVal = _childIndex == curveKey._childIndex && _offset == curveKey._offset;
			CoreAssert.Fatal(!retVal || _type == curveKey._type);
			return retVal;
		}
		
		public int Compare(EMSwfCurveKey x, EMSwfCurveKey y)
	    {
	        var retVal = x._childIndex.CompareTo(y._childIndex);
	        if (retVal == 0)
	        {
	            retVal = x._offset.CompareTo(y._offset);
	        }
	
	        return retVal;
	    }
		public override string ToString ()
		{
			return string.Format ("[EMSwfCurveKey: ChildIndex={0}, Offset={1}, PropertyType={2}]", ChildIndex, Offset, PropertyType);
		}
	}
}