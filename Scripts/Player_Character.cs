using Godot;
using System;

public class Player_Character : KinematicBody
{
    private Vector3 Direction = Vector3.Forward;
    private Vector3 Velocity = Vector3.Zero;

    private float VerticalVelocity = 0;
    private float Gravity = (float) 20;

    private float MovementSpeed = 0;
    private float WalkSpeed = (float) 1.5;
    private float RunSpeed = (float) 5;
    private float Acceleration = (float) 6;


    public override void _Ready()
    {
        
    }

    public override void _Input(InputEvent @event)
    {

    }

    public override void _Process(float delta)
    {
        MoveCharacter(delta);
    }

    public void MoveCharacter(float delta)
    {
        // get the direction of where the camera is pointing
        var h_rotation = GetNode<Spatial>("Camroot").GetNode<Spatial>("h").GlobalTransform.basis.GetEuler().y;
        GD.Print("h_rot is: ", h_rotation);

        // get the direction of the character
        var x = Input.GetActionStrength("left") - Input.GetActionStrength("right");
        var z = Input.GetActionStrength("forward") - Input.GetActionStrength("backward");
        Direction = new Vector3(x,0,z).Rotated(Vector3.Up, h_rotation).Normalized();

        // Is the player walking or running?
        if (Input.IsActionPressed("forward") || Input.IsActionPressed("backward") || Input.IsActionPressed("left") || Input.IsActionPressed("right")){
            if (Input.IsActionPressed("sprint")){
                MovementSpeed = RunSpeed;
            }
            else {
                MovementSpeed = WalkSpeed;
            }
        }
        else {
            MovementSpeed = 0;
        }

        if (!IsOnFloor()){
            VerticalVelocity += Gravity + delta;
        }
        else {
            VerticalVelocity = 0;
        }

        // apply the velocity
        var DirMoveSpeed = Direction * MovementSpeed;
        var VelX = Mathf.Lerp(Velocity.x, DirMoveSpeed.x, delta * Acceleration);
        var VelY = Mathf.Lerp(Velocity.y, DirMoveSpeed.y, delta * Acceleration);
        var VelZ = Mathf.Lerp(Velocity.z, DirMoveSpeed.z, delta * Acceleration);
        Velocity = new Vector3(VelX, VelY, VelZ);
        MoveAndSlide(Velocity + Vector3.Down * VerticalVelocity, Vector3.Up);

    }
}
