﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireGrappleHook : MonoBehaviour {

	/* Flag to indicate if currently "casting" the hook.
	 * True on click, reset to false after collision/miss */
	private bool casting;

	// Represents the world location of a click/cast
	private Vector2 endPoint;

	// Time taken for the object to travel from start to endPoint
	public float castDuration = 6.0f;

	// References to player x and y position
	private float playerX; 
	private float playerY;

	// References to hook x and y position
	private float hookX;
	private float hookY;

	// This draws the rope between the grapple hook and the player
	private LineRenderer ropeLineRenderer;

	/* Two vectors: player position (0) and grapple hook position (1). These are used to 
	 * draw the rope between the player and the hook. */
	private Vector3[] lineRendererVectors;

	// Use this for initialization
	void Start () 
	{
		playerY = gameObject.transform.parent.transform.position.y;
		playerX = gameObject.transform.parent.transform.position.x;

		hookX = gameObject.transform.position.x;
		hookY = gameObject.transform.position.y;

		ropeLineRenderer = gameObject.GetComponent<LineRenderer>();

		// Two vectors: player position (0) and grapple hook position (1)
		lineRendererVectors = new Vector3[2];
		lineRendererVectors [0] = new Vector3 (playerX, playerY, 0);
		lineRendererVectors [1] = new Vector3 (hookX, hookY, 0);
	}
	

	void FixedUpdate () 
	{
		// Update the player and hook position variables
		playerY = gameObject.transform.parent.transform.position.y;
		playerX = gameObject.transform.parent.transform.position.x;
		hookX = gameObject.transform.position.x;
		hookY = gameObject.transform.position.y;

		// Update lineRendererVectors with the new player and hook positions
		lineRendererVectors [0].x = playerX;
		lineRendererVectors [0].y = playerY;
		lineRendererVectors [1].x = hookX;
		lineRendererVectors [1].y = hookY;

		// Keep the LineRenderer attached to the Player object
		ropeLineRenderer.SetPositions(lineRendererVectors);

		if (Input.GetMouseButtonDown(0) && !casting) 
		{
			Vector3 playerVector = new Vector3 (1, 0, 0);
			Vector3 mouseClickVector =
				new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);

			float angleFromPlayerToMouseClick = Vector3.Angle (playerVector, mouseClickVector);

			
			Debug.Log ("ANGLE: " + angleFromPlayerToMouseClick);

			casting = true;
			endPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			//Debug.Log (endPoint);
		}

		// Check if the "casting" flag is true and if the object position has not reached endPoint
		if (casting && !Mathf.Approximately (gameObject.transform.position.magnitude, endPoint.magnitude)) {
			
			/* Move the object to the desired position. Vector2.Lerp moves the object from the source location
			 * to the destination location. castDuration is multiplied by the distance to keep constant speed
			 * independent of the distance travelled. */
			gameObject.transform.position = Vector2.Lerp (gameObject.transform.position, endPoint, 
				1 / (castDuration * (Vector2.Distance (gameObject.transform.position, endPoint))));
		} 
		// Else, if the object arrived at endPoint...
		else if (casting && Mathf.Approximately (gameObject.transform.position.magnitude, endPoint.magnitude)) 
		{
			casting = false;
		}

		// If not casting, the hook should be with the player
		if (!casting) 
		{
			ResetToPlayerLocation ();
		}
	}

	// Check for a collision; if it's a GrapplePoint, log for now (later this is where swinging logic will
	// be initiated).
	void OnCollisionEnter2D(Collision2D collision)
	{
		// Don't check for collisions with the Player object
		if (collision.collider.tag == "Player") 
		{
			Physics2D.IgnoreCollision (gameObject.GetComponent<Collider2D>(), collision.collider);
			return;
		}

		casting = false;

		if (collision.collider.tag == "GrapplePoint") 
		{
			Debug.Log ("GrappleHook collided with GrapplePoint: " + collision.collider.name);
			/* Player attaches to the point and starts swinging. Here we'll probably need to keep "casting" as true 
			 * until the player lets go of the rope. Might need to return in this if block to prevent the hook from 
			 * being reset to the Player coordinates (shouldn't happen until they let go of the swinging rope) */
		}
	}

	// Sets the location of the grappling hook to the player location. Use after collision or whiffed hook
	void ResetToPlayerLocation ()
	{
		/* Grappling hook should go back to location of player object. Difference between Player's and
		 * GrappleHook's x and y coordinates should be subtracted from GrappleHook's coordinates to achieve this. */
		float resetX = playerX - hookX;
		float resetY = playerY - hookY;

		// Construct a new Vector3 with the correct offset values to be added to the current GrappleHook coordinates.
		Vector3 reset = new Vector3 (resetX, resetY, 0);
		gameObject.transform.position += reset;
	}
}