using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    public class Section : Label
    {
        public Section(string t) { Text = t.ToUpperInvariant(); Font = Theme.P(13f); ForeColor = Theme.Dim; AutoSize = true; UseCompatibleTextRendering = true; }
    }
}
