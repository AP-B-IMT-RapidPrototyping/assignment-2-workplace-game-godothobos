using Godot;
using System;

public partial class AskTutorial : Control
{
    [Export] private AnimationPlayer _animationManager;

    [Export] private PackedScene _tutorialScene;
    [Export] private PackedScene _libraryScene;

    private void OnYesPressed()
    {
        GameManager.ResetGame();
        GetTree().ChangeSceneToPacked(_tutorialScene);
    }

    private void OnNoPressed()
    {
        GameManager.ResetGame();
        GetTree().ChangeSceneToPacked(_libraryScene);
    }

    private void OnBackPressed()
    {
        _animationManager.PlayBackwards("ViewPlay");
    }
}