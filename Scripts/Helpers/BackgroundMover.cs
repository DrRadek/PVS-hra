using Godot;
using System;

public partial class BackgroundMover : Node
{
    [Export] Control backgroundNode;

    Node2D player;

    public void SetPlayer(Node2D player)
    {
        this.player = player;
    }

    public override void _Process(double delta)
    {
        var offset = new Vector2(0.5f, 0.5f) + (player.Position * -0.00005f);
        //backgroundNode.Position = (player.GlobalPosition * -0.0001f);

        //offset = new Vector2(0.5f, 0.5f);
        backgroundNode.AnchorTop = offset.Y;
        backgroundNode.AnchorBottom = offset.Y;
        backgroundNode.AnchorLeft = offset.X;
        backgroundNode.AnchorRight = offset.X;
    }
}
