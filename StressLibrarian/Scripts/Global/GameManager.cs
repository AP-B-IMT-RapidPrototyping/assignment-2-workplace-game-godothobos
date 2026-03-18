using Godot;
using System;

public partial class GameManager : Node
{
    public static float Stress = 0;
    public static float Difficulty = 0;

    public static int _timeAlive = 0;
    public static int _highScore = 0;
    public static int _npcHelped = 0;

    public static int ActiveLoudNPC = 0;
    public static int ActiveLoudNPCMax = 3;
    public static int ActiveAskNPC = 0;
    public static int ActiveAskNPCMax = 1;

    private bool _isDead = false;


    private void OnDead()
    {
        GetTree().ChangeSceneToFile("res://scenes/levels/death.tscn");
    }

    public static void ResetGame()
    {
        Stress = 0;
        Difficulty = 0;

        ActiveLoudNPC = 0;
        ActiveAskNPC = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isDead && Stress >= 100)
        {
            _isDead = true;
            Stress = 0;
            OnDead();
        }

        if (Stress <= 0)
            Stress = 0;


        if (Difficulty >= 100)
            Difficulty = 100;
    }
}
