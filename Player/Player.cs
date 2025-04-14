using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private Node3D head;
    private Camera3D camera;

    private const float GRAVITY = 12f;

    [ExportGroup("Motion Settings")]
    [Export]
    private float walkSpeed = 5.2f;
    [Export]
    private float sprintSpeed = 8.0f;
    [Export]
    private float acceleration = 18.0f;
    [Export]
    private float braking = 12.0f;
    [Export]
    private float AirControl = 0.5f;
    [Export]
    private float jumpVelocity = 4.75f;
    [Export]
    private int maxJump = 1;

    [ExportGroup("Camera Settings")]
    [Export]
    private float sensitivity = 0.003f;
    [Export]
    private float bobFrequency = 2.2f;
    [Export]
    private float bobAmplitude = 0.03f;

    private float bobTime = 0.0f;
    private float speed = 0;
    private int currentJumpCount = 0;

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustReleased("left_click"))
            Input.MouseMode = Input.MouseModeEnum.Captured;

        if (Input.IsActionJustReleased("esc"))
            Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            head.RotateY(-eventMouseMotion.Relative.X * sensitivity);
            camera.RotateX(-eventMouseMotion.Relative.Y * sensitivity);

            float cameraClampX = Mathf.Clamp(camera.Rotation.X, Mathf.DegToRad(-60), Mathf.DegToRad(60));
            camera.Rotation = new Vector3(cameraClampX, 0, 0);

            RotateY(head.Rotation.Y);
            head.Rotation = Vector3.Zero;
        }
    }

    public override void _Ready()
    {
        head = GetNode<Node3D>("Head");
        camera = head.GetNode<Camera3D>("Camera3D");
        AddToGroup("teleportable");

        currentJumpCount = maxJump;
        speed = walkSpeed;
    }

    public override void _PhysicsProcess(double delta)
    {
        HorizontalMovement(delta);
        VerticalMovement(delta);
        HeadBobing(delta);
        MoveAndSlide();
    }

    private void HorizontalMovement(double delta)
    {
        float inputX = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        float inputZ = Input.GetActionStrength("down") - Input.GetActionStrength("up");

        Vector3 velocity = Velocity;
        Vector3 direction = (Transform.Basis * new Vector3(inputX, 0, inputZ)).Normalized();

        float generalBraking = braking;
        float generalAcceleration = acceleration;
        speed = walkSpeed;

        if (IsOnFloor())
            speed += Input.GetActionStrength("sprint") * (sprintSpeed - walkSpeed);

        if (!IsOnFloor())
        {
            generalBraking *= AirControl;
            generalAcceleration *= AirControl;
        }

        if (direction.Length() >= 0.2f)
            velocity = velocity.MoveToward(direction * speed, generalAcceleration * (float)delta);
        else
            velocity = velocity.MoveToward(Vector3.Zero, generalBraking * (float)delta);

        velocity.Y = Velocity.Y;
        Velocity = velocity;
    }

    private void VerticalMovement(double delta)
    {
        Vector3 velocity = Velocity;

        if (IsOnFloor())
            currentJumpCount = maxJump;

        if (Input.IsActionJustPressed("jump") && currentJumpCount > 0)
        {
            velocity.Y = jumpVelocity;
            currentJumpCount--;
        }


        Rotation = Rotation.MoveToward(new Vector3(0,Rotation.Y, 0), 1.6f * (float)delta);
        velocity.Y -= GRAVITY * (float)delta;
        Velocity = velocity;
    }

    private void HeadBobing(double delta)
    {
        if (IsOnFloor())
            bobTime += (float)delta * Velocity.Length();

        Vector3 position = Vector3.Zero;
        position.Y = Mathf.Sin(bobTime * bobFrequency) * bobAmplitude;
        position.X = Mathf.Cos(bobTime * bobFrequency / 2) * bobAmplitude;

        Transform3D transform = camera.Transform;
        transform.Origin = position;
        camera.Transform = transform;
    }
}
