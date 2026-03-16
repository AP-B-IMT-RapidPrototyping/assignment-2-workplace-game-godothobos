using Godot;
using System;

public partial class GameManager : Node
{
    public static float stress = 0;


    private void OnDead()
    {
        GetTree().Quit();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (stress >= 100)
        {
            stress = 100;
            OnDead();
        }

        if (stress <= 0)
            stress = 0;
    }
}
