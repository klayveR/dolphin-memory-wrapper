using System;

namespace DolphinMemoryWrapper {
    public abstract class DolphinProcess {
        public IntPtr Handle { get; set; }
        public int ProcessID { get; protected set; }
        public IntPtr EmuStartAddress { get; protected set; }
        public bool EmuStartAddressBacked { get; protected set; }

        public abstract bool findProcess();
        public abstract bool findEmuStartAddress();
        public abstract bool read(long offset, MemoryType type, out byte[] buffer, bool reverseBytes = false);
        public abstract bool write(long offset, byte[] buffer, MemoryType type, bool reverseBytes = false);
    }
}