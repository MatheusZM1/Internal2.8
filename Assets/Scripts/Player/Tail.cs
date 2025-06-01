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
    Vector2 previousPosition;
    Vector2 currentPosition;

    [Header("Tail Physics")]
    public float distance;
    public float smoothing;
    public float updRate;

    [Header("Physics")]
    public float gravity;

    [Header("Raycast")]
    public LayerMask groundMask;

    private void Start()
    {
        StartCoroutine(UpdateTailJoints());

        previousPosition = transform.position;
        currentPosition = transform.position;
    }

    private void Update()
    {
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime; // Interpolate sprite position between previous and current position
        Vector2 interpolatedPosition = Vector2.Lerp(previousPosition, currentPosition, interpolationFactor);
        sprite.transform.position = interpolatedPosition;
    }

    IEnumerator UpdateTailJoints()
    {
        while (true)
        {
            Vector3 direction = targetPos.position - transform.position;

            transform.eulerAngles = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg * Vector3.forward;

            bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.05f, groundMask);

            if (!isGrounded) transform.position -= Vector3.up * gravity * (1 / updRate);

            if (tailHead) transform.position = targetPos.GetChild(0).transform.position + targetPos.GetChild(0).transform.up * -0.25f;
            else transform.position = Vector2.Lerp(transform.position, targetPos.position - (transform.right * distance), smoothing);

            previousPosition = currentPosition;
            currentPosition = transform.position;

            yield return new WaitForSeconds(1 / updRate);
        }
    }
}
