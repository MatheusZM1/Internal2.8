using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputScript))]
public class PlayerMovement : MonoBehaviour
{
    BoxCollider2D bc;

    [Header("Input")]
    public bool isPlayerTwo;
    public float horizontal;
    public float vertical;
    public string horizontalPositiveKey;
    public string horizontalNegativeKey;
    public string verticalPositiveKey;
    public string verticalNegativeKey;
    InputScript inputScript;

    [Header("Directions")]
    public Vector2 directionFacing;

    [Header("Variables")]
    public float speed;
    public float maxFallSpeed;
    public float gravity;

    [Header("Physics")]
    public Vector2 velocity;

    [Header("Ground Check")]
    public bool isGrounded;
    public LayerMask groundMask;
    public LayerMask solidGroundMask;
    public LayerMask semiSolidGroundMask;
    public LayerMask boundaryMask;

    [Header("Jumping")]
    public bool isJumping;
    public float jumpForce;
    public float jumpBufferTimerPreset;
    public float coyoteTimerPreset;
    float coyoteTimer;
    float jumpBufferTimer;

    [Header("Dashing")]
    public bool isDashing;
    public bool canDash;
    public float dashLength;
    public float dashTime;
    public float dashCooldown;
    float currentDashTimer;
    float currentDashCooldown;

    [Header("Parry")]
    public float parryTimeLeft;
    public float parryDuration;

    [Header("Lock")]
    public bool isLocked;

    [Header("Sprite")]
    public Transform spriteObj;
    public Transform bodyObj;
    Vector2 spriteStartScale;
    Coroutine jumpSquishCoroutine;
    Vector2 previousPosition;
    Vector2 currentPosition;

    void Start()
    {
        inputScript = GetComponent<InputScript>();

        bc = GetComponent<BoxCollider2D>();

        previousPosition = transform.position;
        currentPosition = transform.position;

        directionFacing.x = 1;

        spriteStartScale = bodyObj.transform.localScale;
    }

    float GetCustomAxis(string positiveKey, string negativeKey)
    {
        float positiveInput = Input.GetKey(positiveKey) ? 1f : 0f;
        float negativeInput = Input.GetKey(negativeKey) ? -1f : 0f;
        return Mathf.Abs(positiveInput + negativeInput) > ControlOptions.controllerDeadZone ? positiveInput + negativeInput : 0;
    }

    private float GetHorizontal()
    {
        float horizontalKey = isPlayerTwo ? 0 : GetCustomAxis(horizontalPositiveKey, horizontalNegativeKey);
        return Mathf.Clamp(horizontalKey + inputScript.GetPlayerAxis().x, -1, 1);
    }

    private float GetVertical()
    {
        float verticalKey = isPlayerTwo ? 0 : GetCustomAxis(verticalPositiveKey, verticalNegativeKey);
        return Mathf.Clamp(verticalKey + inputScript.GetPlayerAxis().y, -1, 1);
    }

    void Update()
    {
        horizontal = GetHorizontal();
        vertical = GetVertical();

        if (Input.GetKeyDown("1"))
        {
            Time.timeScale = 1f;
        }
        if (Input.GetKeyDown("2"))
        {
            Time.timeScale = 0.5f;
        }
        if (Input.GetKeyDown("3"))
        {
            Time.timeScale = 0.2f;
        }

        directionFacing.y = vertical;

        if (isGrounded && !isDashing)
        {
            isLocked = (Input.GetKey("left ctrl") && !isPlayerTwo) || inputScript.GetActionHold("RBumper");
        }
        else
        {
            isLocked = false;
        }

        Jump();
        Dash();
        Parry();
    }

    private void LateUpdate()
    {
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime; // Interpolate sprite position between previous and current position
        Vector2 interpolatedPosition = Vector2.Lerp(previousPosition, currentPosition, interpolationFactor);
        spriteObj.transform.position = interpolatedPosition + Vector2.up * 0.3f;
    }

