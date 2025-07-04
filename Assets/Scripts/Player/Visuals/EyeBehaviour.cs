using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBehaviour : MonoBehaviour
{
    [Header("Refs")]
    public PlayerMovement playerScript;
    public Vector2 currentDirectionFacing;

    [Header("Eyes")]
    public Transform eyesContainer;
    public Vector2 eyesContainerPos;
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

        eyesContainerPos = MoveEye(eyesContainerPos, new Vector2(0, 0.25f) + new Vector2(xOffset, yOffset) * direction);
        eyesContainer.transform.localPosition = new Vector2(Mathf.Round(eyesContainerPos.x * 32f) / 32f, Mathf.Round(eyesContainerPos.y * 32f) / 32f);
    }

    Vector2 MoveEye(Vector2 referencePos, Vector2 targetPos)
    {
        Vector2 currentPos = referencePos;

        if (Vector2.Distance(currentPos, targetPos) > threshold)
        {
            // This creates an ease-out effect
            Vector2 newPos = Vector2.Lerp(currentPos, targetPos, 1f - Mathf.Exp(-speed * Time.deltaTime));

            // Ensure we don't overshoot the target
            if (Vector2.Distance(newPos, targetPos) > Vector2.Distance(currentPos, targetPos))
            {
                newPos = targetPos;
            }

            return newPos;
        }
        else
        {
            return targetPos; // Snap to exact target
        }
    }
}
