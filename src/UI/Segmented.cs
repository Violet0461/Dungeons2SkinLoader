using System;
using System.Drawing;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Two-option switch with a sliding highlight.</summary>
    public class Segmented : AnimControl
    {
        public string[] Options;
        public event EventHandler Picked;
        public int Index { get { return (int)Math.Round(extraT); } set { extraT = value; Invalidate(); } }

        public Segmented(params string[] o)
        {
            Options = o;
            Height = 34;
            Width = 260;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int i = Math.Min(Options.Length - 1, e.X * Options.Length / Width);
            if (i != Index)
            {
                Index = i;
                if (Picked != null) Picked(this, EventArgs.Empty);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintParentBg(g);
            Theme.Hq(g);

            var r = new RectangleF(1, 1, Width - 3, Height - 3);
            using (var path = Theme.Round(r, 9))
            {
                using (var b = new SolidBrush(Theme.Card)) g.FillPath(b, path);
                using (var pn = new Pen(Theme.Lerp(Theme.Line, Theme.Dim, hover * 0.5f))) g.DrawPath(pn, path);
            }

            float w = r.Width / Options.Length;
            using (var path = Theme.Round(new RectangleF(r.X + 3 + extra * w, r.Y + 3 + press, w - 6, r.Height - 6), 7))
            using (var b = new SolidBrush(Theme.Accent)) g.FillPath(b, path);

            for (int i = 0; i < Options.Length; i++)
            {
                var sz = g.MeasureString(Options[i], Theme.P(12.3f));
                float near = 1 - Math.Min(1, Math.Abs(extra - i));
                using (var b = new SolidBrush(Theme.Lerp(Theme.Text, Theme.OnAccent, near)))
                    g.DrawString(Options[i], Theme.P(12.3f), b, r.X + i * w + (w - sz.Width) / 2, r.Y + (r.Height - sz.Height) / 2);
            }
        }
    }
}