    private void FixedUpdate()
    {
        if (!isLocked) transform.position += (Vector3)velocity * Time.fixedDeltaTime; // Move

        velocity.x = horizontal * speed; // x Speed

        if (isDashing) // Dash movement
        {
            if (currentDashTimer > 0) // Apply dash
            {
                velocity = Vector3.right * directionFacing.x * dashLength / dashTime;
                currentDashTimer -= Time.fixedDeltaTime;
                if (currentDashTimer <= 0) // Dash end
                {
                    float overheardPercent = (0.02f - Mathf.Abs(currentDashTimer)) / Time.fixedDeltaTime; // Decrement dash overhead fail-safe
                    velocity.x *= overheardPercent;
                    isDashing = false;
                    currentDashCooldown = dashCooldown;
                }
            }
        }

        RaycastHit2D wallToTheLeftBottom = Physics2D.Raycast(transform.position - Vector3.up * bc.size.y * 0.45f, -Vector2.right, bc.size.x * 0.5f, solidGroundMask | boundaryMask);
        RaycastHit2D wallToTheLeftTop = Physics2D.Raycast(transform.position + Vector3.up * bc.size.y * 0.45f, -Vector2.right, bc.size.x * 0.5f, solidGroundMask | boundaryMask);
        RaycastHit2D wallToTheRightBottom = Physics2D.Raycast(transform.position - Vector3.up * bc.size.y * 0.45f, Vector2.right, bc.size.x * 0.5f, solidGroundMask | boundaryMask);
        RaycastHit2D wallToTheRightTop = Physics2D.Raycast(transform.position + Vector3.up * bc.size.y * 0.45f, Vector2.right, bc.size.x * 0.5f, solidGroundMask | boundaryMask);

        if (velocity.x < 0) // Check for walls to the left while moving left
        {
            directionFacing.x = -1;
            RaycastHit2D rayToUse = wallToTheLeftBottom;

            if ((wallToTheLeftTop.collider != null && wallToTheLeftTop.distance < wallToTheLeftBottom.distance) || wallToTheLeftBottom.collider == null) rayToUse = wallToTheLeftTop;

            if (rayToUse.collider != null)
            {
                transform.position = new Vector2(rayToUse.point.x + bc.size.x * 0.5f, transform.position.y);
                velocity.x = 0;
            }
        }
        else if (velocity.x > 0) // Check for walls to the right while moving right
        {
            directionFacing.x = 1;
            RaycastHit2D rayToUse = wallToTheRightBottom;

            if ((wallToTheRightTop.collider != null && wallToTheRightTop.distance < wallToTheRightBottom.distance) || wallToTheRightBottom.collider == null) rayToUse = wallToTheRightTop;

            if (rayToUse.collider != null)
            {
                transform.position = new Vector2(rayToUse.point.x - bc.size.x * 0.5f, transform.position.y);
                velocity.x = 0;
            }
        }

        if (isGrounded)
        {

        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
            if (!isDashing) velocity.y = Mathf.Max(velocity.y - gravity * Time.fixedDeltaTime, maxFallSpeed); // Apply gravity when not dashing

            if (velocity.y >= 0) // Check for ceiling while rising
            {
                float rayLength = Mathf.Abs(velocity.y) * Time.fixedDeltaTime + bc.size.y * 0.5f;
                RaycastHit2D ceilingAboveLeft = Physics2D.Raycast(transform.position - Vector3.right * bc.size.x * 0.49f, Vector2.up, rayLength, solidGroundMask);
                RaycastHit2D ceilingAboveRight = Physics2D.Raycast(transform.position + Vector3.right * bc.size.x * 0.49f, Vector2.up, rayLength, solidGroundMask);
                RaycastHit2D rayToUse = ceilingAboveLeft;

                if ((ceilingAboveRight.collider != null && ceilingAboveRight.distance < ceilingAboveLeft.distance) || ceilingAboveLeft.collider == null) rayToUse = ceilingAboveRight;

                if (rayToUse.collider != null) // Bump into ceiling
                {
                    transform.position = new Vector2(transform.position.x, Mathf.Round((rayToUse.point.y - bc.size.y * 0.5f) * 32f) / 32f);
                    velocity.y = 0;
                }
            }
        }

        float grounRayLength = Mathf.Max(0.15f, Mathf.Abs(velocity.y) * Time.fixedDeltaTime);
        RaycastHit2D groundedLeft = Physics2D.Raycast(transform.position + new Vector3(-bc.size.x * 0.49f, -bc.size.y * 0.45f), Vector2.down, grounRayLength, groundMask);
        RaycastHit2D groundedRight = Physics2D.Raycast(transform.position + new Vector3(bc.size.x * 0.49f, -bc.size.y * 0.45f), Vector2.down, grounRayLength, groundMask);

        if (groundedLeft.collider != null || groundedRight.collider != null) // Check for ground
        {
            RaycastHit2D rayToUse = groundedLeft;
            if ((groundedRight.collider != null && groundedRight.distance < groundedLeft.distance) || groundedLeft.collider == null) rayToUse = groundedRight;

            if (velocity.y <= 0) // If falling, stick to ground, otherwise, do not stick to ground (semi-solid ground)
            {
                BecomeGrounded();
                transform.position = new Vector2(transform.position.x, rayToUse.point.y + bc.size.y * 0.5f);
                velocity.y = 0;
            }
            else isGrounded = false;
        }
        else
        {
            isGrounded = false;
        }

        // Update previous and current positions (used for smoothly interpolating player sprite)
        previousPosition = currentPosition;
        currentPosition = transform.position;
    }

