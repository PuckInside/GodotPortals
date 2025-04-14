using Godot;
using System;
using System.Collections.Generic;

public partial class Portal : Node3D
{
	[Export]
	private Portal exitPortal = null;

	public List<Node3D> teleportedObjectsPool = new List<Node3D>();

    public override void _Ready()
	{
		if (exitPortal == null)
			GD.PushError("Linked portal is missing!");

		if (exitPortal == this)
			GD.PushWarning("The linked portal is the same portal!");
    }

	private void _OnArea3D_BodyEntered(Node3D body)
	{
        if (exitPortal == null)
			return;

		if (!body.IsInGroup("teleportable"))
			return;

		if (teleportedObjectsPool.Contains(body))
			return;

		exitPortal.teleportedObjectsPool.Add(body);

		Vector3 exitPosition = exitPortal.Position;
		Vector3 exitRotation = exitPortal.Rotation;      
		exitRotation.Y += Mathf.Pi;

        body.Position = exitPosition;
        body.Rotation = exitRotation;

        CharacterBody3D character = body as CharacterBody3D;
        if (character != null)
        {
            Vector3 forward = exitPortal.GlobalTransform.Basis.Z.Normalized();
            character.Velocity = forward * character.Velocity.Length();
        }
    }

	private void _OnArea3D_BodyExited(Node3D body)
	{
        if (exitPortal == null)
            return;

        if (exitPortal.teleportedObjectsPool.Contains(body))
            exitPortal.teleportedObjectsPool.Remove(body);
    }
}
