using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RUSBP_Admin.Core.Helpers
{
    public static class CursorGuard
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")] private static extern bool ClipCursor(ref RECT rect);
        [DllImport("user32.dll")] private static extern bool ClipCursor(IntPtr rect);      // libera
        [DllImport("user32.dll")] private static extern bool GetClipCursor(out RECT lpRect);

        /// <summary>Restringe el cursor a las coordenadas absolutas indicadas.</summary>
        public static void Restrict(Rectangle screenArea)
        {
            RECT r = new()
            {
                Left = screenArea.Left,
                Top = screenArea.Top,
                Right = screenArea.Right,
                Bottom = screenArea.Bottom
            };

            if (!ClipCursor(ref r))
                MessageBox.Show("ClipCursor falló (¿sin privilegios?).", "CursorGuard");
        }

        /// <summary>Restringe al área interior de un control (en coordenadas de pantalla).</summary>
        public static void RestrictToControl(Control ctl, Padding margin)
        {
            var tl = ctl.PointToScreen(new Point(margin.Left, margin.Top));
            var br = ctl.PointToScreen(new Point(ctl.ClientSize.Width - margin.Right,
                                                 ctl.ClientSize.Height - margin.Bottom));

            Restrict(new Rectangle(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y));
        }

        public static void Release() => ClipCursor(IntPtr.Zero);

        public static Rectangle? CurrentArea()
        {
            return GetClipCursor(out RECT r)
                ? new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top)
                : null;
        }
    }
}
