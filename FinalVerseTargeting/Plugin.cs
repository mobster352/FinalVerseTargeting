﻿using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System;
using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using SamplePlugin.Windows;
using System.Runtime.Serialization;
using Dalamud.Game.ClientState.Objects.Types;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Linq;
using Dalamud.Game.ClientState.Objects;
using System.Drawing;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static Dalamud.Plugin.Services.IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static Dalamud.Plugin.Services.IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static Dalamud.Plugin.Services.IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;

    // private const string CommandName = "/fv";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("FinalVerseTargeting");
    private ConfigWindow ConfigWindow { get; init; }
    // private MainWindow MainWindow { get; init; }

    private IGameObject CurrentTarget;
    private float hp1;
    private float hp2;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        
        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        // MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        // WindowSystem.AddWindow(MainWindow);

        // CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        // {
        //     HelpMessage = "A useful message to display in /xlhelp"
        // });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Log.Information("Overlay draw handler registered");

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        // PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        PluginInterface.UiBuilder.Draw += Draw;
        PluginInterface.UiBuilder.Draw += DrawRing;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");

        // IPlayerCharacter player;
        Log.Information($"ClientState: {ClientState.IsLoggedIn}");
        Log.Information($"PlayerState: {PlayerState.IsLoaded}");
        // foreach(IGameObject obj in ObjectTable.ClientObjects)
        // {
        //     // Log.Information($"obj name: {obj.Name}");
        // }
        // Log.Information($"Count: {ObjectTable.PlayerObjects.Count()}");
        
        
        // Log.Information({player.TargetObjectId});
        // IBattleNpc target = null;
        // Log.Information(target.Name);
        // Character.GetTargetId();

        // Framework.RunOnTick(() => {
        //     Log.Information($"Count: {Plugin.ObjectTable.CharacterManagerObjects.Count()}");
        //     foreach(IGameObject obj in ObjectTable.CharacterManagerObjects)
        //     {
        //         Log.Information($"obj name: {obj.Name}");
        //         if(obj.Name.ToString() == ("Garden Crocota"))
        //         {
        //             Log.Information($"ID: {obj.GameObjectId}");
        //         }
        //     }

        //     IGameObject targetObject = TargetManager.Target;
        //     Log.Information($"Target: {targetObject}");
        //     if(targetObject != null)
        //         Log.Information($"Target Hitbox: {targetObject.HitboxRadius}");
        //     Log.Information($"Player Target: {ClientState.LocalPlayer.TargetObjectId}");
        // });
        Framework.Update += this.OnFrameworkTick;
    }

    private void OnFrameworkTick(IFramework framework) {
        IGameObject targetObject = TargetManager.Target;
        if(targetObject != null){
            if((CurrentTarget == null || CurrentTarget.GameObjectId != targetObject.GameObjectId) && (targetObject.Name.ToString() == "Devoured Eater" || targetObject.Name.ToString() == "Eminent Grief")){
                CurrentTarget = targetObject;
                // Log.Information($"Current Target: {CurrentTarget.Name.ToString()}");
            }
        }
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        // PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        PluginInterface.UiBuilder.Draw -= Draw;
        PluginInterface.UiBuilder.Draw -= DrawRing;

        Framework.Update -= this.OnFrameworkTick;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        // MainWindow.Dispose();

        // CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        // MainWindow.Toggle();
        try
        {
            Log.Information($"/pmycommand status");
        }
        catch (Exception ex)
        {
            Log.Warning($"Status check failed: {ex.Message}");
        }
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    // public void ToggleMainUi() => MainWindow.Toggle();

    private void DrawRing()
    {
        if(TargetManager.Target == null) return;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        Dalamud.Interface.Utility.ImGuiHelpers.ForceNextWindowMainViewport();
        Dalamud.Interface.Utility.ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
        ImGui.Begin("Canvas",
            ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoFocusOnAppearing);
        ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

        // Log.Information($"Target: {CurrentTarget.Name.ToString()}");

        var xOff = 0f + Configuration.OffsetX;
        var yOff = 0f + Configuration.OffsetY;
        var radius = CurrentTarget.HitboxRadius + Configuration.RadiusModifier;
        var numSegments = 100;
        float segAng = MathF.Tau / numSegments;
        var zed = 0f;
        var fill = false;
        uint colour;
        var thicc = Configuration.Thicc;

        if(CurrentTarget.Name.ToString() == "Devoured Eater")
            colour = ImGui.GetColorU32(new Vector4(144, 0, 255, Configuration.Alpha));
        else
            colour = ImGui.GetColorU32(new Vector4(1, 1, 1, Configuration.Alpha));

        for (var i = 0; i < numSegments; i++)
        {
            if(Configuration.TargetingTypeId == ((int)Configuration.TargetingType.SOLID))
            {
                GameGui.WorldToScreen(new Vector3(
                    CurrentTarget.Position.X + xOff + (radius * MathF.Sin(segAng * i)),
                    CurrentTarget.Position.Y+zed,
                    CurrentTarget.Position.Z + yOff + (radius * MathF.Cos(segAng * i))
                    ),
                    out Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos.X, pos.Y));
            }
            else if(Configuration.TargetingTypeId == ((int)Configuration.TargetingType.DASHED))
            {
                GameGui.WorldToScreen(new Vector3(
                    CurrentTarget.Position.X + xOff + radius * MathF.Sin(segAng * i),
                    CurrentTarget.Position.Y+zed,
                    CurrentTarget.Position.Z + yOff + radius * MathF.Cos(segAng * i)),
                    out var pos1);
                GameGui.WorldToScreen(new Vector3(
                    CurrentTarget.Position.X + xOff + radius * MathF.Sin(segAng * (i + 0.4f)),
                    CurrentTarget.Position.Y+zed,
                    CurrentTarget.Position.Z + yOff + radius * MathF.Cos(segAng * (i + 0.4f))),
                    out var pos2);
                ImGui.GetWindowDrawList().AddLine(pos1, pos2, colour, thicc);
            }
        }
        if (fill) { ImGui.GetWindowDrawList().PathFillConvex(colour); }
        else { ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.Closed, thicc); }
    }

    private void DrawBox(float x, float y, float hp, int i)
    {
        // i=10 - second target
        // i=11 - first target
        if(i!=10 && i!=11) return;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        Dalamud.Interface.Utility.ImGuiHelpers.ForceNextWindowMainViewport();
        Dalamud.Interface.Utility.ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
        ImGui.Begin("Canvas",
            ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoFocusOnAppearing);
        ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

        uint colour_g = ImGui.GetColorU32(new Vector4(0, 255, 0, Configuration.Alpha));
        uint colour_y = ImGui.GetColorU32(new Vector4(111, 255, 0, Configuration.Alpha));
        uint colour_r = ImGui.GetColorU32(new Vector4(255, 0, 0, Configuration.Alpha));

        Vector2 topLeft = new Vector2(x-35,y-15);
        Vector2 bottomRight = new Vector2(x+5,y+15);

        if (i == 10)
        {
            if(hp1 - hp2 < 10)
                ImGui.AddRectFilled(ImGui.GetWindowDrawList(), topLeft, bottomRight, colour_g);
            else if(hp1 - hp2 >= 10 && hp1 - hp2 < 15)
                ImGui.AddRectFilled(ImGui.GetWindowDrawList(), topLeft, bottomRight, colour_y);
            else
                ImGui.AddRectFilled(ImGui.GetWindowDrawList(), topLeft, bottomRight, colour_r);
        }

        if (i == 11)
        {
            if(hp2 - hp1 < 10)
                ImGui.AddRectFilled(ImGui.GetWindowDrawList(), topLeft, bottomRight, colour_g);
            else if(hp2 - hp1 >= 10 && hp2 - hp1 < 15)
                ImGui.AddRectFilled(ImGui.GetWindowDrawList(), topLeft, bottomRight, colour_y);
            else
                ImGui.AddRectFilled(ImGui.GetWindowDrawList(), topLeft, bottomRight, colour_r);
        }
    }

    private unsafe void Draw()
    {
        if (Configuration.UseColorBox && Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] && !ClientState.IsPvP)
        {
            try
            {
                var enlist = GameGui.GetAddonByName("_EnemyList", 1);
                if(enlist != IntPtr.Zero)
                {
                    var enlistAtk = (AtkUnitBase*)enlist.Address;
                    if (enlistAtk->UldManager.NodeListCount < 12) return;
                    var baseX = enlistAtk->X;
                    var baseY = enlistAtk->Y;
                    if (enlistAtk->IsVisible)
                    {
                        for (int i = 4; i <= 11; i++)
                        {
                            var enemyTile = (AtkComponentNode*)enlistAtk->UldManager.NodeList[i];
                            if (enemyTile->AtkResNode.IsVisible())
                            {
                                if (enemyTile->Component->UldManager.NodeListCount < 11) continue;
                                var enemyBar = (AtkImageNode*)enemyTile->Component->UldManager.NodeList[10];
                                var hp = (enemyBar->AtkResNode.ScaleX * 100f);
                                if(i==11) hp1 = hp;
                                if(i==10) hp2 = hp;
                                DrawBox(enemyTile->AtkResNode.X * enlistAtk->Scale + baseX - 2f,
                                    enemyTile->AtkResNode.Y * enlistAtk->Scale + enemyTile->AtkResNode.Height * enlistAtk->Scale / 2f + baseY,
                                    hp,
                                    i);
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);
            }
        }
    }
}