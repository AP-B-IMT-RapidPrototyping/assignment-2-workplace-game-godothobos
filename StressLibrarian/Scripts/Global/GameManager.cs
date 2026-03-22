using Godot;
using System;

public partial class GameManager : Node
{
    /* GAME */
    public static float Stress = 0;

    /* STATS */
    public static int _timeAlive = 0;
    public static int _highScore = 0;
    public static int _npcShushed = 0;
    public static int _npcHelped = 0;

    /* NPC MANAGER */
    public static float LoudGlobalCooldown = 0f;
    public static float LoudSpawnDelay = 0f;
    public static int ActiveLoudNPC = 0;
    public static int ActiveLoudNPCMax = 3;
    public static float AskGlobalCooldown = 0f;
    public static int ActiveAskNPC = 0;
    public static int ActiveAskNPCMax = 1;

    /* PLAYER MANAGER */
    private bool _isDead = false;


    [Export] private PackedScene _deathScene = GD.Load<PackedScene>("res://scenes/levels/death.tscn");


    private void OnDead()
    {
        GetTree().ChangeSceneToPacked(_deathScene);
    }

    public static void ResetGame()
    {
        Stress = 0;

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

        if (LoudGlobalCooldown > 0f)
            LoudGlobalCooldown -= (float)delta;
        
        if (LoudSpawnDelay > 0)
            LoudSpawnDelay -= (float)delta;

        if (AskGlobalCooldown > 0f)
            AskGlobalCooldown -= (float)delta;
    }
}
