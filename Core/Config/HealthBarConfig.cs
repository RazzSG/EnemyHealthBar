using System.ComponentModel;
using EnemyHealthBar.Core.Enums;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace EnemyHealthBar.Core.Config;

public class HealthBarConfig : ModConfig
{
    public static HealthBarConfig Instance => ModContent.GetInstance<HealthBarConfig>();
    
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [DefaultValue(HealthBarStyles.Fancy)]
    [DrawTicks]
    public HealthBarStyles Style { get; set; }
    
    [DefaultValue(DepletionDirection.RightToLeft)]
    [DrawTicks]
    public DepletionDirection DepletionDirection { get; set; }
    
    [DefaultValue(typeof(Color), "41, 255, 116, 255"), ColorNoAlpha]
    public Color ColorA { get; set; }
    
    [DefaultValue(typeof(Color), "255, 255, 188, 255"), ColorNoAlpha]
    public Color ColorB { get; set; }

    [DefaultValue(false)]
    public bool HealthInfo { get; set; }

    public override void OnChanged()
    {
        if (Style == HealthBarStyles.Default)
        {
            DepletionDirection = DepletionDirection.RightToLeft;
        }
    }
}