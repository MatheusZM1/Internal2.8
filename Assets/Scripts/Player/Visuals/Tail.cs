using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour
{
    [Header("Sprite")]
    public Transform sprite;

    [Header("Tail")]
    public bool tailHead;
    public Transform targetPos;

    [Header("Tail Physics")]
    public float distance;
    public float smoothing;

    [Header("Physics")]
    public float gravity;

    [Header("Raycast")]
    public LayerMask groundMask;

    private void FixedUpdate()
    {
        Vector3 direction = (targetPos.position + Vector3.up * 0.03125f) - transform.position;

        transform.eulerAngles = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg * Vector3.forward;

        bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.05f, groundMask);

        if (!isGrounded) transform.position -= Vector3.up * gravity * Time.fixedDeltaTime;

        if (tailHead) transform.position = targetPos.GetChild(0).transform.position + targetPos.GetChild(0).transform.up * -0.25f;
        else transform.position = Vector2.Lerp(transform.position, targetPos.position - (transform.right * distance), smoothing);

        sprite.transform.position = transform.position;
    }
}