    private void BecomeGrounded()
    {
        isGrounded = true;
        isJumping = false;
        coyoteTimer = coyoteTimerPreset;
        canDash = true;

        if (velocity.y < -2f)
        {
            StopSquishSprite();
            Vector2 targetSquish = new Vector2(1.1f, 1f);
            jumpSquishCoroutine = StartCoroutine(SquishSprite(targetSquish, 8f));
        }
    }

    private void Jump()
    {
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime; // Decrease jump buffer timer

        if ((Input.GetKeyDown("space") && !isPlayerTwo) || inputScript.GetActionDown("SouthB"))
        {
            if (!isDashing) jumpBufferTimer = jumpBufferTimerPreset;
        }

        if (coyoteTimer > 0 && jumpBufferTimer > 0)
        {
            RaycastHit2D semiSolidGroundLeft = Physics2D.Raycast(transform.position + new Vector3(-bc.size.x * 0.49f, -bc.size.y * 0.45f), Vector2.down, 0.15f, semiSolidGroundMask);
            RaycastHit2D semiSolidGroundRight = Physics2D.Raycast(transform.position + new Vector3(bc.size.x * 0.49f, -bc.size.y * 0.45f), Vector2.down, 0.15f, semiSolidGroundMask);
            if ((semiSolidGroundLeft.collider != null || semiSolidGroundRight.collider != null) && vertical < 0) // Descend semi solid platform
            {
                velocity.y = -4.75f;
                isJumping = true;
                jumpBufferTimer = 0;
                coyoteTimer = 0;
            }
            else // Jump
            {
                velocity.y = jumpForce;
                isJumping = true;
                jumpBufferTimer = 0;
                coyoteTimer = 0;
                StopSquishSprite();
                Vector2 targetSquish = new Vector2(0.85f, 1.15f);
                jumpSquishCoroutine = StartCoroutine(SquishSprite(targetSquish, 2f));
            }
        }

        if ((!Input.GetKey("space") || isPlayerTwo) && !inputScript.GetActionHold("SouthB")) // Release jump (not getkeyup due to jump buffer, as key may be released during buffer
        {
            if (isJumping && velocity.y > 0)
            {
                velocity.y *= 0.5f;
                isJumping = false;
            }
        }
    }

    private void Dash()
    {
        if (currentDashCooldown > 0) // Decrease dash cooldown
        {
            currentDashCooldown -= Time.deltaTime;
        }
        else if ((Input.GetKeyDown("left shift") && !isPlayerTwo) || inputScript.GetActionDown("WestB"))
        {
            if (!isDashing && canDash) // Dash
            {
                isDashing = true;
                canDash = false;
                currentDashTimer = dashTime;
            }

            StopSquishSprite();
        }
    }

    private void Parry()
    {
        if ((Input.GetKeyDown("w") && !isPlayerTwo)) // Parry
        {

        }
    }

    void StopSquishSprite()
    {
        if (jumpSquishCoroutine != null) StopCoroutine(jumpSquishCoroutine);
        bodyObj.transform.localScale = spriteStartScale;
    }

    IEnumerator SquishSprite(Vector2 targetSquish, float squishSpeed)
    {
        Vector2 squishFactor = targetSquish;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * squishSpeed;
            Vector2 currentSquish = Vector2.Lerp(spriteStartScale, spriteStartScale * squishFactor, MathFunctions.SineInOut(Mathf.PingPong(t * 2, 1f)));
            bodyObj.transform.localScale = new Vector3(currentSquish.x, currentSquish.y, 1);

            yield return null;
        }
        bodyObj.transform.localScale = spriteStartScale;
    }

}
