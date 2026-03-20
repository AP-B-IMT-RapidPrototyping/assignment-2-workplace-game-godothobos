using Godot;
using System;
using System.Threading.Tasks;

public partial class Tutorial : Node3D
{
    [Export] private AnimationPlayer _door1;
    [Export] private AnimationPlayer _door2;
    [Export] private AnimationPlayer _lightEnding;
    [Export] private AnimationPlayer _doorEnding;
    [Export] private RigidBody3D _bookBox;
    [Export] private AudioStreamPlayer3D _bellRing;

    private bool _firstDoor = false;
    private bool _secondDoor = false;
    private bool _thirdDoor = false;
    private bool _endingAlreadyPlayed = false;
    private int _lastPickupCount;


    private async void PlayEnding()
    {
        if (!_thirdDoor || _endingAlreadyPlayed)
        {
            return;
        }
        _endingAlreadyPlayed = true;
        _bellRing.Playing = false;
        _lightEnding.Play("Play");

        await ToSignal(_lightEnding, AnimationPlayer.SignalName.AnimationFinished);
        _doorEnding.Play("Play");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_firstDoor && _secondDoor && _thirdDoor)
        {
            return;
        }

        if (!_firstDoor && _bookBox.LinearVelocity.Length() > 10f)
        {
            if (_firstDoor)
                return;

            _firstDoor = true;
            _door1.Play("Open1");
        }

        int currentCount = GetTree().GetNodesInGroup("interact_pickup").Count;
        if (!_secondDoor && currentCount < _lastPickupCount)
        {
            if (_secondDoor)
                return;

            _secondDoor = true;
            _door2.Play("Open2");
        }
        _lastPickupCount = currentCount;

        if (!_thirdDoor && GetTree().GetNodesInGroup("interact_pickup").Count == 1)
        {
            if (_thirdDoor)
                return;
            _bellRing.Playing = true;
            _thirdDoor = true;
        }
    }

}
