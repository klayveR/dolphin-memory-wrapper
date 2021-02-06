using System;

namespace DolphinMemoryWrapper {
    class Common {
        // Methods
        public static uint dolphinAddrToOffset(uint addr) {
            return addr &= 0x7FFFFFFF;
        }

        public static uint offsetToDolphinAddr(uint addr) {
            return addr |= 0x80000000;
        }

        public static UInt16 ReverseBytes(UInt16 value) {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        public static UInt32 ReverseBytes(UInt32 value) {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static UInt64 ReverseBytes(UInt64 value) {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }

        public static int getMemoryTypeSize(MemoryType memoryType, int length = 0) {
            switch (memoryType) {
                case MemoryType.Byte: return sizeof(byte);
                case MemoryType.Halfword: return sizeof(short);
                case MemoryType.Word: return sizeof(int);
                case MemoryType.Float: return sizeof(float);
                case MemoryType.Double: return sizeof(double);
                case MemoryType.String: return length;
                case MemoryType.ByteArray: return length;
                default: return 0;
            }
        }
    }
}
