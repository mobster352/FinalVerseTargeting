using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    // We give this window a constant ID using ###.
    // This allows for labels to be dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Final Verse Targeting")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(600, 400);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        var configTargetingTypeId = configuration.TargetingTypeId;
        if(ImGui.Combo("Targeting Type", ref configTargetingTypeId, configuration.targetingTypes, configuration.targetingTypes.Length))
        {
            configuration.TargetingTypeId = configTargetingTypeId;
            configuration.Save();
        }

        var configRadiusModifier = configuration.RadiusModifier;
        if(ImGui.InputInt("Radius Modifier", ref configRadiusModifier, 1, 1))
        {
            configuration.RadiusModifier = configRadiusModifier;
            configuration.Save();
        }

        var configAlpha = configuration.Alpha;
        if(ImGui.InputFloat("Alpha", ref configAlpha, 0.01f, 0.1f))
        {
            configuration.Alpha = configAlpha;
            configuration.Save();
        }

        var configThicc = configuration.Thicc;
        if(ImGui.InputFloat("Thickness", ref configThicc, 0.5f, 1f))
        {
            configuration.Thicc = configThicc;
            configuration.Save();
        }

        var configOffsetX = configuration.OffsetX;
        if(ImGui.InputFloat("Offset X", ref configOffsetX, 0.1f, 1f))
        {
            configuration.OffsetX = configOffsetX;
            configuration.Save();
        }

        var configOffsetY = configuration.OffsetY;
        if(ImGui.InputFloat("Offset Y", ref configOffsetY, 0.1f, 1f))
        {
            configuration.OffsetY = configOffsetY;
            configuration.Save();
        }

        // Can't ref a property, so use a local copy
        // var configValue = configuration.SomePropertyToBeSavedAndWithADefault;
        // if (ImGui.Checkbox("Random Config Bool", ref configValue))
        // {
        //     configuration.SomePropertyToBeSavedAndWithADefault = configValue;
        //     // Can save immediately on change if you don't want to provide a "Save and Close" button
        //     configuration.Save();
        // }

        // var movable = configuration.IsConfigWindowMovable;
        // if (ImGui.Checkbox("Movable Config Window", ref movable))
        // {
        //     configuration.IsConfigWindowMovable = movable;
        //     configuration.Save();
        // }

        // Enemy hitbox color picker
        // var packed = configuration.EnemyHitboxColor;
        // var color = new Vector4(
        //     ((packed >> 16) & 0xFF) / 255f,
        //     ((packed >> 8) & 0xFF) / 255f,
        //     (packed & 0xFF) / 255f,
        //     ((packed >> 24) & 0xFF) / 255f
        // );

        // if (ImGui.ColorEdit4("Enemy Hitbox Color", ref color))
        // {
        //     uint a = (uint)(color.W * 255f) & 0xFFu;
        //     uint r = (uint)(color.X * 255f) & 0xFFu;
        //     uint g = (uint)(color.Y * 255f) & 0xFFu;
        //     uint b = (uint)(color.Z * 255f) & 0xFFu;
        //     configuration.EnemyHitboxColor = (a << 24) | (r << 16) | (g << 8) | b;
        //     configuration.Save();
        // }
    }
}
