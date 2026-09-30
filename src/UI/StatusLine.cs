using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Right-aligned status line: coloured dots between the parts, text in blue.</summary>
    public class StatusLine : Control
    {
        public string[] Parts = new string[0]; public Color Dot = Color.FromArgb(88, 206, 110), Ink = Theme.Accent;
        public StatusLine() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true); BackColor = Color.Transparent; }
        public void Set(Color dot, params string[] parts) { Dot = dot; Parts = parts; Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            // build: ● part · part   (measured right to left so it hugs the right edge)
            var pieces = new List<Tuple<string, Color>>();
            pieces.Add(Tuple.Create("■", Dot));
            for (int i = 0; i < Parts.Length; i++)
            {
                if (i > 0) pieces.Add(Tuple.Create("■", Dot));
                pieces.Add(Tuple.Create(Parts[i], Ink));
            }
            float x = Width;
            var widths = pieces.Select(p => g.MeasureString(p.Item1, Font).Width + 6).ToList();
            x -= widths.Sum();
            for (int i = 0; i < pieces.Count; i++)
            {
                bool dot = pieces[i].Item1 == "■";
                using (var f = dot ? new Font(Font.FontFamily, Font.Size * 0.6f) : null)
                using (var b = new SolidBrush(pieces[i].Item2))
                {
                    var font = f ?? Font; var sz = g.MeasureString(pieces[i].Item1, font);
                    g.DrawString(pieces[i].Item1, font, b, x + (dot ? (widths[i] - sz.Width) / 2 : 0), (Height - sz.Height) / 2);
                }
                x += widths[i];
            }
        }
    }
}
