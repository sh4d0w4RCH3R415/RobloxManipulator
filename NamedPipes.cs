using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RobloxManipulator
{
	public class NamedPipes
	{
		public const string LuaPipeName = "RobloxManipulator";

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool WaitNamedPipe(string name, int timeout);

		public static bool NamedPipeExists(string pipeName)
		{
			try
			{
				if (!WaitNamedPipe($"\\\\.\\pipe\\{pipeName}", 0))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error == 0)
					{
						return false;
					}
					if (lastWin32Error == 2)
					{
						return false;
					}
				}
				return true;
			}
			catch (Exception) { return false; }
		}

		public static void LuaPipe(string script)
		{
			if (NamedPipeExists(luaPipeName))
			{
				new Thread(() =>
				{
					try
					{
						using (NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", luaPipeName, PipeDirection.Out))
						{
							namedPipeClientStream.Connect();
							using (StreamWriter streamWriter = new StreamWriter(namedPipeClientStream, Encoding.Default, 999999))
							{
								streamWriter.Write(script);
								streamWriter.Dispose();
							}
							namedPipeClientStream.Dispose();
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Error occured connecting to the pipe.", "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
				}).Start();
			}
			else
			{
				MessageBox.Show("Inject ", + Functions.ExploitDllName + " before using this!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
		}
	}
}
