using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleBomb : MonoBehaviour
{
    BoxCollider2D bc;

    [Header("Speeds")]
    public Vector2 velocity;
    public float gravity;

    [Header("Collision")]
    public LayerMask collisionMask;
    public Sprite explosionSprite;

    private void Start()
    {
        bc = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        Actions.resetProjectiles += DeActivate;
        Actions.levelEnd += DeActivate;
    }

    private void OnDisable()
    {
        Actions.resetProjectiles -= DeActivate;
        Actions.levelEnd -= DeActivate;
    }

    void DeActivate()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.eulerAngles += Vector3.forward * 400f * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)velocity * Time.fixedDeltaTime;

        velocity.y -= gravity * Time.fixedDeltaTime;

        bool isColliding = Physics2D.OverlapBox(transform.position, bc.size, transform.eulerAngles.z, collisionMask);

        if (isColliding)
        {
            velocity = Vector2.zero;
            gravity = 0f;

            GetComponent<SpriteRenderer>().sprite = explosionSprite;
            bc.size = Vector2.one * 2;

            Destroy(gameObject, 0.25f);
        }
    }
}
