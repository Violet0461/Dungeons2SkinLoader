using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Dungeons2SkinLoader
{
    /// <summary>The skin list on the left and the editor's live selection state.</summary>
    public partial class MainForm
    {
        int lastLayerSlot = -2;

        // -------------------------------------------------------------- list
        void RebuildCards()
        {
            listPanel.SuspendLayout();
            foreach (var c in cards) c.Dispose();
            cards.Clear();
            listPanel.Controls.Clear();

            for (int i = 0; i < slots.Count; i++)
            {
                int idx = i;
                var c = new SlotCard { Slot = slots[i], Width = listPanel.Parent.Width - 4, Location = new Point(0, i * 138), BackColor = Theme.Panel };
                c.Click += (s, e) => Select(idx);
                cards.Add(c);
                listPanel.Controls.Add(c);
            }

            listPanel.ResumeLayout();
            RefreshCards();
            emptyHint.Visible = slots.Count == 0;
        }

        void RefreshCards()
        {
            foreach (var c in cards)
            {
                c.Hero = gd.FindHero(c.Slot.HeroKey);
                c.Custom = Thumb(c.Slot);
                c.ModeText = ModeText(c.Slot.Mode);
                c.FileName = SkinName(c.Slot);
                c.Selected = c.Slot == Current;
                c.Invalidate();
            }
        }

        // ---------------------------------------------------------- selection
        void Select(int i)
        {
            loading = true;
            current = i;
            var s = Current;
            editor.Parent.Visible = s != null;
            if (s != null)
            {
                UpdateImagePreview(s);
                UpdateHeroPicker(s);
                UpdateFaceModeUi(s);
                UpdateGameFacePanel(s);
                UpdateEyePicker(s);
                UpdateLidControls(s);
                banner.Hero = gd.FindHero(s.HeroKey);
                banner.Refresh3D();
            }
            loading = false;
            RefreshCards();
        }

        void UpdateImagePreview(SkinSlot s)
        {
            try { flatTex.Image = Img.FromFile(s.ImagePath).ToBitmap(); }
            catch { flatTex.Image = null; }
            fileLabel.Text = SkinName(s) + "\r\nAny Minecraft skin works: 64x64 or old 64x32, classic or slim arms.";
        }

        void UpdateHeroPicker(SkinSlot s)
        {
            heroPicker.Items.Clear();
            heroPicker.Selected = null;
            foreach (var h in gd.Heroes.OrderBy(h => h.Name.StartsWith("_")).ThenBy(h => h.Name).ThenBy(h => h.Deluxe))
            {
                var it = new HeroPicker.Item { H = h, Used = slots.Any(o => o != s && o.HeroKey == h.Key) };
                heroPicker.Items.Add(it);
                if (h.Key == s.HeroKey) heroPicker.Selected = it;
            }
            heroPicker.Invalidate();
        }

        void UpdateFaceModeUi(SkinSlot s)
        {
            for (int k = 0; k < 3; k++) modeCards[k].Checked = (int)s.Mode == k;

            gamePanel.Visible = s.Mode == FaceMode.Game;
            eyePanel.Visible = s.Mode == FaceMode.Blink || s.Mode == FaceMode.Game;
            eyePanel.Top = partsTop + (s.Mode == FaceMode.Game ? gamePanel.Height + 8 : 0) + editor.AutoScrollPosition.Y;
            gamePanel.Top = partsTop + editor.AutoScrollPosition.Y;
            eyeInfo.Text = s.Mode == FaceMode.Game
                ? "Your own drawn eyes (blue frames) are hidden, so only the game's eyes show. Click pixels to add or remove them."
                : "Click the eye pixels: left half = eye 1, right half = eye 2. Pick each eye's layer: face (blue) or the outer hat layer (gold).";
        }

        void UpdateGameFacePanel(SkinSlot s)
        {
            if (s.Mode != FaceMode.Game) return;

            var faceCrop = Converter.To64(Img.FromFile(s.ImagePath)).Crop(8, 8, 8, 8);
            var gf = s.Face ?? GameFace.Auto(faceCrop, s.Eyes);
            var vals = new[] { gf.Iris, gf.White, gf.Brow };
            var auto = GameFace.Auto(faceCrop, s.Eyes);
            var fallback = new[] { auto.Iris ?? 0x1E1E1E, auto.White ?? 0xF4F4F4, auto.Brow ?? 0x3A2A20 };
            for (int k = 0; k < 3; k++)
            {
                gfOn[k].On = vals[k].HasValue;
                gfCol[k].Rgb = vals[k] ?? fallback[k];
                gfCol[k].Enabled = vals[k].HasValue;
                gfCol[k].Invalidate();
            }
        }

        void UpdateEyePicker(SkinSlot s)
        {
            try
            {
                var im64 = Converter.To64(Img.FromFile(s.ImagePath));
                picker.Face = im64.Crop(8, 8, 8, 8);
                picker.Hat = im64.Crop(40, 8, 8, 8);
            }
            catch { picker.Face = null; picker.Hat = null; }

            bool blinkUi = s.Mode == FaceMode.Blink;
            lidRow.Visible = eyeSeg[0].Visible = eyeSeg[1].Visible = eyeLbl[0].Visible = eyeLbl[1].Visible = blinkUi;
            picker.Numbered = blinkUi;

            for (int side = 0; side < 2; side++)
            {
                int sd = side;
                var mine = s.Eyes.Where(v => FacePicker.Side(v) == sd).ToList();
                if (mine.Count > 0) picker.EyeLayer[sd] = mine[0] >= Converter.Outer ? 1 : 0;
                else if (current != lastLayerSlot) picker.EyeLayer[sd] = 0;
                eyeSeg[sd].Index = picker.EyeLayer[sd];
            }
            lastLayerSlot = current;

            picker.Eyes = new HashSet<int>(s.Eyes);
            picker.Invalidate();
        }

        void UpdateLidControls(SkinSlot s)
        {
            lidOn.On = s.LidColor.HasValue;
            lidSplit.On = s.LidColor2.HasValue;

            lidCol.Rgb = s.LidColor ?? AutoLid(s, 0);
            lidCol.Invalidate();
            lidCol2.Rgb = s.LidColor2 ?? s.LidColor ?? AutoLid(s, 1);
            lidCol2.Invalidate();

            lidLbl.Text = lidSplit.On ? "Eyelid colour, eye 1 (left)" : "Eyelid colour (what the eye closes to)";
            lidLbl2.Text = lidSplit.On ? "Eyelid colour, eye 2 (right)" : "Different colour for each eye";
            lidCol2.Visible = lidSplit.On;
            lidLbl2.Left = lidSplit.On ? 108 : 56;
        }

        /// <summary>The selected skin as a 64x64 Minecraft skin (for the colour picker's eyedropper).</summary>
        Img CurrentSkinImage()
        {
            var s = Current;
            if (s == null) return null;
            return Converter.To64(Img.FromFile(s.ImagePath));
        }

        int AutoLid(SkinSlot s, int side = -1)
        {
            // the colour next to the first eye on its layer: what the eye closes to by default
            try
            {
                var im = Converter.To64(Img.FromFile(s.ImagePath));
                foreach (var e in s.Eyes.Where(v => side < 0 || FacePicker.Side(v) == side))
                {
                    int bx = e >= Converter.Outer ? 40 : 8, k = e % Converter.Outer, col = k / 8, row = k % 8;
                    foreach (var dx in new[] { 1, -1, 2, -2 })
                    {
                        int x = col + dx;
                        if (x < 0 || x > 7 || s.Eyes.Contains(e - k + x * 8 + row)) continue;
                        var c = im.Get(bx + x, 8 + row);
                        if (c[3] != 0) return (c[0] << 16) | (c[1] << 8) | c[2];
                    }
                }
                var f = im.Get(8 + 3, 8 + 4);
                return (f[0] << 16) | (f[1] << 8) | f[2];
            }
            catch { return 0xC89070; }
        }

        void GameFaceEdited(int k)
        {
            var s = Current;
            if (s == null || loading) return;

            Func<int, int?> v = i => gfOn[i].On ? (int?)gfCol[i].Rgb : null;
            var gf = new GameFace { Iris = v(0), White = v(1), Brow = v(2), Mouth = null };
            s.Face = gf;
            Changed();
        }

        void Changed()
        {
            if (Current == null) return;
            texCache.Clear();
            thumbCache.Clear();
            SaveConfig();
            Select(current);
        }
    }
}
