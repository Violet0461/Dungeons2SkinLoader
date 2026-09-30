using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Hero chooser showing each hero's original look.</summary>
    public class HeroPicker : AnimControl
    {
        public class Item { public Hero H; public bool Used; }

        public List<Item> Items = new List<Item>();
        public Item Selected;
        public event System.EventHandler Picked;

        public HeroPicker()
        {
            Height = 56;
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            PaintParentBg(g);
            Theme.Hq(g);

            var r = new RectangleF(1, 1 + 1.5f * press, Width - 3, Height - 4);
            using (var path = Theme.Round(r, 11))
            {
                using (var b = new SolidBrush(Theme.Lerp(Theme.Card, Theme.CardHi, hover))) g.FillPath(b, path);
                using (var p = new Pen(Theme.Lerp(Theme.Line, Theme.Accent, hover * 0.7f))) g.DrawPath(p, path);
            }

            if (Selected != null && Selected.H.Original != null) g.DrawImage(Selected.H.Original, r.X + 6, r.Y + 3, 46, 46);
            using (var b = new SolidBrush(Theme.Text))
                g.DrawString(Selected != null ? Selected.H.Display : "Choose a hero", Theme.P(15.6f), b, r.X + 58, r.Y + 14);
            using (var b = new SolidBrush(Theme.Lerp(Theme.Dim, Theme.Text, hover)))
                g.FillPolygon(b, new[] { new PointF(Width - 30, r.Y + 22), new PointF(Width - 16, r.Y + 22), new PointF(Width - 23, r.Y + 30) });
        }

        protected override void OnClick(System.EventArgs e)
        {
            base.OnClick(e);
            var menu = new ContextMenuStrip
            {
                Renderer = new DarkRenderer(),
                BackColor = Theme.Card,
                ForeColor = Theme.Text,
                Font = Theme.F(10.5f),
                ImageScalingSize = new Size(44, 44),
                MaximumSize = new Size(Width + 60, 600)
            };
            foreach (var it in Items)
            {
                var item = it;
                var mi = new ToolStripMenuItem(it.H.Display + (it.Used ? "     · has a custom skin" : ""))
                {
                    ForeColor = it.Used ? Theme.Dim : Theme.Text,
                    Image = it.H.Original,
                    ImageScaling = ToolStripItemImageScaling.SizeToFit,
                    Font = it == Selected ? Theme.F(10.5f, System.Drawing.FontStyle.Bold) : Theme.F(10.5f)
                };
                mi.Click += (s, a) => { Selected = item; Invalidate(); if (Picked != null) Picked(this, System.EventArgs.Empty); };
                menu.Items.Add(mi);
            }
            menu.Show(this, new Point(0, Height));
        }
    }

    class DarkRenderer : ToolStripProfessionalRenderer
    {
        public DarkRenderer() : base(new DarkColors())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.ForeColor;
            base.OnRenderItemText(e);
        }
    }
    class DarkColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected { get { return Theme.CardHi; } }
        public override Color MenuItemBorder { get { return Theme.Accent; } }
        public override Color ToolStripDropDownBackground { get { return Theme.Card; } }
        public override Color MenuBorder { get { return Theme.Line; } }
        public override Color ImageMarginGradientBegin { get { return Theme.Panel; } }
        public override Color ImageMarginGradientMiddle { get { return Theme.Panel; } }
        public override Color ImageMarginGradientEnd { get { return Theme.Panel; } }
    }
}
