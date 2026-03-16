using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class MenuOptions : Node3D
{
    [Export] private MeshInstance3D _buttonStart;
    private Color _startDefaultColor;
    [Export] private MeshInstance3D _buttonSettings;
    private Color _settingsDefaultColor;
    [Export] private MeshInstance3D _buttonQuit;
    private Color _quitDefaultColor;
    [Export] private MeshInstance3D _buttonBack;
    private Color _backDefaultColor;

    private Color color;

    [Export] private AnimationPlayer _animationManager;
    [Export] private AudioStreamPlayer _hoverUI;
    [Export] private AudioStreamPlayer _clickUI;


    public override void _Ready()
    {
        color = new Color(0.7f, 0.7f, 0.7f);

        // Start button
        var startMaterial = _buttonStart.GetActiveMaterial(0);
        if (startMaterial is StandardMaterial3D startMat)
        {
            var uniqueMaterial = (StandardMaterial3D)startMat.Duplicate();
            _buttonStart.SetSurfaceOverrideMaterial(0, uniqueMaterial);
            _startDefaultColor = uniqueMaterial.AlbedoColor;
        }

        // Settings button
        var settingsMaterial = _buttonSettings.GetActiveMaterial(0);
        if (settingsMaterial is StandardMaterial3D settingsMat)
        {
            var uniqueMaterial = (StandardMaterial3D)settingsMat.Duplicate();
            _buttonSettings.SetSurfaceOverrideMaterial(0, uniqueMaterial);
            _settingsDefaultColor = uniqueMaterial.AlbedoColor;
        }

        // Quit button
        var quitMaterial = _buttonQuit.GetActiveMaterial(0);
        if (quitMaterial is StandardMaterial3D quitMat)
        {
            var uniqueMaterial = (StandardMaterial3D)quitMat.Duplicate();
            _buttonQuit.SetSurfaceOverrideMaterial(0, uniqueMaterial);
            _quitDefaultColor = uniqueMaterial.AlbedoColor;
        }

        // Back button
        var backMaterial = _buttonBack.GetActiveMaterial(0);
        if (backMaterial is StandardMaterial3D backMat)
        {
            var uniqueMaterial = (StandardMaterial3D)backMat.Duplicate();
            _buttonBack.SetSurfaceOverrideMaterial(0, uniqueMaterial);
            _backDefaultColor = uniqueMaterial.AlbedoColor;
        }
    }


    /* ------------------------------------------ */
    /* Press Start */
    private void OnStartPressed(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            _clickUI.Play();
            _animationManager.Play("ViewStartGame");
            GetTree().ChangeSceneToFile("res://scenes/levels/game.tscn");
        }
    }

    /* Hover Enter Start */
    private void OnStartEntered()
    {
        _hoverUI.Play();
        var material = _buttonStart.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
            material.AlbedoColor = new Color(color);
    }

    /* Hover Exit Start */
    private void OnStartExited()
    {
        var material = _buttonStart.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
            material.AlbedoColor = _startDefaultColor;
    }


    /* ------------------------------------------ */
    /* Press Settings */
    private void OnSettingsPressed(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            _clickUI.Play();
            _animationManager.Play("ViewSettings");
        }
    }

    /* Hover Enter Settings */
    private void OnSettingsEntered()
    {
        _hoverUI.Play();
        var material = _buttonSettings.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
            material.AlbedoColor = new Color(color);
    }

    /* Hover Exit Settings */
    private void OnSettingsExited()
    {
        var material = _buttonSettings.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
            material.AlbedoColor = _settingsDefaultColor;
    }


    /* ------------------------------------------ */
    /* Press Quit */
    private void OnQuitPressed(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            _clickUI.Play();
            GetTree().Quit();
        }
    }

    /* Hover Enter Quit */
    private void OnQuitEntered()
    {
        _hoverUI.Play();
        var material = _buttonQuit.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
            material.AlbedoColor = new Color(color);
    }

    /* Hover Exit Quit */
    private void OnQuitExited()
    {
        var material = _buttonQuit.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
            material.AlbedoColor = _quitDefaultColor;
    }


    /* ------------------------------------------ */
    /* Press Back */
    private void OnBackPressed(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            _animationManager.PlayBackwards("ViewSettings");
        }
    }

    /* Hover Enter Back */
    private void OnBackEntered()
    {
        _hoverUI.Play();
        var material = _buttonBack.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
            material.AlbedoColor = new Color(color);
    }

    /* Hover Exit Back */
    private void OnBackExited()
    {
        var material = _buttonBack.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
            material.AlbedoColor = _backDefaultColor;
    }
}