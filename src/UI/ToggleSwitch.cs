using System;
using System.Drawing;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Pill switch whose knob slides over.</summary>
    public class ToggleSwitch : AnimControl
    {
        public event EventHandler Toggled;
        public bool On { get { return extraT > 0.5f; } set { extraT = value ? 1 : 0; Invalidate(); } }

        public ToggleSwitch()
        {
            Width = 46;
            Height = 26;
            Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            On = !On;
            if (Toggled != null) Toggled(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintParentBg(g);
            Theme.Hq(g);

            var r = new RectangleF(1, 2, Width - 3, Height - 5);
            using (var path = Theme.Round(r, r.Height / 2))
            {
                using (var b = new SolidBrush(Theme.Lerp(Theme.Lerp(Theme.Card, Theme.CardHi, hover), Theme.Accent, extra))) g.FillPath(b, path);
                using (var pen = new Pen(Theme.Lerp(Theme.Line, Theme.AccentDeep, extra))) g.DrawPath(pen, path);
            }

            float d = r.Height - 6 - 2 * press, x = r.X + 3 + extra * (r.Width - d - 6);
            using (var b = new SolidBrush(Theme.Text)) g.FillRectangle(b, (float)Math.Round(x), r.Y + 3 + press, d, d);
            using (var p = new Pen(Color.Black, 2)) g.DrawRectangle(p, (float)Math.Round(x), r.Y + 3 + press, d, d);
        }
    }
}
