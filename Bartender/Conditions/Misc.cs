using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartender.Conditions;

[AttributeUsage(AttributeTargets.Class)]
public class MiscConditionAttribute : Attribute, IConditionCategory
{
    public string CategoryName => "Misc";
    public int DisplayPriority => 100;
}

[MiscCondition]
public class LoggedInCondition : ICondition
{
    public string ID => "l";
    public string ConditionName => "Is Logged In";
    public int DisplayPriority => 0;
    public bool Check(dynamic arg) => DalamudApi.ClientState.IsLoggedIn;
}

[MiscCondition]
public class WeaponDrawnCondition : ICondition
{
    public string ID => "wd";
    public string ConditionName => "Weapon Drawn";
    public int DisplayPriority => 0;
    public bool Check(dynamic arg) => DalamudApi.ClientState.LocalPlayer is { } player && (player.StatusFlags & StatusFlags.WeaponOut) != 0;
}

[MiscCondition]
public class HUDLayoutCondition : ICondition, IDrawableCondition, IArgCondition
{
    public string ID => "hl";
    public string ConditionName => "Current HUD Layout";
    public int DisplayPriority => 0;
    public bool Check(dynamic arg) => (byte)arg == Game.CurrentHUDLayout;
    public string GetTooltip(CondConfig cndCfg) => null;
    public string GetSelectableTooltip(CondConfig cndCfg) => null;
    public void Draw(CondConfig cndCfg)
    {
        var _ = (int)cndCfg.Arg + 1;
        if (ImGui.SliderInt("##HUDLayout", ref _, 1, 4))
        {
            cndCfg.Arg = _ - 1;
            Bartender.Configuration.Save();
        }
    }
    public dynamic GetDefaultArg(CondConfig cndCfg) => Game.CurrentHUDLayout;
}

[MiscCondition]
public class PetCondition : ICondition
{
    public string ID => "pe";
    public string ConditionName => "Pet Is Out";
    public int DisplayPriority => 0;
    public unsafe bool Check(dynamic arg) => Framework.Instance()->GetUiModule()->GetPronounModule()->ResolvePlaceholder("<pet>", 0, 0) != null;
}

[MiscCondition]
public class PluginCondition : ICondition, IDrawableCondition, IArgCondition
{
    public string ID => "p";
    public string ConditionName => "Plugin Enabled";
    public int DisplayPriority => 102;
    public bool Check(dynamic arg) => arg is string plugin && DalamudApi.PluginInterface.InstalledPlugins.Any(p => p.IsLoaded && p.InternalName == plugin);
    public string GetTooltip(CondConfig cfg) => null;
    public string GetSelectableTooltip(CondConfig cfg) => null;
    public void Draw(CondConfig cfg)
    {
        if (ImGui.BeginCombo("##PluginsList", cfg.Arg is string ? cfg.Arg : string.Empty))
        {
            var i = 0;
            foreach (var plugin in DalamudApi.PluginInterface.InstalledPlugins)
            {
                var name = plugin.InternalName;
                if (!ImGui.Selectable($"{name}##{i++}", cfg.Arg == name)) continue;

                cfg.Arg = name;
                Bartender.Configuration.Save();
            }
            ImGui.EndCombo();
        }
    }
    public dynamic GetDefaultArg(CondConfig cfg) => cfg.Arg is string ? cfg.Arg : string.Empty;
}

[MiscCondition]
public class SanctuaryCondition : ICondition
{
    public string ID => "is";
    public string ConditionName => "In Sanctuary";
    public int DisplayPriority => 0;
    public bool Check(dynamic arg) => FFXIVClientStructs.FFXIV.Client.Game.GameMain.IsInSanctuary();
    public string GetTooltip(CondConfig cfg) => null;
    public string GetSelectableTooltip(CondConfig cfg) => "This is where you can get experience boosts";
}

[MiscCondition]
public class PvPZoneCondition : ICondition
{
    public string ID => "pz";
    public string ConditionName => "Is In PvP Zone";
    public int DisplayPriority => 0;
    public bool Check(dynamic arg) => FFXIVClientStructs.FFXIV.Client.Game.GameMain.IsInPvPArea();
    public string GetTooltip(CondConfig cfg) => null;
    public string GetSelectableTooltip(CondConfig cfg) => null;
}

[MiscCondition]
public class PartyCondition : ICondition, IDrawableCondition, IArgCondition
{
    public string ID => "pt";
    public string ConditionName => "# Of Party Members";
    public int DisplayPriority => 0;
    public unsafe bool Check(dynamic arg) => DalamudApi.PartyList.Length == arg;
    public string GetTooltip(CondConfig cfg) => null;
    public string GetSelectableTooltip(CondConfig cfg) => "True if there is enough members in your party";
    public void Draw(CondConfig cfg)
    {
        var _ = (int)cfg.Arg + 1;
        if (ImGui.SliderInt("##MemberCount", ref _, 1, 8))
        {
            cfg.Arg = _ - 1;
            Bartender.Configuration.Save();
        }
    }
    public dynamic GetDefaultArg(CondConfig cfg) => 1;
}

[MiscCondition]
public class IsInHomeWorldCondition : ICondition
{
    public string ID => "hw";
    public string ConditionName => "Is In Home World";
    public int DisplayPriority => 0;
    public bool Check(dynamic arg) => DalamudApi.ClientState.LocalPlayer?.CurrentWorld.Id == DalamudApi.ClientState.LocalPlayer?.HomeWorld.Id;
    public string GetTooltip(CondConfig cfg) => null;
    public string GetSelectableTooltip(CondConfig cfg) => "Check if the current character is in their home world";
}
