using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Dungeons2SkinLoader
{
    // =================================================================== theme
    public static class Theme
    {
        // blue / black palette
        public static readonly Color Bg = Color.FromArgb(7, 9, 15);
        public static readonly Color Panel = Color.FromArgb(12, 16, 26);
        public static readonly Color Card = Color.FromArgb(20, 27, 43);
        public static readonly Color CardHi = Color.FromArgb(29, 39, 62);
        public static readonly Color Line = Color.FromArgb(38, 52, 82);
        public static readonly Color Text = Color.FromArgb(234, 240, 255);
        public static readonly Color Dim = Color.FromArgb(134, 150, 184);
        public static readonly Color Accent = Color.FromArgb(56, 142, 255);
        public static readonly Color AccentHi = Color.FromArgb(98, 170, 255);
        public static readonly Color AccentDeep = Color.FromArgb(22, 82, 180);
        public static readonly Color Gold = Color.FromArgb(246, 190, 70);
        public static readonly Color Warn = Color.FromArgb(240, 184, 72);
        public static readonly Color Bad = Color.FromArgb(236, 98, 92);
        public static readonly Color Blue = Color.FromArgb(120, 184, 255);
        public static readonly Color OnAccent = Color.FromArgb(255, 255, 255);

        public static bool Square = true; // blocky Minecraft look: no rounded corners

        public static Font F(float size, FontStyle st = FontStyle.Regular)
        {
            return new Font("Segoe UI", size, st);
        }

        public static Font Semi(float size)
        {
            return new Font("Segoe UI Semibold", size);
        }

        // blocky display font (Edit Undo BRK), embedded in the exe; falls back to Segoe UI
        static PrivateFontCollection pfc;
        static FontFamily pixel;
        static bool pixelTried;

        public static Font P(float size)
        {
            if (!pixelTried)
            {
                pixelTried = true;
                pixel = LoadPixelFont();
            }
            return pixel != null ? new Font(pixel, size, FontStyle.Regular) : Semi(size);
        }

        static FontFamily LoadPixelFont()
        {
            try
            {
                using (var s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("font.ttf"))
                {
                    var data = new byte[s.Length];
                    s.Read(data, 0, data.Length);

                    // kept for the app's lifetime; PrivateFontCollection needs the memory to stay pinned/allocated
                    var mem = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(data.Length);
                    System.Runtime.InteropServices.Marshal.Copy(data, 0, mem, data.Length);

                    pfc = new PrivateFontCollection();
                    pfc.AddMemoryFont(mem, data.Length);
                    return pfc.Families[0];
                }
            }
            catch { return null; }
        }

        public static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        public static Color Shade(Color c, float k)
        {
            return k < 0 ? Lerp(c, Color.FromArgb(c.A, 0, 0, 0), -k) : Lerp(c, Color.FromArgb(c.A, 255, 255, 255), k);
        }

        public static GraphicsPath Round(RectangleF r, float rad)
        {
            var p = new GraphicsPath();
            float d = rad * 2;
            if (Square || d <= 0)
            {
                p.AddRectangle(new RectangleF((float)Math.Round(r.X), (float)Math.Round(r.Y), (float)Math.Round(r.Width), (float)Math.Round(r.Height)));
                return p;
            }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void Hq(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
        }

        /// <summary>Minecraft-style block: 2px black outline, light top/left and dark bottom/right bevel
        /// (flipped when pressed so it looks pushed in).</summary>
        public static void Bevel(Graphics g, RectangleF r, Color face, bool pressed)
        {
            var rr = Rectangle.Round(r);
            var hi = Shade(face, 0.28f);
            var lo = Shade(face, -0.35f);
            if (pressed) { var tmp = hi; hi = lo; lo = tmp; }

            using (var b = new SolidBrush(hi))
            {
                g.FillRectangle(b, rr.X + 2, rr.Y + 2, rr.Width - 4, 2);
                g.FillRectangle(b, rr.X + 2, rr.Y + 2, 2, rr.Height - 4);
            }
            using (var b = new SolidBrush(lo))
            {
                g.FillRectangle(b, rr.X + 2, rr.Bottom - 4, rr.Width - 4, 2);
                g.FillRectangle(b, rr.Right - 4, rr.Y + 2, 2, rr.Height - 4);
            }
            using (var p = new Pen(Color.Black, 2)) g.DrawRectangle(p, rr.X + 1, rr.Y + 1, rr.Width - 2, rr.Height - 2);
        }
    }
}
