using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    Camera cam;

    [Header("Positions")]
    public Vector3 startPositon;
    public Vector3 targetPosition;

    [Header("Player Objects")]
    PlayerMovement player;
    PlayerMovement playerTwo;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        ControlOptions.Initialize();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        playerTwo = GameObject.FindGameObjectWithTag("PlayerTwo").GetComponent<PlayerMovement>();

        startPositon = transform.position;
        targetPosition = transform.position;
    }

    private void OnEnable()
    {
        Actions.levelReset += Respawn;
    }

    private void OnDisable()
    {
        Actions.levelReset -= Respawn;
    }

    private void FixedUpdate()
    {
        float averageX = (player.transform.position.x + playerTwo.transform.position.x) * 0.5f;
        if (player.isAlive && !playerTwo.isAlive) averageX = player.transform.position.x;
        else if (!player.isAlive && playerTwo.isAlive) averageX = playerTwo.transform.position.x;
        targetPosition = new Vector3(averageX, transform.position.y, transform.position.z);
    }

    private Vector3 velocity = Vector3.zero;
    [SerializeField] private float smoothTime = 0.2f;

    private void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    void Respawn()
    {
        targetPosition = startPositon;
        transform.position = targetPosition;
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
