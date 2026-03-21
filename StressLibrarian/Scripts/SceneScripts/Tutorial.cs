using Godot;
using System;
using System.Threading.Tasks;

public partial class Tutorial : Node3D
{
    [Export] private AnimationPlayer _door1;
    [Export] private AnimationPlayer _door2;
    [Export] private AnimationPlayer _lightEnding;
    [Export] private AnimationPlayer _doorKnock;
    [Export] private AnimationPlayer _doorOpen;
    [Export] private RigidBody3D _bookBox;


    private bool _firstDoor = false;
    private bool _secondDoor = false;
    private bool _thirdDoor = false;
    private bool _lightsAlreadyPlayed = false;
    private bool _knockAlreadyPlayed = false;
    private bool _doorOpenAlreadyPlayed = false;
    private bool _canNpcCome = false;
    private int _lastPickupCount;


    public override void _Ready()
    {
        _lastPickupCount = GetTree().GetNodesInGroup("interact_pickup").Count;
    }


    private async void PlayLightEnding()
    {
        if (!_thirdDoor || _lightsAlreadyPlayed)
        {
            return;
        }
        GD.Print("LIGHT ENDING");
        _lightsAlreadyPlayed = true;
        _lightEnding.Play("Play");

        await ToSignal(_lightEnding, AnimationPlayer.SignalName.AnimationFinished);
    }

    private void PlayKnocking(Node3D body)
    {
        if (!_thirdDoor || _knockAlreadyPlayed)
            return;

        GD.Print("DOOR KNOCKING");
        if (body is Player)
        {
            _doorKnock.Play("Play");
            _canNpcCome = true;
            _knockAlreadyPlayed = true;
        }
    }

    private void NpcEntrance(Node3D body)
    {
        if (!_canNpcCome || _doorOpenAlreadyPlayed)
            return;
        GD.Print("NPC ENTRANCE");

        if (body is Player)
            _doorOpen.Play("Play");
        _doorOpenAlreadyPlayed = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_firstDoor && _secondDoor && _thirdDoor && _lightsAlreadyPlayed)
        {
            return;
        }

        if (!_firstDoor && _bookBox.LinearVelocity.Length() > 10f)
        {
            if (_firstDoor)
                return;

            _firstDoor = true;
            _door1.Play("Open1");
            GD.Print("D1");
        }

        int currentCount = GetTree().GetNodesInGroup("interact_pickup").Count;
        if (!_secondDoor && currentCount < _lastPickupCount)
        {
            if (_secondDoor)
                return;

            _secondDoor = true;
            _door2.Play("Open2");
            GD.Print("D2");
        }
        _lastPickupCount = currentCount;

        if (!_thirdDoor && GetTree().GetNodesInGroup("interact_pickup").Count <= 1)
        {
            if (_thirdDoor)
                return;

            _thirdDoor = true;
            GD.Print("D3");
        }
    }

}
