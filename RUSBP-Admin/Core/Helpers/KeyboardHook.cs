using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using System;
using System.Windows.Forms;

namespace RUSBP_Admin.Core.Helpers
{
    public static class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static IntPtr _hookId = IntPtr.Zero;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;

        public static void Install()
        {
            if (_hookId != IntPtr.Zero) return;

            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule!;
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        public static void Uninstall()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vk = Marshal.ReadInt32(lParam);

                // Bloquea: teclas de Windows y ALT+TAB
                if (vk == (int)Keys.LWin || vk == (int)Keys.RWin)
                    return (IntPtr)1;

                if (vk == (int)Keys.Tab && IsAltPressed())
                    return (IntPtr)1;
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static bool IsAltPressed() =>
            (GetAsyncKeyState((int)Keys.Menu) & 0x8000) != 0;

        // WinAPI
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}
