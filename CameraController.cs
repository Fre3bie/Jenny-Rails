using Godot;
using System;

public class CameraController : Node2D
{
    private Camera2D Camera;
    [Export]
    public float Speed = 400f;
    private bool CameraMoving = false;
   
   
    public override void _Ready()
    {
        Camera = GetNode<Camera2D>("GameplayCamera");
        if (Camera != null)
        {
            Camera.MakeCurrent();
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("move_left") | inputEvent.IsActionPressed("move_right") |
            inputEvent.IsActionPressed("move_up") | inputEvent.IsActionPressed("move_down"))
        {
            CameraMoving = true;
        }
        else if (Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left") == 0 &&
                 Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up") == 0)
        {
            CameraMoving = false;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (CameraMoving)
        {
            MoveCamera(delta);
        }
    }

    public void MoveCamera(float delta)
    {
        Vector2 inputVector = Vector2.Zero;

        inputVector.x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        inputVector.y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");

        GlobalPosition += inputVector * Speed * delta;
    }
}