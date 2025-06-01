using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        ControlOptions.Initialize();
    }

    public bool IsAboveCamera(float yPos)
    {
        return yPos > transform.position.y + 4.21875f;
    }

    public bool IsBelowCamera(float yPos)
    {
        return yPos < transform.position.y - 4.21875f;
    }

    public bool IsRightOfCamera(float xPos)
    {
        return xPos > transform.position.x + 7.5f;
    }

    public bool IsLeftOfCamera(float xPos)
    {
        return xPos < transform.position.x - 7.5f;
    }
}
