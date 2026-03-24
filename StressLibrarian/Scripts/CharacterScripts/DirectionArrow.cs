using Godot;
using System;

public partial class DirectionArrow : Node3D
{
    public Node3D Target;
    [Export] private AnimationPlayer _fadeAnimation;

    public override void _Process(double delta)
    {
        if (Target == null)
        {
            _fadeAnimation.Stop();
            Visible = false;
            return;
        }

        Visible = true;

        _fadeAnimation.Play("fade");


        Vector3 from = GlobalTransform.Origin;
        Vector3 to = Target.GlobalTransform.Origin;

        to.Y = from.Y;

        LookAt(to, Vector3.Up);
    }
}
