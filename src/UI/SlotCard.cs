using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>One entry of "Your skins": the game's hero, an arrow, then your skin.</summary>
    public class SlotCard : AnimControl
    {
        public SkinSlot Slot;
        public Hero Hero;
        public Bitmap Custom;
        public string ModeText, FileName;
        public bool Selected { get { return extraT > 0.5f; } set { extraT = value ? 1 : 0; Invalidate(); } }

        public SlotCard()
        {
            Height = 132;
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintParentBg(g);
            Theme.Hq(g);

            var r = new RectangleF(2, 3 + 1.5f * press, Width - 5, Height - 9);
            using (var path = Theme.Round(r, 12))
            {
                var fill = Theme.Lerp(Theme.Lerp(Theme.Card, Theme.CardHi, hover * 0.6f), Color.FromArgb(22, 38, 72), extra);
                using (var b = new SolidBrush(fill)) g.FillPath(b, path);
                var border = Theme.Lerp(Theme.Lerp(Theme.Line, Theme.Dim, hover * 0.4f), Theme.Accent, extra);
                using (var p = new Pen(border, 1 + 1.2f * extra)) g.DrawPath(p, path);
            }

            float s = 78, x0 = r.X + 12, y0 = r.Y + 34;

            // hero name above its original look, arrow, then the custom skin
            using (var b = new SolidBrush(Theme.Text)) g.DrawString(Hero != null ? Hero.Display : "?", Theme.P(14.3f), b, x0, r.Y + 8);

            var modeColor = Theme.Lerp(Theme.Dim, Theme.Accent, extra);
            using (var b = new SolidBrush(modeColor))
                g.DrawString(ModeText, Theme.F(8.5f), b, r.Right - 12 - g.MeasureString(ModeText, Theme.F(8.5f)).Width, r.Y + 11);

            if (Hero != null && Hero.Original != null) g.DrawImage(Hero.Original, x0, y0, s, s);

            var arrowColor = Theme.Lerp(Theme.Dim, Theme.Accent, 0.35f + 0.65f * System.Math.Max(extra, hover));
            DrawArrow(g, x0 + s + 8, y0 + s / 2, 34, arrowColor);

            if (Custom != null) g.DrawImage(Custom, x0 + s + 50, y0, s, s);

            using (var b = new SolidBrush(Theme.Dim))
                g.DrawString(FileName, Theme.F(8f), b, new RectangleF(x0 + 2 * s + 58, y0 + 24, r.Right - (x0 + 2 * s + 64), 40));
        }

        public static void DrawArrow(Graphics g, float x, float cy, float w, Color c)
        {
            using (var p = new Pen(c, 4f) { StartCap = LineCap.Square, EndCap = LineCap.Square, LineJoin = LineJoin.Miter })
            {
                g.DrawLine(p, x, cy, x + w, cy);
                g.DrawLines(p, new[] { new PointF(x + w - 9, cy - 8), new PointF(x + w, cy), new PointF(x + w - 9, cy + 8) });
            }
        }
    }
}
