using Godot;
using System;

public partial class Library : Node3D
{
    [Export] private Timer _stressTimer;
    private double _timePassed;


    private void OnStressTimerTimeout()
    {
        GameManager.Stress++;
    }


    public override void _PhysicsProcess(double delta)
    {
        _timePassed += delta;

        if (_timePassed >= 1.0)
        {
            GameManager._timeAlive++;
            _timePassed -= 1.0;
        }
    }

}
