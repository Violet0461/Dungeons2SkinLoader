using System.Drawing;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }

    // Hosts a scrolling panel but clips off its bright native scrollbars; the wheel still scrolls.
    class ClipHost : DoubleBufferedPanel
    {
        public readonly Panel Inner;

        public ClipHost(Panel inner, Color bg)
        {
            Inner = inner;
            BackColor = bg;
            inner.AutoScroll = true;
            Controls.Add(inner);
            Resize += (s, e) => inner.Bounds = new Rectangle(0, 0, Width + SystemInformation.VerticalScrollBarWidth, Height + SystemInformation.HorizontalScrollBarHeight);
        }
    }
}
