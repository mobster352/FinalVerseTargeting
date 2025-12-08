using Dalamud.Configuration;
using System;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    // Packed ARGB color used for enemy hitboxes (0xAARRGGBB)
    public uint EnemyHitboxColor { get; set; } = 0x80FF0000; // default: semi-transparent red
    public float Alpha {get; set;} = 1.0f;
    public float Thicc {get;set;} = 25f;
    public float OffsetX {get;set;} = 0f;
    public float OffsetY {get;set;} = 0f;
    public int TargetingTypeId {get;set;} = 0;
    public int RadiusModifier {get;set;} = 0;
    public readonly string[] targetingTypes = new[]{ "Solid", "Dashed"};


    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public enum TargetingType: int
    {
        SOLID = 0,
        DASHED = 1
    }
}
