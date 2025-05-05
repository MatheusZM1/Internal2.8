using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Input")]
    public float horizontal;
    public float vertical;

    [Header("Variables")]
    public float speed;
    public float gravity;

    [Header("Physics")]
    public Vector2 velocity;

    [Header("Ground Check")]
    public bool isGrounded;
    public LayerMask groundMask;

    void Start()
    {
        
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        velocity.x = horizontal * speed;

        isGrounded = Physics2D.Raycast(transform.position, -Vector2.up, 0.5f, groundMask);

        if (isGrounded)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y += gravity;
        }
    }
}
