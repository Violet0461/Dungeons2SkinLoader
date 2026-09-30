using System.Collections.Generic;
using System.Linq;

namespace Dungeons2SkinLoader
{
    static class FriendScripts
    {
        public static Dictionary<string, string> Files(IEnumerable<string> heroes)
        {
            var names = string.Join(",", new[] { ModBuilder.ModName }.Concat(ModBuilder.LegacyNames).Select(n => "'" + n + "'"));
            var d = new Dictionary<string, string>();
            d["install.ps1"] = string.Join("\r\n", new[] {
                "# " + App.Name + " " + App.Version + " by " + App.Author + " - installs / removes the skin mod",
                "$ErrorActionPreference = 'Stop'",
                "$steam = (Get-ItemProperty 'HKCU:\\Software\\Valve\\Steam' -ErrorAction SilentlyContinue).SteamPath",
                "$libs = @()",
                "if ($steam) { $libs += $steam; $vdf = Join-Path $steam 'steamapps\\libraryfolders.vdf'",
                "  if (Test-Path $vdf) { $libs += (Select-String -Path $vdf -Pattern '\"path\"\\s+\"(.+)\"' | ForEach-Object { $_.Matches[0].Groups[1].Value -replace '\\\\\\\\','\\' }) } }",
                "$game = $libs | ForEach-Object { Join-Path $_ 'steamapps\\common\\Minecraft Dungeons II' } | Where-Object { Test-Path $_ } | Select-Object -First 1",
                "if (-not $game) { Write-Host 'Could not find Minecraft Dungeons II in your Steam libraries.'; exit 1 }",
                "if (Get-Process Dungeons-Win64-Shipping -ErrorAction SilentlyContinue) { Write-Host 'Close the game first, then run this again.'; exit 1 }",
                "$paks = Join-Path $game 'Dungeons\\Content\\Paks'",
                "$mods = Join-Path $paks '~mods'",
                "New-Item -ItemType Directory -Force $mods | Out-Null",
                "$here = Split-Path -Parent $MyInvocation.MyCommand.Path",
                "# remove any earlier install (older versions used other file names)",
                "$removed = 0",
                "foreach ($dir in $mods, $paks) { foreach ($n in " + names + ") { foreach ($ext in 'pak','utoc','ucas','sig') {",
                "  $f = Join-Path $dir \"$n.$ext\"; if (Test-Path $f) { [IO.File]::Delete($f); $removed++ } } }",
                "  $m = Join-Path $dir '" + Game.Marker + "'; if (Test-Path $m) { [IO.File]::Delete($m) } }",
                "if ($args[0] -eq 'uninstall') { Write-Host \"Custom skins removed ($removed files).\"; exit 0 }",
                "if ($removed -gt 0) { Write-Host \"Removed the previous install ($removed files).\" }",
                "Copy-Item (Join-Path $here '" + ModBuilder.ModName + ".*') $mods -Force",
                "Set-Content (Join-Path $mods '" + Game.Marker + "') @('" + App.Name + " by " + App.Author + "', 'version=" + App.Version + "', ('installed=' + (Get-Date -Format 'yyyy-MM-dd HH:mm')), 'heroes=" + string.Join(", ", heroes).Replace("'", "''") + "')",
                "Write-Host \"Custom skins installed to $mods\"" });
            d["INSTALL.bat"] = "@echo off\r\npowershell -NoProfile -ExecutionPolicy Bypass -File \"%~dp0install.ps1\"\r\npause\r\n";
            d["UNINSTALL.bat"] = "@echo off\r\npowershell -NoProfile -ExecutionPolicy Bypass -File \"%~dp0install.ps1\" uninstall\r\npause\r\n";
            d["README.txt"] = App.Name + " " + App.Version + " by " + App.Author + " - custom skins for Minecraft Dungeons II (cosmetic only)\r\n\r\n" +
                "Changes how these heroes look on your PC:\r\n  " + string.Join(", ", heroes) +
                "\r\n\r\nInstall:  close the game, extract this zip, double-click INSTALL.bat\r\n" +
                "          (an older install of this mod is detected and replaced automatically)\r\n" +
                "Remove:   close the game, double-click UNINSTALL.bat\r\n\r\n" +
                "Nothing about gameplay changes; only players who install this see the skins.\r\n" +
                "Fan-made; not affiliated with Mojang or Microsoft.\r\n";
            return d;
        }
    }
}
