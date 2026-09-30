using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Big before/after banner: the game's hero -> your skin (drag to turn).</summary>
    public class SwapBanner : AnimControl
    {
        public Hero Hero; public Func<int, double, Bitmap> RenderCustom; public double Yaw = -22;
        int dragX; bool dragging; Bitmap cache; double cacheYaw = double.NaN; int cacheSize; float nudge;
        public void Refresh3D() { cache = null; extraT = 1; extra = 0; Invalidate(); }
        public SwapBanner() { Height = 340; Cursor = Cursors.SizeWE; }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); dragging = true; dragX = e.X; }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); dragging = false; }
        protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); if (!dragging) return; Yaw -= (e.X - dragX) * 0.9; dragX = e.X; Invalidate(); }
        protected override void Step() { base.Step(); nudge += 0.06f; if (hover > 0.01f || extra < 0.99f) Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; PaintParentBg(g); Theme.Hq(g);
            var r = new RectangleF(0, 0, Width - 1, Height - 1);
            using (var path = Theme.Round(r, 16))
            {
                using (var b = new LinearGradientBrush(r, Color.FromArgb(22, 32, 56), Color.FromArgb(10, 13, 22), 90f)) g.FillPath(b, path);
                using (var p = new Pen(Theme.Line)) g.DrawPath(p, path);
            }
            int size = (int)Math.Min(Height - 76, (Width - 130) / 2);
            float half = Width / 2f, lx = (half - 45 - size) / 2 + 12, rx = half + 45 + (half - 45 - size) / 2 - 12, iy = 62;
            using (var b = new SolidBrush(Theme.Dim)) g.DrawString("GAME'S HERO", Theme.P(11.7f), b, lx, 12);
            using (var b = new SolidBrush(Theme.Text)) g.DrawString(Hero != null ? Hero.Display : "", Theme.P(19.5f), b, lx, 28);
            using (var b = new SolidBrush(Theme.Accent)) g.DrawString("YOUR SKIN", Theme.P(11.7f), b, rx, 12);
            using (var b = new SolidBrush(Theme.Dim)) g.DrawString("drag to turn it around", Theme.F(9.5f), b, rx, 32);
            // soft floor shadows
            foreach (var x in new[] { lx, rx })
                using (var b = new SolidBrush(Color.FromArgb(70, 0, 0, 0))) g.FillEllipse(b, x + size * 0.22f, iy + size * 0.93f, size * 0.56f, size * 0.07f);
            if (Hero != null && Hero.Original != null) g.DrawImage(Hero.Original, lx, iy, size, size);
            float bob = (float)Math.Sin(nudge) * 3 * hover;
            SlotCard.DrawArrow(g, half - 30 + bob, iy + size / 2f, 60, Theme.Accent);
            if (RenderCustom != null)
            {
                if (cache == null || cacheYaw != Yaw || cacheSize != size) { cache = RenderCustom(size, Yaw); cacheYaw = Yaw; cacheSize = size; }
                if (cache != null)
                {
                    // new skins pop in with a quick scale-up
                    float k = 0.9f + 0.1f * extra, w = size * k;
                    g.DrawImage(cache, rx + (size - w) / 2, iy + (size - w), w, w);
                }
            }
        }
    }
}
