using Godot;

public class MouseRotator
{
    private Node2D flipTarget;

    public MouseRotator(Node2D flipTarget)
    {
        this.flipTarget = flipTarget;
    }

    public float calculateRotation()
    {
        Vector2 mousePos = flipTarget.GetGlobalMousePosition();
        Vector2 globalPos = flipTarget.GlobalPosition;

        return (mousePos - globalPos).Angle();
    }
}