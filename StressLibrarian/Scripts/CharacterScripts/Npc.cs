using Godot;
using System;
using System.Data;

public partial class Npc : CharacterBody3D
{
    public enum NPCState
    {
        IDLE,
        WANDER,
        LOUD,
        SNIFFPLAYER,
        ASKPLAYER
    }


    [Export] private Node3D _models;

    private NPCState _state;
    private CharacterBody3D _player;
    private float _actionCooldown;
    [Export] private float _baseCooldown = 5f;
    [Export] private float _minCooldown = 5f;



    public override void _Ready()
    {
        GetRandomModel();

        _state = NPCState.IDLE;
        _player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;

        ResetCooldown();
    }

    private void GetRandomModel()
    {
        int count = _models.GetChildCount();
        int chosen = GD.RandRange(0, count);

        for (int i = 0; i < count; i++)
        {
            Node3D model = _models.GetChild<Node3D>(i);
            model.Visible = i == chosen;

            if (i != chosen)
                model.QueueFree();
        }
    }

    private void ResetCooldown()
    {
        _actionCooldown = Mathf.Max(_minCooldown, _baseCooldown);
    }


    private void ChooseNextAction()
    {
        _state = (NPCState)GD.RandRange(0, 4);
        ResetCooldown();
        GD.Print($"{Name} is now {_state}.");
    }

    private void DoMoveToTarget(double delta)
    {
        GD.Print($"{Name} is moving.");
    }

    private void DoNoise(double delta)
    {
        GD.Print($"{Name} is loud.");

    }
    private void DoSniffPlayer(double delta)
    {
        GD.Print($"{Name} is sniffing.");

    }
    private void DoAskPlayer(double delta)
    {
        GD.Print($"{Name} is asking.");

    }



    public override void _PhysicsProcess(double delta)
    {
        _actionCooldown -= (float)delta;
        if (_actionCooldown <= 0)
        {
            ChooseNextAction();
        }

        switch (_state)
        {
            case NPCState.IDLE:

                break;

            case NPCState.WANDER:
                DoMoveToTarget(delta);
                break;
            case NPCState.LOUD:
                DoNoise(delta);
                break;
            case NPCState.SNIFFPLAYER:
                DoSniffPlayer(delta);
                break;
            case NPCState.ASKPLAYER:
                DoAskPlayer(delta);
                break;

        }
    }

}
