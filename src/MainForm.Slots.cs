using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Dungeons2SkinLoader
{
    /// <summary>Adding, replacing and removing skin slots.</summary>
    public partial class MainForm
    {
        // -------------------------------------------------------------- slots
        static readonly string[] HeroOrder =
        {
            "Darian", "Eshe", "Greta", "Javier", "Nuru", "Violet", "Qamar", "Esperanza",
            "Healer", "Ranger", "Tank", "Valorie", "PizzaChef", "Steve", "Alex"
        };

        void BrowseAdd()
        {
            using (var d = new OpenFileDialog { Filter = "Minecraft skin (*.png)|*.png", Multiselect = true, Title = "Choose Minecraft skin PNG files" })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                foreach (var f in d.FileNames) AddSkin(f);
            }
        }

        void AddSkin(string file)
        {
            Img im;
            try { im = Converter.To64(Img.FromFile(file)); }
            catch (Exception ex) { Msg("That file can't be used as a skin:\r\n\r\n" + ex.Message, true); return; }

            var used = new HashSet<string>(slots.Select(s => s.HeroKey));
            var free = HeroOrder.Select(n => n + "|0").Concat(gd.Heroes.Select(h => h.Key))
                .FirstOrDefault(k => !used.Contains(k) && gd.FindHero(k) != null);
            if (free == null) { Msg("Every hero already has a custom skin. Remove one first.", true); return; }

            var dir = Path.Combine(App.DataDir, "skins");
            Directory.CreateDirectory(dir);
            var dst = Path.Combine(dir, Guid.NewGuid().ToString("N").Substring(0, 12) + ".png");
            File.Copy(file, dst);
            File.WriteAllText(dst + ".name", Path.GetFileName(file));

            var slot = new SkinSlot { HeroKey = free, ImagePath = dst };
            var eyes = Converter.DetectEyes(im);
            if (eyes.Count > 0) { slot.Mode = FaceMode.Blink; slot.Eyes = eyes; }

            slots.Add(slot);
            SaveConfig();
            RebuildCards();
            Select(slots.Count - 1);
            listPanel.ScrollControlIntoView(cards[cards.Count - 1]);
            toast.Show("Added " + Path.GetFileName(file) + " as " + gd.FindHero(free).Display + ". Install when you're ready!", Theme.Blue);
        }

        void ChangeImage()
        {
            var s = Current;
            if (s == null) return;
            using (var d = new OpenFileDialog { Filter = "Minecraft skin (*.png)|*.png", Title = "Choose a Minecraft skin PNG" })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                try { Converter.To64(Img.FromFile(d.FileName)); }
                catch (Exception ex) { Msg("That file can't be used as a skin:\r\n\r\n" + ex.Message, true); return; }

                File.Copy(d.FileName, s.ImagePath, true);
                File.WriteAllText(s.ImagePath + ".name", Path.GetFileName(d.FileName));
                s.Eyes.Clear();
                if (s.Mode == FaceMode.Blink) AutoEyes();
                Changed();
            }
        }

        void AutoEyes(bool warn = true)
        {
            var s = Current;
            if (s == null) return;
            try { s.Eyes = Converter.DetectEyes(Converter.To64(Img.FromFile(s.ImagePath))); }
            catch { }
            if (s.Eyes.Count == 0 && warn) toast.Show("Couldn't spot the eyes automatically. Click them on the face grid.", Theme.Warn);
        }

        void RemoveSlot()
        {
            var s = Current;
            if (s == null) return;

            slots.Remove(s);
            texCache.Remove(s);
            thumbCache.Remove(s);
            try { File.Delete(s.ImagePath); File.Delete(s.ImagePath + ".name"); }
            catch { }

            SaveConfig();
            RebuildCards();
            Select(Math.Min(current, slots.Count - 1));
            toast.Show("Skin removed. Click \"Install to game\" to apply.", Theme.Dim);
        }
    }
}
