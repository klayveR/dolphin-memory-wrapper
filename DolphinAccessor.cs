using System;

namespace DolphinMemoryWrapper {
    public enum DolphinStatus {
        Hooked,
        NotRunning,
        NotEmulating,
        Unhooked
    }

    public class DolphinAccessor {
        protected DolphinProcess Process { get; set; }
        public DolphinStatus Status { get; protected set; }
        public IntPtr EmuStartAddress {
            get {
                return this.Process.EmuStartAddress;
            }
        }
        public bool EmuStartAddressBacked {
            get {
                return this.Process.EmuStartAddressBacked;
            }
        }

        public DolphinAccessor() {
            this.Process = getNewDolphinProcess();
            this.Status = DolphinStatus.Unhooked;
        }

        public bool hook() {
            if (!this.Process.findProcess()) {
                this.Status = DolphinStatus.NotRunning;
                Console.WriteLine("Dolphin not running");
            } else if (!this.Process.findEmuStartAddress()) {
                this.Status = DolphinStatus.NotEmulating;
                Console.WriteLine("Dolphin not emulating");
            } else {
                this.Status = DolphinStatus.Hooked;
                Console.WriteLine("Dolphin hooked");
                return true;
            }

            return false;
        }

        public void unhook() {
            this.Process = getNewDolphinProcess();
            this.Status = DolphinStatus.Unhooked;
        }

        public bool read(long offset, MemoryType type, out byte[] buffer, bool reverseBytes = false) {
            if (this.Status == DolphinStatus.Hooked) {
                return this.Process.read(offset, type, out buffer, reverseBytes);
            }

            buffer = new byte[0];
            return false;
        }

        public bool write(long offset, byte[] buffer, MemoryType type, bool reverseBytes = false) {
            if (this.Status == DolphinStatus.Hooked) {
                return this.Process.write(offset, buffer, type, reverseBytes);
            }

            return false;
        }

        protected DolphinProcess getNewDolphinProcess() {
            // TODO: Linux
            return new WindowsDolphinProcess();
        }
    }
}