using HarmonyLib;
using PluginAPI.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JailbirdGlassFix;

public class Plugin
{
    private Harmony _harmony;

    [PluginEntryPoint("Jailbird Glass Fix", "1.0.0", "Fixes a base-game issue where breaking a window with the Jailbird item crashes the player.", "GBN")]
    public void EntryPoint()
    {
        _harmony = new Harmony($"GBN-JAILBIRDFIX-{DateTime.Now}");
        _harmony.PatchAll();    
    }
}
