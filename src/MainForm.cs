using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    // ================================================================ window
    /// <summary>Main window: fields, wiring, and small shared helpers. The rest of the
    /// behaviour lives in the other MainForm.*.cs partial files, grouped by concern.</summary>
    public partial class MainForm : Form
    {
        GameData gd; string gameDir;
        List<SkinSlot> slots = new List<SkinSlot>(); bool layers = true; int current = -1;
        Dictionary<SkinSlot, Img> texCache = new Dictionary<SkinSlot, Img>();
        Dictionary<SkinSlot, Bitmap> thumbCache = new Dictionary<SkinSlot, Bitmap>();

        Panel listPanel, editor, header; List<SlotCard> cards = new List<SlotCard>();
        SwapBanner banner; HeroPicker heroPicker; OptionCard[] modeCards; FacePicker picker; Panel eyePanel, gamePanel; Label eyeInfo;
        ToggleSwitch[] gfOn; Swatch[] gfCol; int partsTop; Segmented[] eyeSeg; Label[] eyeLbl; ToggleSwitch lidOn, lidSplit; Swatch lidCol, lidCol2; Panel lidRow; Label lidLbl, lidLbl2;
        Label fileLabel, emptyHint; StatusLine statusPill; PictureBox flatTex; CheckBox layersBox; Toast toast; bool loading;

        public MainForm()
        {
            Text = App.Name + "  -  by " + App.Author; BackColor = Theme.Bg; ForeColor = Theme.Text; Font = Theme.F(9.5f);
            ClientSize = new Size(1300, 840); MinimumSize = new Size(1140, 780); StartPosition = FormStartPosition.CenterScreen;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            DoubleBuffered = true; AllowDrop = true;
            DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
            DragDrop += (s, e) => { foreach (var f in (string[])e.Data.GetData(DataFormats.FileDrop)) AddSkin(f); };
            gd = App.LoadData();
            BuildUi();
            LoadConfig();
            DetectGame(Game.Find());
            RebuildCards(); Select(slots.Count > 0 ? 0 : -1);
        }

        SkinSlot Current { get { return current >= 0 && current < slots.Count ? slots[current] : null; } }

        // ------------------------------------------------------------ rendering
        string SkinName(SkinSlot s) { var m = s.ImagePath + ".name"; return File.Exists(m) ? File.ReadAllText(m) : Path.GetFileName(s.ImagePath); }
        static string ModeText(FaceMode m) { return m == FaceMode.Blink ? "blinking eyes" : m == FaceMode.Game ? "game face" : "face as drawn"; }

        Img Tex(SkinSlot s)
        {
            Img t;
            if (!texCache.TryGetValue(s, out t))
            {
                try { t = Converter.Convert(gd, Img.FromFile(s.ImagePath), s.Mode, s.Eyes, s.Face, s.LidColor, s.LidColor2); } catch { t = null; }
                texCache[s] = t;
            }
            return t;
        }
        Bitmap Thumb(SkinSlot s)
        {
            Bitmap b;
            if (!thumbCache.TryGetValue(s, out b))
            {
                var t = Tex(s);
                b = t == null ? null : Renderer.RenderIcon(gd.IconGeo[layers ? 0 : 1], t, 160, gd.IconCam, 0, 2).ToBitmap();
                thumbCache[s] = b;
            }
            return b;
        }

        void Msg(string t, bool warn) { MessageBox.Show(this, t, App.Name, MessageBoxButtons.OK, warn ? MessageBoxIcon.Warning : MessageBoxIcon.Information); }
    }
}

