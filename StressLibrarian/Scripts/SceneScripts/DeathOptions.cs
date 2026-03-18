using Godot;
using System;

public partial class DeathOptions : Control
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }


    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/levels/main_menu.tscn");
    }
}
