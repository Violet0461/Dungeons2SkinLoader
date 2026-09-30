using System;
using System.Drawing;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Selectable card with a radio dot that pops in when chosen.</summary>
    public class OptionCard : AnimControl
    {
        public string Title, Desc; public event EventHandler Picked;
        public bool Checked { get { return extraT > 0.5f; } set { extraT = value ? 1 : 0; Invalidate(); } }
        public OptionCard(string t, string d) { Title = t; Desc = d; Height = 74; Cursor = Cursors.Hand; }
        protected override void OnClick(EventArgs e) { base.OnClick(e); if (Picked != null) Picked(this, e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; PaintParentBg(g); Theme.Hq(g);
            var r = new RectangleF(1, 1 + 1.5f * press, Width - 3, Height - 4);
            using (var path = Theme.Round(r, 11))
            {
                using (var b = new SolidBrush(Theme.Lerp(Theme.Lerp(Theme.Card, Theme.CardHi, hover * 0.7f), Color.FromArgb(22, 38, 72), extra))) g.FillPath(b, path);
                using (var p = new Pen(Theme.Lerp(Theme.Lerp(Theme.Line, Theme.Dim, hover * 0.5f), Theme.Accent, extra), 1 + extra)) g.DrawPath(p, path);
            }
            float cy = r.Y + r.Height / 2;
            using (var b = new SolidBrush(Theme.Bg)) g.FillRectangle(b, 17, cy - 10, 20, 20);
            using (var p = new Pen(Theme.Lerp(Theme.Dim, Theme.Accent, extra), 2)) g.DrawRectangle(p, 17, cy - 10, 20, 20);
            float d = (float)Math.Round(10 * extra * (1 + 0.25f * (float)Math.Sin(Math.PI * extra)));
            if (d > 0.5f) using (var b = new SolidBrush(Theme.Accent)) g.FillRectangle(b, 27 - d / 2, cy - d / 2, d, d);
            using (var b = new SolidBrush(Theme.Text)) g.DrawString(Title, Theme.P(14.3f), b, 52, r.Y + 10);
            using (var b = new SolidBrush(Theme.Dim)) g.DrawString(Desc, Theme.F(9f), b, new RectangleF(52, r.Y + 33, Width - 62, 36));
        }
    }
}
