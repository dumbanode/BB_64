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
    private float AngularAcceleration = (float) 7;

    private int DebugPrintLoop = 0;


    public override void _Ready()
    {
        
    }

    public override void _Input(InputEvent @event)
    {

    }

    public override void _Process(float delta)
    {
        DebugPrintLoop++;
        if (DebugPrintLoop == 101){
            DebugPrintLoop = 0;
        }

        MoveCharacter(delta);
    }

    public void MoveCharacter(float delta)
    {
    
        var h_rotation = (float) 0;
        if (Input.IsActionPressed("forward") || Input.IsActionPressed("backward") || Input.IsActionPressed("left") || Input.IsActionPressed("right")){
            // get the direction of where the camera is pointing
            h_rotation = GetNode<Spatial>("Camroot").GetNode<Spatial>("h").GlobalTransform.basis.GetEuler().y;

            // get the direction of the character
            var x = Input.GetActionStrength("left") - Input.GetActionStrength("right");
            var z = Input.GetActionStrength("forward") - Input.GetActionStrength("backward");
            Direction = new Vector3(x,0,z).Rotated(Vector3.Up, h_rotation).Normalized();

            // Is the player walking or running?
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

        // make the player face towards where the camera is pointing
        var meshRotation = Mathf.LerpAngle(GetNode<Spatial>("Mesh").Rotation.y, Mathf.Atan2(Direction.x, Direction.z), delta * AngularAcceleration);
        GetNode<Spatial>("Mesh").Rotation = new Vector3(0, meshRotation, 0);
        DebugPrint("H Rotation is: ", h_rotation);
        DebugPrint("MeshRotation is: ", meshRotation);
        //GD.Print("MeshRotation is: ", meshRotation);

        // Move and Slide that bad boy
        MoveAndSlide(Velocity + Vector3.Down * VerticalVelocity, Vector3.Up);

    }

    private void DebugPrint(string Message, object VariableToPrint)
    {
        if (DebugPrintLoop == 100){
            GD.Print(Message, VariableToPrint);
        }
    }
}
