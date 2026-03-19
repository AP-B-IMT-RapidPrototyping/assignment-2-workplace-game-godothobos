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

    private bool _firstDoor = false;
    private bool _secondDoor = false;
    private bool _thirdDoor = false;
    private int _lastPickupCount;


    private async void PlayEnding()
    {
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

            _thirdDoor = true;
            PlayEnding(); /* this should play if the player is looking at the door. (check discordw) */
        }
    }

}
