using Godot;
using System;

public partial class HealthPopup : Control
{
    [Export] Label label;

    double popupTime = 0.0;
    double popupLifetime = 1.2;
    float popupSpeed = 20.0f;

    public override void _Ready()
    {
        base._Ready();
    }

    public void Init(float damage, float hpLeft, float maxHp)
    {
        label.Text = $"-{damage:0.##} ({Mathf.Max(hpLeft, 0):0.##}/{maxHp:0.##})";
        popupTime = 0.0f;
    }

    public override void _Process(double delta)
    {
        popupTime += delta;
        if (popupTime > popupLifetime)
        {
            QueueFree();
        }

        Position = new Vector2 (Position.X, Position.Y - (float)delta * popupSpeed);
        var color = label.LabelSettings.FontColor;
        color.A = 1 - (float)(popupTime/popupLifetime);
        label.LabelSettings.FontColor = color;
    }
}
