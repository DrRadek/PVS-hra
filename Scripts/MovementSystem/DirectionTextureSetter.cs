using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class DirectionTextureSetter : Node
{
    [Export] Sprite2D spriteToSetTo;
    [Export] MovableObject movableObject;
    [Export] Texture2D[] textures = new Texture2D[9];

    public override void _Ready()
    {
        movableObject.OnCurrentDirChanged += MovableObject_OnCurrentDirChanged;
    }

    private void MovableObject_OnCurrentDirChanged(MovableObject.CurrentDirection currentDirection)
    {
        int index = 0;
        switch (currentDirection)
        {
            case MovableObject.CurrentDirection.Up:
                index = 1;
                break;
            case MovableObject.CurrentDirection.Up | MovableObject.CurrentDirection.Right:
                index = 2;
                break;
            case MovableObject.CurrentDirection.Right:
                index = 3;
                break;
            case MovableObject.CurrentDirection.Right | MovableObject.CurrentDirection.Down:
                index = 4;
                break;
            case MovableObject.CurrentDirection.Down:
                index = 5;
                break;
            case MovableObject.CurrentDirection.Down | MovableObject.CurrentDirection.Left:
                index = 6;
                break;
            case MovableObject.CurrentDirection.Left:
                index = 7;
                break;
            case MovableObject.CurrentDirection.Left | MovableObject.CurrentDirection.Up:
                index = 8;
                break;
        }

        spriteToSetTo.Texture = textures[index];
    }
}
