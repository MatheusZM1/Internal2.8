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
    public float xOffset;
    public float yOffset;
    public Vector2 defaultEyePos;

    private void Update()
    {
        Vector2 direction = playerScript.directionFacing.normalized;
        if (playerScript.horizontal == 0 && playerScript.vertical != 0)
        {
            direction = Vector2.up * playerScript.directionFacing.y;
        }
        MoveEye(leftEye, new Vector2(-defaultEyePos.x, defaultEyePos.y) + new Vector2(xOffset, yOffset) * direction);
        MoveEye(rightEye, defaultEyePos + new Vector2(xOffset, yOffset) * direction);
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
