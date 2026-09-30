using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dungeons2SkinLoader
{
    /// <summary>Persisting the skin list, layer setting and game folder to disk.</summary>
    public partial class MainForm
    {
        // -------------------------------------------------------------- config
        string ConfigPath { get { return Path.Combine(App.DataDir, "config.txt"); } }

        void SaveConfig()
        {
            var lines = new List<string> { "layers=" + (layers ? 1 : 0) };
            foreach (var s in slots)
                lines.Add(string.Join("|", s.HeroKey, s.ImagePath, s.Mode.ToString(), string.Join(" ", s.Eyes.Select(e => (e / 8) + "." + (e % 8))), s.Face != null ? s.Face.Serialize() : "", s.LidColor.HasValue ? "lid=" + s.LidColor.Value.ToString("x6") + (s.LidColor2.HasValue ? "," + s.LidColor2.Value.ToString("x6") : "") : ""));
            File.WriteAllLines(ConfigPath, lines);
            File.WriteAllText(Path.Combine(App.DataDir, "gamefolder.txt"), gameDir ?? "");
        }

        void LoadConfig()
        {
            loading = true;
            if (File.Exists(ConfigPath))
                foreach (var l in File.ReadAllLines(ConfigPath))
                {
                    if (l.StartsWith("layers=")) { layers = l.EndsWith("1"); continue; }
                    var p = l.Split('|'); if (p.Length < 5 || !File.Exists(p[2])) continue;
                    var s = new SkinSlot { HeroKey = p[0] + "|" + p[1], ImagePath = p[2] };
                    FaceMode m; if (Enum.TryParse(p[3], out m)) s.Mode = m;
                    if (p.Length > 5) s.Face = GameFace.Parse(p[5]);
                    if (p.Length > 6 && p[6].StartsWith("lid="))
                    {
                        var lc = p[6].Substring(4).Split(',');
                        s.LidColor = Convert.ToInt32(lc[0], 16);
                        if (lc.Length > 1) s.LidColor2 = Convert.ToInt32(lc[1], 16);
                    }
                    if (p[4].Trim() != "") s.Eyes = p[4].Split(' ').Select(e => int.Parse(e.Split('.')[0]) * 8 + int.Parse(e.Split('.')[1])).ToList();
                    if (gd.FindHero(s.HeroKey) != null) slots.Add(s);
                }
            layersBox.Checked = layers;
            var gf = Path.Combine(App.DataDir, "gamefolder.txt");
            if (File.Exists(gf)) { var g = File.ReadAllText(gf).Trim(); if (g != "" && Game.IsGame(g)) gameDir = g; }
            loading = false;
        }
    }
}
