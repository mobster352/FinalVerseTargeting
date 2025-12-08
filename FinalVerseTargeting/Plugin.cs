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

    // private const string CommandName = "/fv";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("FinalVerseTargeting");
    private ConfigWindow ConfigWindow { get; init; }
    // private MainWindow MainWindow { get; init; }

    private IGameObject CurrentTarget;

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
}