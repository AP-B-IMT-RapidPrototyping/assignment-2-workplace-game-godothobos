using Godot;
using System;

public partial class GameManager : Node
{
    public static float Stress = 0;
    public static float Difficulty = 0;

    public static int _boxHighScore = 0;

    public static int ActiveLoudNPC = 0;
    public static int ActiveLoudNPCMax = 3;
    public static int ActiveAskNPC = 0;
    public static int ActiveAskNPCMax = 1;


    private void OnDead()
    {
        GetTree().Quit();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Stress >= 100)
        {
            Stress = 100;
            OnDead();
        }

        if (Stress <= 0)
            Stress = 0;


        if (Difficulty >= 100)
            Difficulty = 100;
    }
}
