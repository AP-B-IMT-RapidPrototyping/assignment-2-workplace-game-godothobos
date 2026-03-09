using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] private float _mouseSensitivity = 0.003f;
    [Export] private Camera3D _camera;
    
    [Export] private float _speed = 5.0f;
    [Export] private float _jumpVelocity = 4.5f;


    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * _mouseSensitivity);
            
            _camera.RotateX(-mouseMotion.Relative.Y * _mouseSensitivity);

            Vector3 cameraRotation = _camera.Rotation;
            cameraRotation.X = Math.Clamp(cameraRotation.X, -1.5f, 1.5f);
            _camera.Rotation = cameraRotation;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("escape"))
        {
            GetTree().Quit();
        }
    }



    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = _jumpVelocity;
        }

        // Get the input direction and handle the movement/deceleration.
        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * _speed;
            velocity.Z = direction.Z * _speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

}
