using Godot;
using System;

public partial class Exp : Area2D, ICollectible
{
    private int xpAmount = 10;

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
