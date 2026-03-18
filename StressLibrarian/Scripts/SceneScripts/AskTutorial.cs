using Godot;
using System;

public partial class AskTutorial : Control
{
    [Export] AnimationPlayer _animationManager;

    private void OnYesPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/levels/tutorial.tscn");
    }

    private void OnNoPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/levels/Library.tscn");
    }

    private void OnBackPressed()
    {
        _animationManager.PlayBackwards("ViewPlay");
    }
}
