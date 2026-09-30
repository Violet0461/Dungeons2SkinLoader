using System.Drawing;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Colour square that opens a colour picker.</summary>
    public class Swatch : AnimControl
    {
        public int Rgb;
        public event System.EventHandler ColorChanged;
        public System.Func<Img> SkinSource;
        public string Title = "Choose a colour";

        public Swatch()
        {
            Width = 44;
            Height = 30;
            Cursor = Cursors.Hand;
        }

        protected override void OnClick(System.EventArgs e)
        {
            base.OnClick(e);
            if (!Enabled) return;

            Img skinImg = null;
            try { if (SkinSource != null) skinImg = SkinSource(); } catch { }

            using (var d = new ColorPickerDialog(Rgb, skinImg, Title))
                if (d.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    Rgb = d.Rgb;
                    Invalidate();
                    if (ColorChanged != null) ColorChanged(this, System.EventArgs.Empty);
                }
        }

        protected override void OnEnabledChanged(System.EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintParentBg(g);
            Theme.Hq(g);

            var r = new RectangleF(1, 1 + 1.5f * press, Width - 3, Height - 4);
            var col = Color.FromArgb(Rgb >> 16 & 255, Rgb >> 8 & 255, Rgb & 255);
            if (!Enabled) col = Theme.Lerp(col, Theme.Bg, 0.75f);

            using (var path = Theme.Round(r, 7))
            {
                using (var b = new SolidBrush(col)) g.FillPath(b, path);
                using (var pen = new Pen(Theme.Lerp(Theme.Line, Theme.Text, hover), 1.5f)) g.DrawPath(pen, path);
            }
        }
    }
}
