using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBehaviour : MonoBehaviour
{
    [Header("Refs")]
    public PlayerMovement playerScript;
    public Vector2 currentDirectionFacing;

    [Header("Eyes")]
    public Transform leftEye;
    public Transform rightEye;

    [Header("Eye Physics")]
    public float speed = 5f;
    private float threshold = 0.001f;

    [Header("Positions")]
    public Vector2[] facingRightPos;
    public Vector2[] facingUpRightPos;
    public Vector2[] facingDownRightPos;
    public Vector2[] facingUpPos;
    public Vector2[] facingDownPos;

    private void Update()
    {
        if (playerScript.directionFacing.y == 0)
        {
            if (playerScript.directionFacing.x > 0) // Facing right
            {
                MoveEye(leftEye, facingRightPos[0]);
                MoveEye(rightEye, facingRightPos[1]);
            }
            else // Facing left
            {
                MoveEye(leftEye, facingRightPos[1] * new Vector2(-1, 1));
                MoveEye(rightEye, facingRightPos[0] * new Vector2(-1, 1));
            }
        }
        else
        {
            if (playerScript.horizontal == 0)
            {
                if (playerScript.directionFacing.y > 0) // Facing up
                {
                    MoveEye(leftEye, facingUpPos[0]);
                    MoveEye(rightEye, facingUpPos[1]);
                }
                else if (playerScript.directionFacing.y < 0) // Facing down
                {
                    MoveEye(leftEye, facingDownPos[0]);
                    MoveEye(rightEye, facingDownPos[1]);
                }
            }
            else
            {
                if (playerScript.directionFacing.x > 0)
                {
                    if (playerScript.directionFacing.y > 0) // Facing up right
                    {
                        MoveEye(leftEye, facingUpRightPos[0]);
                        MoveEye(rightEye, facingUpRightPos[1]);
                    }
                    else if (playerScript.directionFacing.y < 0) // Facing down right
                    {
                        MoveEye(leftEye, facingDownRightPos[0]);
                        MoveEye(rightEye, facingDownRightPos[1]);
                    }
                }
                else
                {
                    if (playerScript.directionFacing.y > 0) // Facing up left
                    {
                        MoveEye(leftEye, facingUpRightPos[1] * new Vector2(-1, 1));
                        MoveEye(rightEye, facingUpRightPos[0] * new Vector2(-1, 1));
                    }
                    else if (playerScript.directionFacing.y < 0) // Facing down left
                    {
                        MoveEye(leftEye, facingDownRightPos[1] * new Vector2(-1, 1));
                        MoveEye(rightEye, facingDownRightPos[0] * new Vector2(-1, 1));
                    }
                }
            }
        }
    }

    void MoveEye(Transform targetEye, Vector2 targetPos)
    {
        Vector2 currentPos = targetEye.localPosition;

        if (Vector2.Distance(currentPos, targetPos) > threshold)
        {
            // Eased movement using Lerp with Time.deltaTime-based interpolation
            // This creates an ease-out effect
            Vector2 newPos = Vector2.Lerp(currentPos, targetPos, 1f - Mathf.Exp(-speed * Time.deltaTime));

            // Ensure we don't overshoot the target
            if (Vector2.Distance(newPos, targetPos) > Vector2.Distance(currentPos, targetPos))
            {
                newPos = targetPos;
            }

            targetEye.localPosition = newPos;
        }
        else
        {
            targetEye.localPosition = targetPos; // Snap to exact target
        }
    }
}
