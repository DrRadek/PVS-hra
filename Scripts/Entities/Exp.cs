using Godot;
using Godot.Collections;
using System;

public partial class Exp : Area2D, ICollectible
{
    [Export] Array<Texture2D> textures = new();
    [Export] Sprite2D sprite;
    private int xpAmount = 10;

    public override void _Ready()
    {
        Random random = new Random();
        sprite.Texture = textures[(int)random.NextInt64(textures.Count)];
    }

    public void Init(int xpAmount)
    {
        this.xpAmount = xpAmount;
    }

    public void Collect(Node collector)
    {
        if (collector is IXpReceiver xpReceiver)
        {
            xpReceiver.ReceiveXp(xpAmount);
        }

        QueueFree();
    }
}
