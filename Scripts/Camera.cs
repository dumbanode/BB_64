using Godot;
using System;

public class Camera : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    private float camroot_h = 0;
    private float camroot_v = 0;
    private float cam_v_min = -50;
    private float cam_v_max = 50;
    private float h_sensitivity = (float)0.1;
    private float v_sensitivity = (float)0.1;
    private float h_acceleration = (float) 10;
    private float v_acceleration = (float) 10;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // hide the mouse
        Input.MouseMode = Input.MouseModeEnum.Captured;

        // make sure the camera doesn't collide with player
        GetNode<Spatial>("h").GetNode<Spatial>("v").GetNode<ClippedCamera>("Camera").AddException(GetParent());
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion){
            //GD.Print("Mouse Motion at: ", eventMouseMotion.Position);
            camroot_h += -eventMouseMotion.Relative.x * h_sensitivity;
            camroot_v += eventMouseMotion.Relative.y * v_sensitivity;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        MoveCamera(delta);
    }

    private void MoveCamera(float delta)
    {
        var CurrRotationY = GetNode<Spatial>("h").RotationDegrees.y;
        var CurrRotationX = GetNode<Spatial>("h").GetNode<Spatial>("v").RotationDegrees.x;
        camroot_v = Mathf.Clamp(camroot_v, cam_v_min, cam_v_max);

        var camroot_h_lerp = Mathf.Lerp(CurrRotationY, camroot_h, delta * h_acceleration);
        var camroot_v_lerp = Mathf.Lerp(CurrRotationX, camroot_v, delta * v_acceleration);
        GetNode<Spatial>("h").RotationDegrees = new Vector3((float) 0, camroot_h_lerp, (float) 0);
        GetNode<Spatial>("h/v").RotationDegrees = new Vector3(camroot_v_lerp, (float) 0, (float) 0);
    }
}
