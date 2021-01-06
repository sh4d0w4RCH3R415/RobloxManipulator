using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RobloxManipulator
{
	public sealed class Injector
	{
		public static readonly IntPtr INTPTR_ZERO = (IntPtr)0;

		[DllImport("kernel32", SetLastError = true)]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

		[DllImport("kernel32", SetLastError = true)]
		private static extern int CloseHandle(IntPtr hObject);

		[DllImport("kernel32", SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[DllImport("kernel32", SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32", SetLastError = true)]
		private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32", SetLastError = true)]
		private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

		[DllImport("kernel32", SetLastError = true)]
		private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

		private static Injector _instance;

		public static Injector GetInstance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new Injector();
				}
				return _instance;
			}
		}

		Injector(){}

		public InjectionResult Inject(string sProcName, string sDllPath)
		{
			if (!File.Exists(sDllPath))
			{
				return InjectionResult.DLLNotFound;
			}

			uint _procId = 0;

			Process[] _procs = Process.GetProcesses();
			for (int i = 0; i < _procs.Length; i++)
			{
				if (_procs[i].ProcessName == sProcName)
				{
					_procId = (uint)_procs[i].Id;
					break;
				}
			}

			if (_procId == 0)
			{
				return InjectionResult.GameProcessNotFound;
			}

			if (!bInject(_procId, sDllPath))
			{
				return InjectionResult.InjectionFailed;
			}

			return InjectionResult.Success;
		}

		private bool bInject(uint pToBeInjected, string sDllPath)
		{
			IntPtr hndProc = OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, 1, pToBeInjected);

			if (hndProc == INTPTR_ZERO)
			{
				return false;
			}

			IntPtr lpLLAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

			if (lpLLAddress == INTPTR_ZERO)
			{
				return false;
			}

			IntPtr lpAddress = VirtualAllocEx(hndProc, (IntPtr)null, (IntPtr)sDllPath.Length, 0x100 | 0x2000, 0x40);

			if (lpAddress == INTPTR_ZERO)
			{
				return false;
			}

			byte[] bytes = Encoding.ASCII.GetBytes(sDllPath);

			if (WriteProcessMemory(hndProc, lpAddress, bytes, (uint)bytes.Length, 0) == 0)
			{
				return false;
			}

			if (CreateRemoteThread(hndProc, (IntPtr)null, INTPTR_ZERO, lpLLAddress, lpAddress, 0, (IntPtr)null) == INTPTR_ZERO)
			{
				return false;
			}

			CloseHandle(hndProc);

			return true;
		}
	}
}
