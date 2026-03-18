using Godot;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

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


    [Export] private NavigationAgent3D _navAgent;
    [Export] private NavigationRegion3D _navRegion;
    [Export] private Node3D _models;
    [Export] private RayCast3D _raycastNpc;
    [Export] private float _sniffMinRadius = 3f;
    [Export] private float _sniffMaxRadius = 6f;
    [Export] private float _askStopDistance = 1.5f;
    private float _askCooldown = 0f;
    private bool _isFollowingPlayer = false;

    private NPCState _state;
    private CharacterBody3D _player;
    private float _actionCooldown;
    [Export] private float _baseCooldown = 5f;
    [Export] private float _minCooldown = 5f;

    private Vector3 _wanderTarget;

    private float _moveSpeed = 6f;
    private float _sprintSpeed = 9f;



    public override void _Ready()
    {
        GetRandomModel();

        _state = NPCState.IDLE;
        _player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;

        ResetCooldown();
    }

    private void MoveNPC(double delta)
    {
        if (_navAgent.IsNavigationFinished())
        {
            Velocity = Vector3.Zero;
            return;
        }

        Vector3 next = _navAgent.GetNextPathPosition();
        Vector3 direction = (next - GlobalPosition).Normalized();

        if (direction.Length() > 0.01f)
            Rotation = new Vector3(0, (float)Math.Atan2(direction.X, direction.Z), 0);

        Velocity = direction * _moveSpeed;
        MoveAndSlide();
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
        float min = 4f;
        float max = 9f;
        _actionCooldown = Mathf.Lerp(min, max, GD.Randf());
    }

    private void ChooseNextAction()
    {
        NPCState newState;

        do
        {
            newState = (NPCState)GD.RandRange(0, 4);

            if (newState == NPCState.LOUD && GameManager.ActiveLoudNPC >= GameManager.ActiveLoudNPCMax)
                continue;

            if (newState == NPCState.ASKPLAYER && (GameManager.ActiveAskNPC >= GameManager.ActiveAskNPCMax || _askCooldown >= 0))
                continue;

            break;

        } while (true);

        SetState(newState);
    }

    private void SetState(NPCState newState)
    {
        /* exit old states */
        if (_state == NPCState.LOUD)
            GameManager.ActiveLoudNPC--;

        if (_state == NPCState.ASKPLAYER)
            GameManager.ActiveAskNPC--;

        /* enter new state */
        if (newState == NPCState.LOUD)
            GameManager.ActiveLoudNPC++;

        if (newState == NPCState.ASKPLAYER)
            GameManager.ActiveAskNPC++;

        _state = newState;
        ResetCooldown();

        GD.Print($"{Name} is now {_state}.");
    }

    private Vector3 GetRandomNavLocation()
    {
        if (_navRegion == null || _navRegion.NavigationMesh == null)
            return GlobalPosition;

        var vertices = _navRegion.NavigationMesh.GetVertices();

        if (vertices.Length == 0)
            return GlobalPosition;

        int index = GD.RandRange(0, vertices.Length - 1);
        return vertices[index];
    }

    private Vector3 GetRandomPointAroundPlayer()
    {
        if (_player == null)
            return GlobalPosition;

        float angle = GD.Randf() * Mathf.Tau;
        float distance = Mathf.Lerp(_sniffMinRadius, _sniffMaxRadius, GD.Randf());
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
        return _player.GlobalPosition + offset;
    }

    private void AskOnReachPlayer()
    {
        GD.Print($"{Name} has reached player.");
        _isFollowingPlayer = true;
    }

    private void AskFollowPlayerUntilReached(double delta)
    {
        /* follow player until destination is reached */
        if (_player == null)
            return;

        _navAgent.TargetPosition = _player.GlobalPosition;

        MoveNPC(delta);

        if (true)
        {
            GD.Print($"{Name} task complete.");
            _isFollowingPlayer = false;
            _askCooldown = 10f;
            SetState(NPCState.IDLE);
        }
    }

    private void DoMoveToTarget(double delta) /* long live AI */
    {
        GD.Print($"{Name} is moving.");
        if (_wanderTarget == Vector3.Zero || GlobalPosition.DistanceTo(_wanderTarget) < 0.5f)
        {
            _wanderTarget = GetRandomNavLocation();
            _navAgent.TargetPosition = _wanderTarget;
        }

        MoveNPC(delta);

        if (_navAgent.IsNavigationFinished())
        {
            _state = NPCState.IDLE;
        }
    }

    private void DoNoise(double delta)
    {
        GD.Print($"{Name} is loud.");

        GD.Print($"{Name} has been silenced.");
        SetState(NPCState.IDLE);
    }

    private void DoSniffPlayer(double delta)
    {
        GD.Print($"{Name} is sniffing.");

        if (_player == null)
            return;

        if (_navAgent.IsNavigationFinished() || _navAgent.TargetPosition == Vector3.Zero)
            _navAgent.TargetPosition = GetRandomPointAroundPlayer();

        MoveNPC(delta);

        if (_navAgent.IsNavigationFinished())
        {
            SetState(NPCState.IDLE);
        }
    }

    private void DoMoveToPlayer(double delta)
    {
        GD.Print($"{Name} is asking.");

        if (_player == null)
            return;

        _navAgent.TargetPosition = _player.GlobalPosition;

        MoveNPC(delta);

        if (GlobalPosition.DistanceTo(_player.GlobalPosition) <= _askStopDistance)
        {
            AskOnReachPlayer();
        }
    }



    public override void _PhysicsProcess(double delta)
    {
        _actionCooldown -= (float)delta;
        if (_askCooldown > 0f)
            _askCooldown -= (float)delta;

        if ((_state == NPCState.IDLE || _state == NPCState.WANDER) && _actionCooldown <= 0)
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
                if (_isFollowingPlayer)
                    AskFollowPlayerUntilReached(delta);
                else
                    DoMoveToPlayer(delta);
                break;
        }
    }
}
