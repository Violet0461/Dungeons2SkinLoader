using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>8x8 face grid (face with the outer layer drawn over it, as in the game). Click pixels to
    /// mark them as blinking eyes: left half = eye 1, right half = eye 2, each on its own layer.</summary>
    public class FacePicker : AnimControl
    {
        public Img Face, Hat;
        public int[] EyeLayer = { 0, 0 };
        public HashSet<int> Eyes = new HashSet<int>();
        public event System.EventHandler Changed;
        public bool Numbered = true;

        const int Cell = 30;
        int hot = -1;

        public FacePicker()
        {
            Width = Height = Cell * 8 + 2;
            Cursor = Cursors.Hand;
        }

        public static int Side(int code)
        {
            return (code % Converter.Outer) / 8 < 4 ? 0 : 1;
        }

        int Code(MouseEventArgs e)
        {
            int x = e.X / Cell, y = e.Y / Cell;
            return x < 0 || x > 7 || y < 1 || y > 6 ? -1 : x * 8 + y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int k = Code(e);
            if (k != hot) { hot = k; Invalidate(); }
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            hot = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int k = Code(e);
            if (k < 0) return;

            // toggle: remove the pixel on either layer, or add it on its eye's layer
            if (!Eyes.Remove(k) & !Eyes.Remove(k + Converter.Outer)) Eyes.Add(k + EyeLayer[Side(k)] * Converter.Outer);

            Invalidate();
            if (Changed != null) Changed(this, System.EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintParentBg(g);
            Theme.Hq(g);
            if (Face == null) return;

            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    PaintCell(g, x, y);
        }

        void PaintCell(Graphics g, int x, int y)
        {
            var r = new Rectangle(x * Cell + 1, y * Cell + 1, Cell - 1, Cell - 1);
            var fc = Face.Get(x, y);
            var hc = Hat != null ? Hat.Get(x, y) : new byte[4];
            var col = hc[3] > 127 ? Color.FromArgb(hc[0], hc[1], hc[2]) : fc[3] == 0 ? Theme.Card : Color.FromArgb(fc[0], fc[1], fc[2]);
            if (y < 1 || y > 6) col = Theme.Lerp(col, Theme.Bg, 0.65f);

            g.SmoothingMode = SmoothingMode.None;
            using (var b = new SolidBrush(col)) g.FillRectangle(b, r);

            // outer-layer pixel
            if (hc[3] > 127) using (var pn = new Pen(Color.FromArgb(70, 255, 255, 255))) g.DrawRectangle(pn, r.X, r.Y, r.Width - 1, r.Height - 1);

            int k = x * 8 + y;
            if (k == hot) using (var b = new SolidBrush(Color.FromArgb(70, 255, 255, 255))) g.FillRectangle(b, r);

            bool onFace = Eyes.Contains(k), onOuter = Eyes.Contains(k + Converter.Outer);
            if (!onFace && !onOuter) return;

            using (var pn = new Pen(onOuter ? Theme.Gold : Theme.Accent, 3)) g.DrawRectangle(pn, r.X + 2, r.Y + 2, r.Width - 5, r.Height - 5);
            if (!Numbered) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            var badge = new RectangleF(r.Right - 15, r.Y - 3, 16, 16);
            using (var b = new SolidBrush(onOuter ? Theme.Gold : Theme.Accent)) g.FillRectangle(b, badge);
            string n = (Side(k) + 1).ToString();
            using (var f = Theme.F(7.5f, FontStyle.Bold))
            {
                var sz = g.MeasureString(n, f);
                using (var b = new SolidBrush(Theme.OnAccent)) g.DrawString(n, f, b, badge.X + (16 - sz.Width) / 2, badge.Y + (16 - sz.Height) / 2);
            }
        }
    }
}
