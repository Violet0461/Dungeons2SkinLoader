using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    // ============================================================= animation
    /// <summary>Base for controls that ease their hover/press state every frame.</summary>
    public class AnimControl : Control
    {
        static readonly Timer clock = new Timer { Interval = 15 };
        static readonly List<AnimControl> live = new List<AnimControl>();

        static AnimControl()
        {
            clock.Tick += (s, e) => { foreach (var c in live.ToArray()) c.Step(); };
            clock.Start();
        }

        protected float hover, press, extra;
        protected float hoverT, pressT, extraT;

        public AnimControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            live.Add(this);
            Disposed += (s, e) => live.Remove(this);
        }

        protected virtual void Step()
        {
            bool changed = false;
            changed |= Ease(ref hover, hoverT, 0.22f);
            changed |= Ease(ref press, pressT, 0.35f);
            changed |= Ease(ref extra, extraT, 0.14f);
            if (changed) Invalidate();
        }

        protected static bool Ease(ref float v, float t, float k)
        {
            if (Math.Abs(v - t) < 0.004f)
            {
                if (v == t) return false;
                v = t;
                return true;
            }
            v += (t - v) * k;
            return true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled) hoverT = 1;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoverT = 0;
            pressT = 0;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Enabled && e.Button == MouseButtons.Left) pressT = 1;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            pressT = 0;
        }

        protected void PaintParentBg(System.Drawing.Graphics g)
        {
            g.Clear(Parent != null ? Parent.BackColor : Theme.Panel);
        }
    }
}
