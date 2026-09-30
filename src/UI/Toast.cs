using System.Drawing;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Pill that slides up at the bottom, then fades away.</summary>
    public class Toast : AnimControl
    {
        Timer life = new Timer { Interval = 4200 };
        Color col = Theme.Accent;

        public Toast()
        {
            Visible = false;
            Height = 48;
            life.Tick += (s, e) => { life.Stop(); extraT = 0; };
        }

        public void Show(string text, Color c)
        {
            Text = text;
            col = c;
            Visible = true;
            BringToFront();
            extra = 0;
            extraT = 1;
            life.Stop();
            life.Start();
            Invalidate();
        }

        protected override void Step()
        {
            base.Step();
            if (extraT == 0 && extra < 0.02f && Visible) Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintParentBg(g);
            Theme.Hq(g);

            float a = extra, dy = (1 - a) * 18;
            var r = new RectangleF(1, 1 + dy, Width - 3, Height - 3);
            using (var path = Theme.Round(r, 22))
            {
                using (var b = new SolidBrush(Color.FromArgb((int)(250 * a), Theme.Lerp(col, Color.Black, 0.6f)))) g.FillPath(b, path);
                using (var p = new Pen(Color.FromArgb((int)(255 * a), col), 1.6f)) g.DrawPath(p, path);
            }

            using (var b = new SolidBrush(Color.FromArgb((int)(255 * a), Theme.Text)))
            {
                var sz = g.MeasureString(Text, Theme.Semi(10.5f));
                g.DrawString(Text, Theme.Semi(10.5f), b, (Width - sz.Width) / 2, r.Y + (r.Height - sz.Height) / 2);
            }
        }
    }
}
