using Godot;
using System;

public class CameraController3d : Spatial
{
    private const int MoveMargin = 20;
    private const float RayLength = 1000f;
    private Camera Camera;
    private Plane GroundPlane = new Plane(Vector3.Up, 0);
    private bool CameraMoving = false;
    private bool CameraDragging = false;
    private Vector2 Origin;
    private float ZoomDirection = 0;

    [Export]
    public float Speed = 40f;
    [Export]
    private float MinZoom = 3;
    [Export]
    private float MaxZoom = 20;
    [Export]
    private float ZoomDuration = 0.9f;
    [Export]
    private float ZoomSpeed = 20f;

    public override void _Ready()
    {
        Camera = GetNode<Camera>("Elevation/Camera");
        if (Camera != null)
        {
            Camera.MakeCurrent();
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("pan_camera"))
        {
            Origin = GetViewport().GetMousePosition();
            CameraDragging = true;
        }
        if (inputEvent.IsActionReleased("pan_camera"))
        {
            CameraDragging = false;
        }
        if (inputEvent.IsActionPressed("zoom_in"))
        {
            ZoomDirection = -1;
        }
        if (inputEvent.IsActionPressed("zoom_out"))
        {
            ZoomDirection = 1;
        }
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

    public override void _Process(float delta)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();

        if (CameraDragging)
        {
            DragCamera(mousePos, delta);
        }

        if (!CameraDragging)
        {
            MoveCamera(mousePos, delta);
        }
        ZoomCamera(delta);
    }

    private void DragCamera(Vector2 mousePos, float delta)
    {
        Vector3 newVector = new Vector3(mousePos.x - Origin.x, 0, mousePos.y - Origin.y);
        Vector2 mouseSpeed = GetMouseSpeed();
        Vector3 velo = (GlobalTransform.basis.z * mouseSpeed.y + GlobalTransform.basis.x * mouseSpeed.x) * delta * 2;
        Vector3 velocity = new Vector3(Translation.x * mouseSpeed.x, 0, Translation.y * mouseSpeed.y);
        GlobalTranslate(-velo);
    }

    private void ZoomCamera(float delta)
    {
        Vector3? groundTarget = GetGroundPosition();
        float newZoom = Mathf.Clamp(Camera.Translation.z + ZoomDirection * ZoomSpeed * delta, MinZoom, MaxZoom);
        Vector3 cameraTranslate = new Vector3(0, 0, newZoom);
        Camera.Translation = cameraTranslate;
        if (groundTarget != null)
        {
            RealignCamera((Vector3)groundTarget);
        }
        ZoomDirection *= ZoomDuration;
        if (Mathf.Abs(ZoomDirection) < 0.01f)
        {
            ZoomDirection = 0f;
        }
    }

    public void MoveCamera(Vector2 mousePos, float delta)
    {
        Vector3 inputVector = Vector3.Zero;
        
        if (!CameraMoving)
        {
            Vector2 viewportSize = GetViewport().Size;
            if (mousePos.x < MoveMargin)
            {
                inputVector.x -= 1;
            }
            if (mousePos.y < MoveMargin)
            {
                inputVector.z -= 1;
            }
            if (mousePos.x > viewportSize.x - MoveMargin)
            {
                inputVector.x += 1;
            }
            if (mousePos.y > viewportSize.y - MoveMargin)
            {
                inputVector.z += 1;
            }
        }

        else
        {
            inputVector.x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            inputVector.z = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
        }
        GlobalTranslate(inputVector * Speed * delta);
    }

    private Vector2 GetMouseSpeed()
    {
        Vector2 currentPos = GetViewport().GetMousePosition();
        Vector2 mouseSpeed = currentPos - Origin;
        Origin = currentPos;
        return mouseSpeed;        
    }

    private Vector3? GetGroundPosition()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector3 rayStart = Camera.ProjectRayOrigin(mousePos);
        Vector3 rayEnd = rayStart + Camera.ProjectRayNormal(mousePos) * RayLength;
        return GroundPlane.IntersectRay(rayStart, rayEnd);
    }

    private void RealignCamera(Vector3 point)
    {
        Vector3? newPosition = GetGroundPosition();
        if (newPosition == null)
        {
            return;
        }
        GlobalTranslate(point - (Vector3)newPosition);
    }

}