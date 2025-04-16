using System;
using EnemyHealthBar.Core.Enums;
using EnemyHealthBar.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace EnemyHealthBar.Styles.Base;

public abstract class HealthBar : ModType
{
    public abstract HealthBarStyles Style { get; }
    public abstract DepletionDirection Direction { get; }
    public abstract Asset<Texture2D> BackgroundTexture { get; }
    public abstract Asset<Texture2D> FillTexture { get; }
    public virtual Asset<Texture2D> DamageFillTexture { get; }
    
    public virtual void DrawHealthBar(float x, float y, int health, int maxHealth, float alpha, float scale)
    {
        if (health <= 0)
            return;
        
        int barWidth = BackgroundTexture.Width();
        Vector2 barPosition = new Vector2(x - barWidth / 2f * scale, y);
        
        float currentHealth = MathHelper.Clamp((float)health / (float)maxHealth, 0f, 1f);
        
        int fillWidth = Math.Max(2, (int)(barWidth * currentHealth));
        Vector2 fillPosition = Vector2.Zero;
        Rectangle fillRect = Rectangle.Empty;
        
        if (Direction == DepletionDirection.RightToLeft)
        {
            fillPosition = barPosition;
            
            fillRect = new Rectangle(0, 0, fillWidth, FillTexture.Height());
        }
        else if (Direction == DepletionDirection.Symmetrical)
        {
            int fillOffset = (barWidth - fillWidth) / 2;
            
            fillPosition = new Vector2(barPosition.X + fillOffset * scale, barPosition.Y);
            
            fillRect = new Rectangle(fillOffset, 0, fillWidth, FillTexture.Height());
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
        
        Main.spriteBatch.Draw(BackgroundTexture.Value, drawPosition, new Rectangle(0, 0, barWidth, BackgroundTexture.Height()), Color.White * alpha, 0f, origin, scale, spriteEffect, 0f);
        
        Main.spriteBatch.Draw(FillTexture.Value, fillDrawPosition, fillRect, Color.White * alpha, 0f, origin, scale, spriteEffect, 0f);
    }
    
    protected sealed override void Register()
    {
        ModTypeLookup<HealthBar>.Register(this);

        if (!HealthBarSystem.Styles.Contains(this))
            HealthBarSystem.Styles.Add(this);
    }
}