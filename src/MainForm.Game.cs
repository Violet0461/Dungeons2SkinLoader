using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Finding the game, installing/uninstalling the mod, and exporting it for a friend.</summary>
    public partial class MainForm
    {
        // ---------------------------------------------------------------- game
        void DetectGame(string found)
        {
            if (gameDir == null) gameDir = found;
            var green = Color.FromArgb(88, 206, 110);
            if (gameDir == null) statusPill.Set(Theme.Bad, "Game not found", "set the folder below");
            else if (!Game.VersionMatches(gd, gameDir)) statusPill.Set(Theme.Warn, "Different game version");
            else
            {
                string ver; var st = Game.Detect(gameDir, out ver);
                if (st == Game.State.Current) statusPill.Set(green, "Game found", "skins installed");
                else if (st == Game.State.Old) statusPill.Set(Theme.Warn, "Old version installed", "install to update");
                else statusPill.Set(green, "Game found", "ready");
            }
            SaveConfig();
        }

        void PickGameFolder()
        {
            using (var d = new FolderBrowserDialog { Description = "Select the \"Minecraft Dungeons II\" folder (the one that contains Dungeons.exe)." })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                if (!Game.IsGame(d.SelectedPath)) { Msg("That folder doesn't look like Minecraft Dungeons II. Pick the folder that contains Dungeons.exe.", true); return; }
                gameDir = d.SelectedPath; DetectGame(null);
            }
        }

        bool ReadyForGame()
        {
            if (gameDir == null) { Msg("Minecraft Dungeons II wasn't found. Use \"Change game folder\" at the top right.", true); return false; }
            if (Game.Running()) { Msg("Close Minecraft Dungeons II first, then try again.", true); return false; }
            return true;
        }

        Dictionary<string, byte[]> BuildMod()
        {
            Cursor = Cursors.WaitCursor;
            try { return ModBuilder.Build(gd, slots, layers, null); }
            finally { Cursor = Cursors.Default; }
        }

        void Install()
        {
            if (slots.Count == 0) { Msg("Add at least one skin first.", true); return; }
            if (!ReadyForGame()) return;
            if (!Game.VersionMatches(gd, gameDir) &&
                MessageBox.Show(this, "Your game is a different version than this app was made for (probably a game update).\r\n\r\nThe mod may not load, or could look broken. You can always remove it again with \"Uninstall mod\".\r\n\r\nInstall anyway?",
                    App.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                int removed = Game.Install(gameDir, BuildMod(), slots.Select(s => gd.FindHero(s.HeroKey).Display));
                DetectGame(null);
                toast.Show((removed > 0 ? "✔   Replaced the previous install. " : "✔   ") + "Installed " + slots.Count + " skin" + (slots.Count == 1 ? "" : "s") + "!  Pick the hero in the Locker.", Theme.Accent);
            }
            catch (UnauthorizedAccessException) { Msg("Windows didn't allow writing to the game folder. Try running the app as administrator.", true); }
            catch (Exception ex) { Msg("Install failed:\r\n\r\n" + ex.Message, true); }
        }

        void RemoveMod()
        {
            if (!ReadyForGame()) return;
            if (!Game.Installed(gameDir)) { toast.Show("The mod isn't installed. The game is using its normal skins.", Theme.Dim); return; }
            if (MessageBox.Show(this, "Remove the skin mod from the game? Your skins stay in this app, so you can install them again later.", App.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { Game.Uninstall(gameDir); DetectGame(null); toast.Show("Mod removed. The game is back to its normal skins.", Theme.Accent); }
            catch (Exception ex) { Msg("Couldn't remove the mod:\r\n\r\n" + ex.Message, true); }
        }

        void Export()
        {
            if (slots.Count == 0) { Msg("Add at least one skin first.", true); return; }
            using (var d = new SaveFileDialog { Filter = "Zip file (*.zip)|*.zip", FileName = "Dungeons2SkinLoader_skins.zip", Title = "Save the mod for a friend" })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var files = BuildMod();
                    if (File.Exists(d.FileName)) File.Delete(d.FileName);
                    using (var z = ZipFile.Open(d.FileName, ZipArchiveMode.Create))
                    {
                        foreach (var kv in files) using (var st = z.CreateEntry(kv.Key).Open()) st.Write(kv.Value, 0, kv.Value.Length);
                        foreach (var kv in FriendScripts.Files(slots.Select(s => gd.FindHero(s.HeroKey).Display)))
                            using (var w = new StreamWriter(z.CreateEntry(kv.Key).Open())) w.Write(kv.Value);
                    }
                    toast.Show("Saved " + Path.GetFileName(d.FileName) + " - your friend unzips it and runs INSTALL.bat", Theme.Accent);
                }
                catch (Exception ex) { Msg("Export failed:\r\n\r\n" + ex.Message, true); }
            }
        }

        void ShowHelp()
        {
            Msg("How it works\r\n\r\n" +
                "1.  Add Minecraft skin PNGs (drag them in or click \"Add skin\").\r\n" +
                "2.  For each one, pick the hero it replaces and how the face should behave.\r\n" +
                "3.  Close the game and click \"Install to game\".\r\n" +
                "4.  In the game, pick that hero in the Locker.\r\n\r\n" +
                "Only the look of the heroes changes; gameplay is untouched, and only players who installed the mod see your skins. " +
                "To see each other's skins, friends need the same mod (\"Export for a friend\").\r\n\r\n" +
                "\"Uninstall mod\" puts everything back to normal. After a game update the mod may stop loading until the app is updated.\r\n\r\n" +
                "Installing again always replaces the previous install (older versions included), so there are never two copies.\r\n\r\n" +
                App.Name + " " + App.Version + " - made by " + App.Author + ". Fan-made; not affiliated with Mojang or Microsoft.", false);
        }
    }
}
