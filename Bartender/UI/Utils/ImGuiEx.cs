using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Component.Excel;
using ImGuiNET;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.UI.Utils;

public static class ImGuiEx
{
    public static void SetItemTooltip(string s, ImGuiHoveredFlags flags = ImGuiHoveredFlags.None)
    {
        if (ImGui.IsItemHovered(flags))
            ImGui.SetTooltip(s);
    }

    public static void SetLocalizedTooltip(string s, ImGuiHoveredFlags flags = ImGuiHoveredFlags.None)
        => SetItemTooltip(Localization.Get(s), flags);

    private static bool SliderEnabled = false;
    private static bool SliderVertical = false;
    private static float SliderInterval = 0;
    private static int LastHitInterval = 0;
    private static Action<bool, bool, bool> SliderAction;
    public static void SetupSlider(bool vertical, float interval, Action<bool, bool, bool> action)
    {
        SliderEnabled = true;
        SliderVertical = vertical;
        SliderInterval = interval;
        LastHitInterval = 0;
        SliderAction = action;
    }

    public static void DoSlider()
    {
        if (!SliderEnabled) return;

        var popupOpen = !ImGui.IsPopupOpen("_SLIDER") && ImGui.IsPopupOpen(null, ImGuiPopupFlags.AnyPopup);
        if (!popupOpen)
        {
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(new Vector2(-100));
            ImGui.OpenPopup("_SLIDER", ImGuiPopupFlags.NoOpenOverItems);
            if (!ImGui.BeginPopup("_SLIDER")) return;
        }

        var drag = SliderVertical ? ImGui.GetMouseDragDelta().Y : ImGui.GetMouseDragDelta().X;
        var dragInterval = (int)(drag / SliderInterval);
        var hit = false;
        var increment = false;
        if (dragInterval > LastHitInterval)
        {
            hit = true;
            increment = true;
        }
        else if (dragInterval < LastHitInterval)
        {
            hit = true;
        }

        var closing = !ImGui.IsMouseDown(ImGuiMouseButton.Left);

        if (LastHitInterval != dragInterval)
        {
            while (LastHitInterval != dragInterval)
            {
                LastHitInterval += increment ? 1 : -1;
                SliderAction(hit, increment, closing && LastHitInterval == dragInterval);
            }
        }
        else
            SliderAction(false, false, closing);

        if (closing)
            SliderEnabled = false;

        if (!popupOpen)
            ImGui.EndPopup();
    }

    private static string search = string.Empty;
    private static HashSet<uint> filtered;
    public static bool ExcelSheetCombo<T>(
        string id,
        [NotNullWhen(true)] out T? selected,
        Func<ExcelSheet<T>, string> getPreview,
        ImGuiComboFlags flags,
        Func<T, string, bool> searchPredicate,
        Func<T, bool> selectableDrawing) where T : struct, IExcelRow<T>
    {
        var sheet = DalamudApi.DataManager.GetExcelSheet<T>();
        return ExcelSheetCombo(id, out selected, getPreview(sheet), flags, sheet, searchPredicate, selectableDrawing);
    }

    public static bool ExcelSheetCombo<T>(string id,
        [NotNullWhen(true)] out T? selected,
        string preview,
        ImGuiComboFlags flags,
        ExcelSheet<T> sheet,
        Func<T, string, bool> searchPredicate,
        Func<T, bool> drawRow) where T : struct, IExcelRow<T>
    {
        selected = default;
        if (!ImGui.BeginCombo(id, preview, flags))
            return false;

        if (ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() && !ImGui.IsAnyItemActive())
        {
            search = string.Empty;
            filtered = null;
            ImGui.SetKeyboardFocusHere(0);
        }

        if (ImGui.InputText("##ExcelSheetComboSearch", ref search, 128))
            filtered = null;

        filtered ??= sheet.Where(s => searchPredicate(s, search)).Select(s => s.RowId).ToHashSet();

        var i = 0;
        foreach (var rowID in filtered)
        {
            if (sheet.GetRowOrDefault(rowID) is not { } row)
                continue;

            ImGui.PushID(i++);
            if (drawRow(row))
                selected = row;
            ImGui.PopID();

            if (selected == null)
                continue;
            ImGui.EndCombo();
            return true;
        }

        ImGui.EndCombo();
        return false;
    }
}
