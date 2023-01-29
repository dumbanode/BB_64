using Godot;
using System;

public class Player_Character : KinematicBody
{
    private Vector3 Direction = Vector3.Forward;
    private Vector3 Velocity = Vector3.Zero;
    private Vector3 StrafeDir = Vector3.Zero;
    private Vector3 Strafe = Vector3.Zero;

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
        // if aim is clicked, switch into the aiming state, else go to not_aiming state
        if (Input.IsActionPressed("aim")){
            GetNode<AnimationTree>("AnimationTree").Set("parameters/aim_transition/current", 0);
        }
        else {
            GetNode<AnimationTree>("AnimationTree").Set("parameters/aim_transition/current", 1);
        }
    

        var h_rotation = (float) 0;
        // get the direction of where the camera is pointing
        h_rotation = GetNode<Spatial>("Camroot").GetNode<Spatial>("h").GlobalTransform.basis.GetEuler().y;
        DebugPrint("H Rotation 1: ", h_rotation);
        if (Input.IsActionPressed("forward") || Input.IsActionPressed("backward") || Input.IsActionPressed("left") || Input.IsActionPressed("right")){

            // get the direction of the character
            var x = Input.GetActionStrength("left") - Input.GetActionStrength("right");
            var z = Input.GetActionStrength("forward") - Input.GetActionStrength("backward");
            //Direction = new Vector3(x,0,z).Rotated(Vector3.Up, h_rotation).Normalized();
            Direction = new Vector3(x,0,z).Rotated(Vector3.Up, h_rotation);

            StrafeDir = Direction;

            Direction = Direction.Normalized();

            // Is the player walking or running?
            // If the sprint button is pressed AND if the current state is not_aiming
            // (We do not want to run if we're aiming)
            if (Input.IsActionPressed("sprint") && (int) GetNode<AnimationTree>("AnimationTree").Get("parameters/aim_transition/current") == 1){
                MovementSpeed = RunSpeed;
            }
            else {
                MovementSpeed = WalkSpeed;
            }
        }
        else {
            MovementSpeed = 0;
            StrafeDir = Vector3.Zero;
        }

        // apply vertical velocity if not on floor
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

        // if we are NOT aiming
        if ((int)GetNode<AnimationTree>("AnimationTree").Get("parameters/aim_transition/current") == 1){
            // aim the player towards what keys are pressed
            var meshRotation = Mathf.LerpAngle(GetNode<Spatial>("Mesh").Rotation.y, Mathf.Atan2(Direction.x, Direction.z), delta * AngularAcceleration);
            GetNode<Spatial>("Mesh").Rotation = new Vector3(0, meshRotation, 0);
        }
        // if we are aiming
        else {
            // aim the player towards where the camera is pointing
            var meshRotation = Mathf.LerpAngle(GetNode<Spatial>("Mesh").Rotation.y, h_rotation, delta * AngularAcceleration);
            DebugPrint("Mesh Rotation: ", meshRotation);
            DebugPrint("H Rotation 2: ", h_rotation);
            GetNode<Spatial>("Mesh").Rotation = new Vector3(0, meshRotation, 0);
        }

        // Lerp the strafe and update the strafe animation
        var strafeX = Mathf.Lerp(Strafe.x, StrafeDir.x, delta * Acceleration);
        var strafeY = Mathf.Lerp(Strafe.y, StrafeDir.y, delta * Acceleration);
        var strafeZ = Mathf.Lerp(Strafe.z, StrafeDir.z, delta * Acceleration);
        Strafe = new Vector3(strafeX, strafeY, strafeZ);

        GetNode<AnimationTree>("AnimationTree").Set("parameters/Strafe/blend_position", new Vector2(-Strafe.x, Strafe.z));

        var currBlend = GetNode<AnimationTree>("AnimationTree").Get("parameters/Strafe/blend_position");

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
