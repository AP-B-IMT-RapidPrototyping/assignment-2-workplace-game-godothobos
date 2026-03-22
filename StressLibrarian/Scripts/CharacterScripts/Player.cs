using Godot;
using System;
using System.Net.NetworkInformation;
using System.Text;

public partial class Player : CharacterBody3D
{
    public enum PlayerState
    {
        IDLE,
        WALKING,
        SPRINTING
    }
    bool _isInteracting = false;


    /* CAMERA EXPORTS */
    [Export] private Camera3D _camera;
    [Export] private float _mouseSensitivity = 0.003f;
    [Export] private float _normalFov = 80f;
    [Export] private float _sprintingFov = 90f;
    [Export] private float _changeFovStrength = 10f;
    [Export] private RayCast3D _playerRayCast;
    [Export] private Marker3D _holdPointMarker;


    /* MOVEMENT EXPORTS */
    [Export] private float _speed = 5.0f;
    [Export] private float _sprintSpeed = 7.0f;
    [Export] private float _acceleration = 60.0f;
    [Export] private float _deceleration = 80.0f;

    [Export] private float _holdStrength = 26f;


    /* UI EXPORTS */
    [Export] private Control _playerUI;
    [Export] private TextureRect _grabUI;
    [Export] private TextureRect _interactUI;
    [Export] private Control _stressView;
    [Export] private Label _stressLabel;

    /* AUDIO EXPORTS */
    [Export] private AudioStreamPlayer3D pickupSound;

    /* STATE MACHINE */
    private PlayerState playerState = PlayerState.IDLE;
    private RigidBody3D picked_object;


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

    private void ExtraStress()
    {
        if (playerState == PlayerState.SPRINTING)
            GameManager.Stress++;
    }

    private void HandleInteractionUI()
    {
        _grabUI.Visible = false;
        _interactUI.Visible = false;

        if (picked_object != null)
            return;

        if (_playerRayCast.IsColliding())
        {
            var collider = _playerRayCast.GetCollider() as RigidBody3D;
            if (collider == null || !IsInstanceValid(collider))
                return;

            if (collider.IsInGroup("interact_pickup"))
            {
                _grabUI.Visible = true;
            }
            if (collider.IsInGroup("interact_NPC"))
            {
                _interactUI.Visible = true;
            }
        }
    }

    private void HandleInteraction()
    {
        if (Input.IsActionJustPressed("interact"))
        {
            if (_playerRayCast.IsColliding())
            {
                var collider = _playerRayCast.GetCollider() as Node;

                if (collider != null && collider.IsInGroup("interact_pickup") && collider is RigidBody3D)
                {
                    GD.Print($"INTERACTION: PICKUP: {collider.Name}");
                    picked_object = collider as RigidBody3D;
                    if (_grabUI.Visible == true)
                        pickupSound.Play();
                }
                if (collider != null && collider.IsInGroup("interact_npc"))
                {
                    if (collider is Npc npc)
                    {
                        npc.Interact(this);
                    }
                    GD.Print($"INTERACTION: NPC: {collider.Name}");
                }
            }
        }
    }

    private void HandleDropHeldItem()
    {
        if (Input.IsActionJustPressed("drop"))
        {
            if (picked_object != null)
            {
                picked_object.GravityScale = 1f;
                picked_object = null;
            }
        }

    }

    private void HandlePickedObject()
    {
        if (picked_object == null)
            return;

        Vector3 a = picked_object.GlobalTransform.Origin;
        Vector3 b = _holdPointMarker.GlobalTransform.Origin;
        picked_object.LinearVelocity = (b - a) * _holdStrength;

        Vector3 directionToCamera = (_camera.GlobalTransform.Origin - picked_object.GlobalTransform.Origin).Normalized();
        Vector3 lookAtPosition = picked_object.GlobalTransform.Origin + new Vector3(directionToCamera.X, 0, directionToCamera.Z);
        picked_object.LookAt(lookAtPosition, Vector3.Up);
    }

    public void HandleForceDropObject(RigidBody3D obj)
    {
        if (picked_object == obj)
        {
            picked_object = null;
        }
    }


    private void UpdateStress()
    {
        _stressLabel.Text = $"S: {GameManager.Stress}";

        float stressToModulate = GameManager.Stress / 100;
        _stressView.Modulate = new Color(1f, 1f, 1f, stressToModulate);
    }

    public void SetInteracting(bool value)
    {
        _isInteracting = value;
    }

    /* ------------------------- */
    /* PROCESS && PHYSICS PROCESS*/
    /* ------------------------- */
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

        // Add the gravity
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Get the input direction and handle the movement/deceleration.
        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        /* Manage statemachine */
        if (direction == Vector3.Zero)
        {
            playerState = PlayerState.IDLE;
        }
        else if (Input.IsActionPressed("sprint"))
        {
            playerState = PlayerState.SPRINTING;
        }
        else
        {
            playerState = PlayerState.WALKING;
        }

        /* Use statemachine */
        float currentSpeed = _speed;
        float targetFov = _normalFov;

        if (playerState == PlayerState.SPRINTING)
        {
            currentSpeed = _sprintSpeed;
            targetFov = _sprintingFov;
        }

        Vector3 targetVelocity = direction * currentSpeed;
        if (direction != Vector3.Zero)
        {
            velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, _acceleration * (float)delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, _acceleration * (float)delta);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, _deceleration * (float)delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, _deceleration * (float)delta);
        }
        _camera.Fov = Mathf.Lerp(_camera.Fov, targetFov, _changeFovStrength * (float)delta);

        Velocity = velocity;

        HandleInteractionUI();
        HandleInteraction();
        HandlePickedObject();
        HandleDropHeldItem();

        UpdateStress();

        if (!_isInteracting)
            MoveAndSlide();
    }

}
