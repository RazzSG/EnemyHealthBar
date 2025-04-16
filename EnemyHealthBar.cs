using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace EnemyHealthBar;

public class EnemyHealthBar : Mod
{
    public static Asset<Effect> AnimatedFillEffect;
    
    public override void Load()
    {
        if (!Main.dedServ)
        {
            AnimatedFillEffect = Assets.Request<Effect>("Assets/Effects/AnimatedFillEffect", AssetRequestMode.ImmediateLoad);
            
            GameShaders.Misc["EnemyHealthBar:AnimatedFillEffect"] = new MiscShaderData(AnimatedFillEffect, "AnimatedFillPass");
        }
    }
    
    public override void Unload()
    {
       AnimatedFillEffect = null;
    }
}