using System;
using System.Threading;
using System.Windows.Forms;

namespace RobloxManipulator
{
	public class Functions
	{
		public const string ExploitDllName = "RobloxManipulator.dll";
		private static bool alreadyInjected = false;
		
		public static void Inject()
		{
			if (NamedPipes.NamedPipeExists(NamedPipes.LuaPipeName))
			{
				MessageBox.Show("Already injected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				alreadyInjected = true;
				return;
			}
			else if (!NamedPipes.NamedPipeExists(NamedPipes.LuaPipeName))
			{
				switch (Injector.GetInstance.Inject("RobloxPlayerBeta", AppDomain.CurrentDomain.BaseDirectory + ExploitDllName))
				{
					case InjectionResult.DLLNotFound:
						MessageBox.Show($"Couldn't find {ExploitDllName}!", "Dll was not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					case InjectionResult.GameProcessNotFound:
						MessageBox.Show("Couldn't find RobloxPlayerBeta.exe!", "Target process was not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					case InjectionResult.InjectionFailed:
						MessageBox.Show("Injection failed!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
				}
				alreadyInjected = false;
			}
			Thread.Sleep(3000);
			if (!NamedPipes.NamedPipeExists(NamedPipes.LuaPipeName))
			{
				MessageBox.Show("Injection failed!\nMaybe you are missing or took more time to check if was ready or other stuff.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				alreadyInjected = false;
			}
		}
		public static void Execute(string script)
		{
			if (alreadyInjected)
			{
				NamedPipes.LuaPipe(script);
			}
			else if (!alreadyInjected)
			{
				Inject();
			}
		}

		public static readonly OpenFileDialog OpenFileDialog = new OpenFileDialog
		{
			Title = "Select a script...",
			Filter = "Lua Script|*.lua;|Normal Script|*.txt;|Script|*.lua; *.txt;|All Types (*.*)|*.*;",
			FilterIndex = 1,
			RestoreDirectory = true,
		};
		public static readonly SaveFileDialog SaveFileDialog = new SaveFileDialog
		{
			Title = "Save your script...",
			Filter = "Lua Script|*.lua;|Normal Script|*.txt;|Script|*.lua; *.txt;|All Types (*.*)|*.*;",
			FilterIndex = 1,
			RestoreDirectory = true,
		};
	}
}
