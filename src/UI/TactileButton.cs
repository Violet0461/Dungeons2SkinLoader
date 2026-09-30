using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    public enum BtnKind { Primary, Secondary }

    /// <summary>Rounded button with hover glow, a press that sinks into its base, and a click ripple.</summary>
    public class TactileButton : AnimControl
    {
        public BtnKind Kind;
        public string Glyph;
        PointF rippleAt;
        float ripple = 1;

        public TactileButton(string text, BtnKind kind = BtnKind.Secondary, string glyph = null)
        {
            Text = text;
            Kind = kind;
            Glyph = glyph;
            Cursor = Cursors.Hand;
            Height = 44;
            Width = 160;
            Font = Theme.P(Kind == BtnKind.Primary ? 14f : 12.5f);
        }

        protected override void Step()
        {
            base.Step();
            if (ripple < 1)
            {
                ripple = System.Math.Min(1, ripple + 0.045f);
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            rippleAt = e.Location;
            ripple = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintParentBg(g);
            Theme.Hq(g);

            Color baseC, hiC, fg, under;
            if (Kind == BtnKind.Primary) { baseC = Theme.Accent; hiC = Theme.AccentHi; fg = Theme.OnAccent; under = Theme.AccentDeep; }
            else { baseC = Theme.Card; hiC = Theme.CardHi; fg = Theme.Text; under = Color.FromArgb(4, 6, 11); }

            float depth = 4f, sink = depth * 0.8f * press;

            // the "base" the button sits on; pressing pushes the face down into it
            using (var path = Theme.Round(new RectangleF(1, 1 + depth, Width - 3, Height - depth - 2), 10))
            using (var b = new SolidBrush(under)) g.FillPath(b, path);

            var body = new RectangleF(1, 1 + sink, Width - 3, Height - depth - 2);
            var c = Theme.Shade(Theme.Lerp(baseC, hiC, hover), -0.12f * press);
            using (var path = Theme.Round(body, 10))
            {
                using (var b = new SolidBrush(c)) g.FillPath(b, path);
                Theme.Bevel(g, body, c, press > 0.5f);

                if (hover > 0.02f && Kind != BtnKind.Primary)
                    using (var p = new Pen(Color.FromArgb((int)(200 * hover), Theme.Accent), 2))
                        g.DrawRectangle(p, body.X + 1, body.Y + 1, body.Width - 2, body.Height - 2);

                if (ripple < 1)
                {
                    g.SetClip(path);
                    float r = 20 + ripple * Width;
                    using (var b = new SolidBrush(Color.FromArgb((int)(80 * (1 - ripple)), 255, 255, 255)))
                        g.FillEllipse(b, rippleAt.X - r, rippleAt.Y - r, 2 * r, 2 * r);
                    g.ResetClip();
                }
            }

            var txt = (Glyph != null ? Glyph + "   " : "") + Text;
            var sz = g.MeasureString(txt, Font);
            using (var b = new SolidBrush(fg)) g.DrawString(txt, Font, b, (Width - sz.Width) / 2, body.Y + (body.Height - sz.Height) / 2);
        }
    }
}
