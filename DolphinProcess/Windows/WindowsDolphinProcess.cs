using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;

namespace DolphinMemoryWrapper {
    class WindowsDolphinProcess : DolphinProcess {
        public override bool findProcess() {
            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist) {
                if ((new[] { "Dolphin", "DolphinQt2", "DolphinWx" }).Contains(process.ProcessName)) {
                    this.ProcessID = process.Id;
                    this.Handle = WinAPI.OpenProcess(WinAPI.PROCESS_QUERY_INFORMATION | WinAPI.PROCESS_VM_OPERATION |
                        WinAPI.PROCESS_VM_READ | WinAPI.PROCESS_VM_WRITE, false, ProcessID);
                    // this.Handle = p.Handle;

                    return true;
                }
            }

            return false;
        }

        public override bool findEmuStartAddress() {
            WinAPI.MEMORY_BASIC_INFORMATION info = new WinAPI.MEMORY_BASIC_INFORMATION();
            WinAPI.SYSTEM_INFO sysInfo = new WinAPI.SYSTEM_INFO();
            WinAPI.GetSystemInfo(out sysInfo);

            bool AddressFound = false;
            IntPtr ptr = sysInfo.minimumApplicationAddress;
            while (WinAPI.VirtualQueryEx(this.Handle, ptr, out info, Convert.ToUInt32(Marshal.SizeOf(info))) == Marshal.SizeOf(info)) {
                if (!AddressFound) {
                    // Find Address
                    if ((long)info.RegionSize == 0x2000000 && info.Type == WinAPI.TypeEnum.MEM_MAPPED) {
                        WinAPI.PSAPI_WORKING_SET_EX_INFORMATION[] wsInfo = new WinAPI.PSAPI_WORKING_SET_EX_INFORMATION[1];
                        wsInfo[0].VirtualAddress = info.BaseAddress;
                        if (WinAPI.QueryWorkingSetEx(this.Handle, wsInfo, Marshal.SizeOf<WinAPI.PSAPI_WORKING_SET_EX_INFORMATION>())) {
                            this.EmuStartAddress = info.BaseAddress;
                            AddressFound = true;
                        }
                    }
                } else {
                    // If first address was found, check if it's backed by physical memory
                    IntPtr regionBaseAddress = info.BaseAddress;
                    IntPtr offsetPtr = IntPtr.Add(this.EmuStartAddress, 0x10000000);

                    if ((long)regionBaseAddress == (long)offsetPtr) {
                        WinAPI.PSAPI_WORKING_SET_EX_INFORMATION[] wsInfo = new WinAPI.PSAPI_WORKING_SET_EX_INFORMATION[1];
                        wsInfo[0].VirtualAddress = info.BaseAddress;
                        if (WinAPI.QueryWorkingSetEx(this.Handle, wsInfo, Marshal.SizeOf<WinAPI.PSAPI_WORKING_SET_EX_INFORMATION>())) {
                            this.EmuStartAddressBacked = true;
                        }

                        break;
                    } else if ((long)regionBaseAddress > (long)offsetPtr) {
                        this.EmuStartAddressBacked = false;
                        break;
                    }
                }

                ptr = new IntPtr((long)ptr + (long)info.RegionSize);
            }

            return this.EmuStartAddress != IntPtr.Zero;
        }

        public override bool read(long offset, MemoryType type, out byte[] buffer, bool reverseBytes = false) {
            IntPtr ptr = new IntPtr((long)this.EmuStartAddress + offset);
            IntPtr bytesRead = IntPtr.Zero;
            int bufferLength = Common.getMemoryTypeSize(type);
            buffer = new byte[bufferLength];

            bool result = WinAPI.ReadProcessMemory(this.Handle, ptr, buffer, bufferLength, out bytesRead);

            if (result && bufferLength == (long)bytesRead) {
                if (reverseBytes) {
                    Array.Reverse(buffer, 0, buffer.Length);
                }

                return true;
            }

            return false;
        }

        public override bool write(long offset, byte[] buffer, MemoryType type, bool reverseBytes = false) {
            IntPtr ptr = new IntPtr((long)this.EmuStartAddress + offset);
            IntPtr bytesRead = IntPtr.Zero;
            int bufferLength = Common.getMemoryTypeSize(type);

            bool result = WinAPI.WriteProcessMemory(this.Handle, ptr, buffer, buffer.Length, out bytesRead);

            if (result && bufferLength == (long)bytesRead) {
                if (reverseBytes) {
                    Array.Reverse(buffer, 0, buffer.Length);
                }

                return true;
            }

            return false;
        }
    }
}