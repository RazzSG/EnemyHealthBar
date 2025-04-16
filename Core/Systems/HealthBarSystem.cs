using System.Collections.Generic;
using System.Linq;
using EnemyHealthBar.Core.Config;
using EnemyHealthBar.Core.Enums;
using EnemyHealthBar.Styles.Base;
using Terraria;
using Terraria.ModLoader;

namespace EnemyHealthBar.Core.Systems;

public class HealthBarSystem : ModSystem
{
	internal static readonly List<HealthBar> Styles = [];

    public override void Load()
    {
        On_Main.DrawHealthBar += DrawHealthBar;
    }
    
    public override void Unload()
    {
        On_Main.DrawHealthBar -= DrawHealthBar;
    }
    
    private void DrawHealthBar(On_Main.orig_DrawHealthBar orig, Main self, float x, float y, int health, int maxhealth, float alpha, float scale, bool noflip)
    {
	    HealthBarStyles selectedStyle = HealthBarConfig.Instance.Style;
        
        if (selectedStyle == HealthBarStyles.Default)
        {
            orig.Invoke(self, x, y, health, maxhealth, alpha, scale, noflip);
        }
        
        foreach (var healthBarStyle in Styles.Where(healthBarStyle => healthBarStyle.Style == selectedStyle))
        {
            healthBarStyle.DrawHealthBar(x, y, health, maxhealth, alpha, scale);
        }
    }
}