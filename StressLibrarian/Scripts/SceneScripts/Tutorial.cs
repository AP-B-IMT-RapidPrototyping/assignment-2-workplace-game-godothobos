using Godot;
using System;

public partial class Tutorial : Node3D
{
    [Export] private AnimationPlayer _door1;
    [Export] private AnimationPlayer _door2;
    [Export] private RigidBody3D bookBox;

    private bool firstDoor = false;
    private bool secondDoor = false;


    public override void _PhysicsProcess(double delta)
    {

        if (!firstDoor && bookBox.LinearVelocity.Length() > 10f)
        {
            if (firstDoor)
                return;
            firstDoor = true;
            _door1.Play("Open1");
        }

        if (!secondDoor && GetTree().GetNodesInGroup("interact_pickup").Count == 0)
        {
            if (secondDoor)
                return;

            secondDoor = true;
            _door2.Play("Open2");
        }
    }

}
