using Godot;
using System;

public partial class AskTutorial : Control
{
    private void OnYesPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/levels/tutorial.tscn");
    }

    private void OnNoPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/levels/game.tscn");
    }
}
