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

    private float AimTurn = (float) 0;

    private int DebugPrintLoop = 0;


    public override void _Ready()
    {
        
    }

    public override void _Input(InputEvent @event)
    {
        // gradually turn when the player moves the mouse
        if (@event is InputEventMouseMotion eventMouseMotion){
            AimTurn = -eventMouseMotion.Relative.x * (float) 0.015;
        }
    }

    public override void _Process(float delta)
    {
        DebugPrintLoop++;
        if (DebugPrintLoop == 101){
            DebugPrintLoop = 0;
        }

        // Set the state we are currently in
        // Eg: Aiming / Not Aiming
        SetState();
        MoveCharacter(delta);
    }

    public void SetState(){
        // if aim is clicked, switch into the aiming state, else go to not_aiming state
        if (Input.IsActionPressed("aim")){
            GetNode<AnimationTree>("AnimationTree").Set("parameters/aim_transition/current", 0);
        }
        else {
            GetNode<AnimationTree>("AnimationTree").Set("parameters/aim_transition/current", 1);
        }
    }

    public void MoveCharacter(float delta)
    {
    
        // get the direction of where the camera is pointing
        var hRotation = GetNode<Spatial>("Camroot/h").GlobalTransform.basis.GetEuler().y;
        
        // If the movement keys have been pressed, move the character accordingly
        if (Input.IsActionPressed("forward") || Input.IsActionPressed("backward") || Input.IsActionPressed("left") || Input.IsActionPressed("right")){

            // update the direction of where the character should be facing
            var x = Input.GetActionStrength("left") - Input.GetActionStrength("right");
            var z = Input.GetActionStrength("forward") - Input.GetActionStrength("backward");
            // rotate the direction based on where the camera is pointing
            Direction = new Vector3(x,0,z).Rotated(Vector3.Up, hRotation);

            // what direction should the strafe go?
            StrafeDir = Direction;

            // scale the direction into a unit vector
            Direction = Direction.Normalized();

            // Is the player walking or running?
            // If the sprint button is pressed AND if the current state is not_aiming
            // (We do not want to run if we're aiming)
            if (Input.IsActionPressed("sprint") && (int) GetNode<AnimationTree>("AnimationTree").Get("parameters/aim_transition/current") == 1){
                // set the movement speed accordingly
                MovementSpeed = RunSpeed;
                // Smoothly transition from the player's current animation to the walk run animation
                // 1 - Run, 0 - Walk, -1 - Idle
                GetNode<AnimationTree>("AnimationTree").Set("parameters/IWRBlend/blend_amount", Mathf.Lerp((float)GetNode<AnimationTree>("AnimationTree").Get("parameters/IWRBlend/blend_amount"), 1, delta * Acceleration));
            }
            else {
                // Set the movement speed to walking speed and smoothly transition the current animation to the walk animation 
                MovementSpeed = WalkSpeed;
                GetNode<AnimationTree>("AnimationTree").Set("parameters/IWRBlend/blend_amount", Mathf.Lerp((float)GetNode<AnimationTree>("AnimationTree").Get("parameters/IWRBlend/blend_amount"), 0, delta * Acceleration));
            }
        }
        else {
            // Set the movement speed to zero and smoothly transition to the idle animation
            MovementSpeed = 0;
            StrafeDir = Vector3.Zero;
            GetNode<AnimationTree>("AnimationTree").Set("parameters/IWRBlend/blend_amount", Mathf.Lerp((float)GetNode<AnimationTree>("AnimationTree").Get("parameters/IWRBlend/blend_amount"), -1, delta * Acceleration));

            // If we are currently aiming, update the the direction vector to where the camera is pointing
            if ((int) GetNode<AnimationTree>("AnimationTree").Get("parameters/aim_transition/current") == 0){
                Direction = GetNode<Spatial>("Camroot/h").GlobalTransform.basis.z;
            }
        }

        // apply vertical velocity if not on floor
        if (!IsOnFloor()){
            VerticalVelocity += Gravity + delta;
        }
        else {
            VerticalVelocity = 0;
        }

        // calculate the velocity
        var DirMoveSpeed = Direction * MovementSpeed;
        var VelX = Mathf.Lerp(Velocity.x, DirMoveSpeed.x, delta * Acceleration);
        var VelY = Mathf.Lerp(Velocity.y, DirMoveSpeed.y, delta * Acceleration);
        var VelZ = Mathf.Lerp(Velocity.z, DirMoveSpeed.z, delta * Acceleration);
        Velocity = new Vector3(VelX, VelY, VelZ);


        // if we are NOT aiming, aim the player mesh towards where the last keys were pushed
        if ((int)GetNode<AnimationTree>("AnimationTree").Get("parameters/aim_transition/current") == 1){
            var meshRotation = Mathf.LerpAngle(GetNode<Spatial>("Mesh").Rotation.y, Mathf.Atan2(Direction.x, Direction.z), delta * AngularAcceleration);
            GetNode<Spatial>("Mesh").Rotation = new Vector3(0, meshRotation, 0);
        }
        // if we are aiming, aim the player towards where the camera is pointing
        else {
            var meshRotation = Mathf.LerpAngle(GetNode<Spatial>("Mesh").Rotation.y, hRotation, delta * AngularAcceleration);
            GetNode<Spatial>("Mesh").Rotation = new Vector3(0, meshRotation, 0);
        }


        // Lerp the strafe and update the strafe animation
        var strafeX = Mathf.Lerp(Strafe.x, StrafeDir.x + 1 * AimTurn, delta * Acceleration);
        var strafeY = Mathf.Lerp(Strafe.y, StrafeDir.y, delta * Acceleration);
        var strafeZ = Mathf.Lerp(Strafe.z, StrafeDir.z, delta * Acceleration);
        Strafe = new Vector3(strafeX, strafeY, strafeZ);

        // Update where on the blend position graph the animation will sit
        GetNode<AnimationTree>("AnimationTree").Set("parameters/Strafe/blend_position", new Vector2(-Strafe.x, Strafe.z));

        // Move and Slide that bad boy
        MoveAndSlide(Velocity + Vector3.Down * VerticalVelocity, Vector3.Up);

        // Reset the aim turn
        AimTurn = 0;

    }

    private void DebugPrint(string Message, object VariableToPrint)
    {
        if (DebugPrintLoop == 100){
            GD.Print(Message, VariableToPrint);
        }
    }
}
