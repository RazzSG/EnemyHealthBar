using System;
using System.IO;
using EnemyHealthBar.Core.Config;
using EnemyHealthBar.Core.Enums;
using EnemyHealthBar.Styles.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace EnemyHealthBar.Styles;

public sealed class FancyHealthBar : HealthBar
{
    public override HealthBarStyles Style => HealthBarStyles.Fancy;
    
    public override DepletionDirection Direction => HealthBarConfig.Instance.DepletionDirection;
    
    public override Asset<Texture2D> BackgroundTexture => Mod.Assets.Request<Texture2D>("Assets/FancyHealthBarBG");
    
    public override Asset<Texture2D> FillTexture => Mod.Assets.Request<Texture2D>("Assets/FancyHealthBarFill");
    
    public override Asset<Texture2D> DamageFillTexture => Mod.Assets.Request<Texture2D>("Assets/FancyHealthBarDamageFill");
    
    public override void DrawHealthBar(float x, float y, int health, int maxHealth, float alpha, float scale)
    {
        if (health <= 0)
            return;
        
        int barWidth = BackgroundTexture.Width();
        Vector2 barPosition = new Vector2(x - barWidth / 2f * scale, y);
        
        float currentHealth = MathHelper.Clamp((float)health / (float)maxHealth, 0f, 1f);
        float percentHealth = currentHealth * 100;
        
        NPC targetNPC = null;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.active && npc.life == health && npc.lifeMax == maxHealth)
            {
                targetNPC = npc;
                break;
            }
        }
        
        Player targetPlayer = null;
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.active && player.statLife == health && player.statLifeMax2 == maxHealth)
            {
                targetPlayer = player;
                break;
            }
        }
        
        float damageFillPercent = 1f;
        
        if (targetNPC != null && targetNPC.TryGetGlobalNPC(out FancyHealthBarGlobalNPC npcData))
            damageFillPercent = npcData.DamageFillPercent;
        else if (targetPlayer != null && targetPlayer.TryGetModPlayer(out FancyHealthBarPlayer playerData))
            damageFillPercent = playerData.DamageFillPercent;
        
        int fillWidth = Math.Max(2, (int)(barWidth * currentHealth));
        int tailWidth = Math.Max(2, (int)(barWidth * damageFillPercent));
        Vector2 fillPosition = Vector2.Zero;
        Vector2 tailPosition = Vector2.Zero;
        Rectangle fillRect = Rectangle.Empty;
        Rectangle tailRect = Rectangle.Empty;

        if (HealthBarConfig.Instance.DepletionDirection == DepletionDirection.RightToLeft)
        {
            fillPosition = barPosition;
            tailPosition = barPosition;
            
            fillRect = new Rectangle(0, 0, fillWidth, FillTexture.Value.Height);
            tailRect = new Rectangle(0, 0, tailWidth, DamageFillTexture.Value.Height);
        }
        else if (HealthBarConfig.Instance.DepletionDirection == DepletionDirection.Symmetrical)
        {
            int fillOffset = (barWidth - fillWidth) / 2;
            int tailOffset = (barWidth - tailWidth) / 2;
            
            fillPosition = new Vector2(barPosition.X + fillOffset * scale, barPosition.Y);
            tailPosition = new Vector2(barPosition.X + tailOffset * scale, barPosition.Y);
            
            fillRect = new Rectangle(fillOffset, 0, fillWidth, FillTexture.Value.Height);
            tailRect = new Rectangle(tailOffset, 0, tailWidth, DamageFillTexture.Value.Height);
        }
        
        Vector2 origin = Vector2.Zero;
        SpriteEffects spriteEffect = SpriteEffects.None;
        
        if (Main.LocalPlayer.gravDir == -1f)
        {
            origin.Y += BackgroundTexture.Height();
            spriteEffect = SpriteEffects.FlipVertically;
            
            barPosition.Y = Main.screenHeight - (barPosition.Y - Main.screenPosition.Y) + Main.screenPosition.Y;
        }
        
        Vector2 drawPosition = barPosition - Main.screenPosition;
        Vector2 fillDrawPosition = fillPosition - Main.screenPosition;
        Vector2 tailDrawPosition = tailPosition - Main.screenPosition;
        
        Main.spriteBatch.Draw(BackgroundTexture.Value, drawPosition, new Rectangle(0, 0, barWidth, BackgroundTexture.Height()), Color.White * alpha, 0f, origin, scale, spriteEffect, 0f);
        
        Main.spriteBatch.Draw(DamageFillTexture.Value, tailDrawPosition, tailRect, Color.White * 0.6f * alpha, 0f, origin, scale, spriteEffect, 0f);
        
        MiscShaderData fillEffect = GameShaders.Misc["EnemyHealthBar:AnimatedFillEffect"];
        fillEffect.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
        fillEffect.Shader.Parameters["colorA"]?.SetValue(HealthBarConfig.Instance.ColorA.ToVector3());
        fillEffect.Shader.Parameters["colorB"]?.SetValue(HealthBarConfig.Instance.ColorB.ToVector3());
        fillEffect.Shader.Parameters["amplitude"]?.SetValue(0.15f);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, EnemyHealthBar.AnimatedFillEffect.Value, Main.GameViewMatrix.TransformationMatrix);
        
        Main.spriteBatch.Draw(FillTexture.Value, fillDrawPosition, fillRect, Color.White * alpha, 0f, origin, scale, spriteEffect, 0f);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        
        if (HealthBarConfig.Instance.HealthInfo)
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, $"{percentHealth:F0}%", drawPosition - new Vector2(0f, 6.5f), new Color(255, 194, 196),  Color.Black, 0f, origin, Vector2.One * 0.65f, -1f, 1f);
    }
}

public class FancyHealthBarGlobalNPC : GlobalNPC
{
    public float DamageFillPercent = 1f;
    public int FadeDelayTimer;
    
    private const float FadeSpeed = 0.15f;
    private const int DelayTicks = 30;
    
    public override bool InstancePerEntity => true;
    
    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        FadeDelayTimer = DelayTicks;
    }
    
    public override bool PreAI(NPC npc)
    {
        float currentHealth = MathHelper.Clamp((float)npc.life / (float)npc.lifeMax, 0f, 1f);
        
        if (DamageFillPercent < currentHealth)
            DamageFillPercent = currentHealth;
        
        if (FadeDelayTimer > 0)
            FadeDelayTimer--;
        else if (DamageFillPercent > currentHealth)
            DamageFillPercent = MathHelper.Lerp(DamageFillPercent, currentHealth, FadeSpeed);
        
        return true;
    }
    
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write(DamageFillPercent);
    }
    
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        DamageFillPercent = binaryReader.ReadSingle();
    }
}

public class FancyHealthBarPlayer : ModPlayer
{
    public float DamageFillPercent = 1f;
    public int FadeDelayTimer;
    
    private const float FadeSpeed = 0.15f;
    private const int DelayTicks = 30;
    
    public override void OnHurt(Player.HurtInfo info)
    {
        FadeDelayTimer = DelayTicks;
    }
    
    public override void PreUpdate()
    {
        float currentHealth = MathHelper.Clamp((float)Player.statLife / Player.statLifeMax2, 0f, 1f);
        
        if (DamageFillPercent < currentHealth)
            DamageFillPercent = currentHealth;
        
        if (FadeDelayTimer > 0)
            FadeDelayTimer--;
        else if (DamageFillPercent > currentHealth)
            DamageFillPercent = MathHelper.Lerp(DamageFillPercent, currentHealth, FadeSpeed);
    }
}