using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Builds every control in the window and wires up their events.</summary>
    public partial class MainForm
    {
        // ------------------------------------------------------------------ UI
        void BuildUi()
        {
            header = new DoubleBufferedPanel { Dock = DockStyle.Top, Height = 92, BackColor = Theme.Panel };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics; Theme.Hq(g);
                using (var b = new LinearGradientBrush(header.ClientRectangle, Color.FromArgb(16, 40, 86), Theme.Panel, 0f)) g.FillRectangle(b, header.ClientRectangle);
                if (Icon != null) using (var ic = new Icon(Icon, 64, 64)) g.DrawIcon(ic, new Rectangle(22, 18, 56, 56));
                using (var b = new SolidBrush(Theme.Text)) g.DrawString(App.Name, Theme.P(27.3f), b, 88, 8);
                // author credit: gold, with a dark drop shadow so it stands out
                var author = "by " + App.Author;
                using (var b = new SolidBrush(Color.Black)) g.DrawString(author, Theme.P(19f), b, 93, 49);
                using (var b = new SolidBrush(Theme.Gold)) g.DrawString(author, Theme.P(19f), b, 91, 47);
                var sz = g.MeasureString(author, Theme.P(19f));
                using (var b = new SolidBrush(Theme.Dim)) g.DrawString("·   your own Minecraft skins on the heroes   ·   cosmetic only", Theme.F(9.5f), b, 93 + sz.Width + 4, 55);
                using (var p = new Pen(Theme.Line)) g.DrawLine(p, 0, header.Height - 1, header.Width, header.Height - 1);
            };
            statusPill = new StatusLine { Height = 30, Width = 420, Font = Theme.P(13.7f), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            var help = MakeLink("How it works", ShowHelp); var change = MakeLink("Change game folder", PickGameFolder);
            header.Controls.AddRange(new Control[] { statusPill, help, change });
            header.Resize += (s, e) =>
            {
                statusPill.Location = new Point(header.Width - statusPill.Width - 24, 16);
                change.Location = new Point(header.Width - change.Width - 24, 56); help.Location = new Point(change.Left - help.Width - 18, 56);
            };

            // bottom bar
            var bottom = new DoubleBufferedPanel { Dock = DockStyle.Bottom, Height = 86, BackColor = Theme.Panel };
            bottom.Paint += (s, e) => { using (var p = new Pen(Theme.Line)) e.Graphics.DrawLine(p, 0, 0, bottom.Width, 0); };
            layersBox = new CheckBox { Text = "3D outer layers (jacket, sleeves, pants)", AutoSize = true, Checked = true, ForeColor = Theme.Text, Location = new Point(26, 22), Font = Theme.F(10f), Cursor = Cursors.Hand };
            layersBox.CheckedChanged += (s, e) => { if (loading) return; layers = layersBox.Checked; thumbCache.Clear(); SaveConfig(); RefreshCards(); banner.Refresh3D(); };
            var hint = new Label { Text = "Applies to every hero; skins without outer-layer pixels look the same.", AutoSize = true, ForeColor = Theme.Dim, Location = new Point(45, 48), Font = Theme.F(8.5f) };
            var install = new TactileButton("Install to game", BtnKind.Primary, "⬇") { Width = 250, Height = 54, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            var remove = new TactileButton("Uninstall mod", BtnKind.Secondary) { Width = 180, Height = 54, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            var export = new TactileButton("Export for a friend", BtnKind.Secondary, "⇪") { Width = 246, Height = 54, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            foreach (var b in new[] { install, remove, export }) b.BackColor = Theme.Panel;
            install.Click += (s, e) => Install(); remove.Click += (s, e) => RemoveMod(); export.Click += (s, e) => Export();
            bottom.Controls.AddRange(new Control[] { layersBox, hint, install, remove, export });
            bottom.Resize += (s, e) =>
            {
                install.Location = new Point(bottom.Width - 274, 16); remove.Location = new Point(install.Left - 192, 16); export.Location = new Point(remove.Left - 258, 16);
            };

            // left: skin list
            var left = new DoubleBufferedPanel { Dock = DockStyle.Left, Width = 380, BackColor = Theme.Panel };
            left.Paint += (s, e) => { using (var p = new Pen(Theme.Line)) e.Graphics.DrawLine(p, left.Width - 1, 0, left.Width - 1, left.Height); };
            var lt = new Section("Your skins") { Location = new Point(22, 18) };
            var lt2 = new Label { Text = "game's hero  →  your skin", AutoSize = true, ForeColor = Theme.Dim, Location = new Point(160, 20), Font = Theme.F(8.5f) };
            listPanel = new DoubleBufferedPanel { BackColor = Theme.Panel };
            var listHost = new ClipHost(listPanel, Theme.Panel) { Location = new Point(12, 42) };
            var add = new TactileButton("Add skin", BtnKind.Primary, "+") { Height = 52 };
            var del = new TactileButton("Remove", BtnKind.Secondary) { Width = 112, Height = 52 };
            foreach (var b in new[] { add, del }) b.BackColor = Theme.Panel;
            add.Click += (s, e) => BrowseAdd(); del.Click += (s, e) => RemoveSlot();
            emptyHint = new Label { Text = "No skins yet.\r\n\r\nClick \"Add skin\" or drop skin\r\nPNG files anywhere on this window.", ForeColor = Theme.Dim, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Font = Theme.F(10.5f) };
            left.Controls.AddRange(new Control[] { lt, lt2, emptyHint, listHost, add, del });
            left.Resize += (s, e) =>
            {
                listHost.Size = new Size(left.Width - 20, left.Height - 42 - 80);
                add.Location = new Point(18, left.Height - 68); add.Width = left.Width - 36 - 124; del.Location = new Point(left.Width - 18 - 112, left.Height - 68);
                emptyHint.Bounds = new Rectangle(12, 120, left.Width - 24, 140);
                foreach (var c in cards) c.Width = listHost.Width - 4;
            };

            // right: editor
            editor = new DoubleBufferedPanel { BackColor = Theme.Bg };
            var editorHost = new ClipHost(editor, Theme.Bg) { Dock = DockStyle.Fill };
            int y = 22, X = 30;
            banner = new SwapBanner { Location = new Point(X, y), BackColor = Theme.Bg };
            banner.RenderCustom = (size, yaw) =>
            {
                var s = Current; var t = s != null ? Tex(s) : null; if (t == null) return null;
                return Renderer.RenderIcon(gd.IconGeo[layers ? 0 : 1], t, size, gd.IconCam, yaw + 22, 2).ToBitmap();
            };
            editor.Controls.Add(banner); y += banner.Height + 24;

            editor.Controls.Add(new Section("Skin image") { Location = new Point(X, y) }); y += 26;
            flatTex = new PictureBox { Location = new Point(X, y), Size = new Size(112, 112), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Theme.Card };
            var choose = new TactileButton("Choose PNG…", BtnKind.Secondary) { Location = new Point(X + 130, y), Width = 180, BackColor = Theme.Bg };
            choose.Click += (s, e) => ChangeImage();
            fileLabel = new Label { Location = new Point(X + 130, y + 56), AutoSize = false, Width = 380, Height = 56, ForeColor = Theme.Dim };
            editor.Controls.AddRange(new Control[] { flatTex, choose, fileLabel }); y += 134;

            editor.Controls.Add(new Section("Hero to replace") { Location = new Point(X, y) }); y += 26;
            heroPicker = new HeroPicker { Location = new Point(X, y), Width = 380, BackColor = Theme.Bg };
            heroPicker.Picked += (s, e) =>
            {
                var cur = Current; if (loading || cur == null || heroPicker.Selected == null) return;
                var key = heroPicker.Selected.H.Key;
                var other = slots.FirstOrDefault(o => o != cur && o.HeroKey == key);
                if (other != null) other.HeroKey = cur.HeroKey;       // swap the two heroes
                cur.HeroKey = key; Changed();
            };
            var heroHint = new Label { Location = new Point(X + 396, y + 10), AutoSize = false, Width = 260, Height = 40, ForeColor = Theme.Dim, Text = "In the game, pick this hero in the Locker to wear your skin." };
            editor.Controls.AddRange(new Control[] { heroPicker, heroHint }); y += 78;

            editor.Controls.Add(new Section("Face") { Location = new Point(X, y) }); y += 26;
            modeCards = new[] {
                new OptionCard("As drawn", "Your face exactly as in the skin. No animation."),
                new OptionCard("Game face", "The game's own animated eyes and brows (blinking, looking around), coloured from your skin."),
                new OptionCard("Blinking eyes", "Your own eye pixels blink in place, without moving. Choose them below.") };
            for (int i = 0; i < 3; i++)
            {
                var c = modeCards[i]; int mode = i; c.Location = new Point(X, y); c.BackColor = Theme.Bg;
                c.Picked += (s, e) =>
                {
                    var cur = Current; if (cur == null) return;
                    cur.Mode = (FaceMode)mode;
                    if ((cur.Mode == FaceMode.Blink || cur.Mode == FaceMode.Game) && cur.Eyes.Count == 0) AutoEyes(cur.Mode == FaceMode.Blink);
                    Changed();
                };
                editor.Controls.Add(c); y += 82;
            }
            partsTop = y + 6;
            gamePanel = new Panel { Location = new Point(X, partsTop), Size = new Size(600, 176), BackColor = Theme.Bg };
            var gTitle = new Label { Text = "The game draws animated eyes and brows over the face. Both eyes always share one colour. Switch parts off or click a colour to change it.", Location = new Point(0, 0), Size = new Size(600, 38), ForeColor = Theme.Dim };
            gamePanel.Controls.Add(gTitle);
            string[] parts = { "Eyes (irises)", "Eye whites", "Eyebrows" };
            gfOn = new ToggleSwitch[3]; gfCol = new Swatch[3];
            for (int i = 0; i < 3; i++)
            {
                int k = i, ry = 44 + i * 38;
                gfOn[i] = new ToggleSwitch { Location = new Point(0, ry), BackColor = Theme.Bg };
                gfCol[i] = new Swatch { Location = new Point(58, ry - 2), BackColor = Theme.Bg };
                var lbl = new Label { Text = parts[i], Location = new Point(114, ry + 3), AutoSize = true, ForeColor = Theme.Text, Font = Theme.F(10f) };
                gfOn[i].Toggled += (s, e) => GameFaceEdited(k);
                gfCol[i].ColorChanged += (s, e) => { gfOn[k].On = true; GameFaceEdited(k); };
                gamePanel.Controls.AddRange(new Control[] { gfOn[i], gfCol[i], lbl });
            }
            var autoCols = new TactileButton("Auto colours", BtnKind.Secondary) { Location = new Point(300, 44), Width = 150, BackColor = Theme.Bg };
            autoCols.Click += (s, e) => { var cur = Current; if (cur == null) return; cur.Face = null; Changed(); };
            gamePanel.Controls.Add(autoCols);
            for (int i = 0; i < 3; i++) { gfCol[i].SkinSource = CurrentSkinImage; gfCol[i].Title = "Colour: " + parts[i]; }
            editor.Controls.Add(gamePanel);
            eyePanel = new Panel { Location = new Point(X, partsTop), Size = new Size(600, 290), BackColor = Theme.Bg };
            picker = new FacePicker { Location = new Point(0, 0), BackColor = Theme.Bg };
            picker.Changed += (s, e) => { var cur = Current; if (cur == null) return; cur.Eyes = picker.Eyes.OrderBy(v => v).ToList(); Changed(); };
            eyeSeg = new Segmented[2]; eyeLbl = new Label[2];
            for (int i = 0; i < 2; i++)
            {
                int side = i;
                eyeLbl[i] = new Label { Text = i == 0 ? "Eye 1 (left)" : "Eye 2 (right)", Location = new Point(264, 64 + i * 42 + 8), AutoSize = true, ForeColor = Theme.Text, Font = Theme.P(12.3f), UseCompatibleTextRendering = true };
                eyeSeg[i] = new Segmented("Face layer", "Outer layer") { Location = new Point(370, 64 + i * 42), Width = 230, BackColor = Theme.Bg };
                eyeSeg[i].Picked += (s, e) =>
                {
                    var cur = Current; if (cur == null) return;
                    picker.EyeLayer[side] = eyeSeg[side].Index;
                    // move this eye's pixels to the chosen layer
                    cur.Eyes = cur.Eyes.Select(v => FacePicker.Side(v) == side ? v % Converter.Outer + eyeSeg[side].Index * Converter.Outer : v).Distinct().OrderBy(v => v).ToList();
                    Changed();
                };
            }
            lidRow = new Panel { Location = new Point(264, 152), Size = new Size(360, 72), BackColor = Theme.Bg };
            lidOn = new ToggleSwitch { Location = new Point(0, 4), BackColor = Theme.Bg };
            lidCol = new Swatch { Location = new Point(56, 2), BackColor = Theme.Bg };
            lidLbl = new Label { Text = "Eyelid colour (what the eye closes to)", Location = new Point(108, 8), AutoSize = true, ForeColor = Theme.Text };
            lidSplit = new ToggleSwitch { Location = new Point(0, 42), BackColor = Theme.Bg };
            lidCol2 = new Swatch { Location = new Point(56, 40), BackColor = Theme.Bg };
            lidLbl2 = new Label { Text = "Different colour for each eye", Location = new Point(108, 46), AutoSize = true, ForeColor = Theme.Text };
            Action applyLid = () =>
            {
                var cur = Current; if (cur == null || loading) return;
                cur.LidColor = lidOn.On ? (int?)lidCol.Rgb : null;
                cur.LidColor2 = lidOn.On && lidSplit.On ? (int?)lidCol2.Rgb : null;
                Changed();
            };
            lidOn.Toggled += (s, e) => applyLid();
            lidSplit.Toggled += (s, e) => { if (lidSplit.On) lidOn.On = true; applyLid(); };
            lidCol.ColorChanged += (s, e) => { lidOn.On = true; applyLid(); };
            lidCol2.ColorChanged += (s, e) => { lidOn.On = true; lidSplit.On = true; applyLid(); };
            lidRow.Controls.AddRange(new Control[] { lidOn, lidCol, lidLbl, lidSplit, lidCol2, lidLbl2 });
            lidCol.SkinSource = lidCol2.SkinSource = CurrentSkinImage;
            lidCol.Title = "Eyelid colour"; lidCol2.Title = "Eyelid colour, eye 2 (right)";
            eyeInfo = new Label { Location = new Point(264, 0), Width = 320, Height = 60, ForeColor = Theme.Dim,
                Text = "Click the face pixels that are the eyes (they get a green frame). Each chosen pixel squashes shut on the game's blink, then reopens.\r\n\r\nThe dimmed top and bottom rows can't blink." };
            var auto = new TactileButton("Auto-detect", BtnKind.Secondary) { Location = new Point(264, 232), Width = 144, BackColor = Theme.Bg };
            var clear = new TactileButton("Clear", BtnKind.Secondary) { Location = new Point(420, 232), Width = 100, BackColor = Theme.Bg };
            auto.Click += (s, e) => { AutoEyes(); Changed(); };
            clear.Click += (s, e) => { var cur = Current; if (cur == null) return; cur.Eyes.Clear(); Changed(); };
            eyePanel.Controls.AddRange(new Control[] { picker, eyeInfo, eyeSeg[0], eyeSeg[1], eyeLbl[0], eyeLbl[1], lidRow, auto, clear });
            editor.Controls.Add(eyePanel);
            editor.Controls.Add(new Label { Location = new Point(X, y + 520), Height = 16, Width = 4, BackColor = Theme.Bg });   // bottom breathing room
            editorHost.Resize += (s, e) =>
            {
                int w = Math.Max(560, editorHost.Width - 2 * X);
                banner.Width = w; foreach (var mc in modeCards) mc.Width = w; eyePanel.Width = w; eyeInfo.Width = Math.Max(200, w - 272);
                gamePanel.Width = w; gTitle.Width = w;
                heroHint.Width = Math.Max(160, w - 400);
            };

            toast = new Toast { Width = 620, BackColor = Theme.Bg };

            Controls.Add(toast); Controls.Add(editorHost); Controls.Add(left); Controls.Add(bottom); Controls.Add(header);
            EventHandler place = (s, e) => toast.Location = new Point(380 + (ClientSize.Width - 380 - toast.Width) / 2, ClientSize.Height - 86 - toast.Height - 16);
            Resize += place; place(null, null);
        }

        LinkLabel MakeLink(string t, Action a)
        {
            var l = new LinkLabel { Text = t, AutoSize = true, LinkColor = Theme.Blue, ActiveLinkColor = Theme.Text, LinkBehavior = LinkBehavior.HoverUnderline, Font = Theme.F(9.5f), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            l.LinkClicked += (s, e) => a(); return l;
        }
    }
}
