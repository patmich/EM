using System;

namespace LLT
{
    public static class EMUpdateFlag
    {
        public enum Flags
        {
            InitMesh,
            UpdateDrawCalls,
        }
        
        public static byte Flag(params Flags[] flags)
        {
            byte retVal = 0;
            for(var i = 0; i < flags.Length; i++)
            {
                retVal |= (byte)(1 << (byte)flags[i]);
            }
            return retVal;
        }
        
        public static bool Should(byte val, Flags flag)
        {
            return (val & (1 << (byte)flag)) > 0;
        }
    }
}

