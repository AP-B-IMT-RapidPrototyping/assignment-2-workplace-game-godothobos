using Godot;
using System;

public partial class Library : Node3D
{
    [Export] private Timer _stressTimer;


    private void OnStressTimerTimeout()
    {
        GameManager.Stress++;
    }
}
