using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputScript))]
public class PlayerMovement : MonoBehaviour
{
    PlayerWeapon weaponScript;
    PlayerHP healthScript;
    BoxCollider2D bc;

    [Header("Static")]
    public static int playersDead;

    [Header("Input")]
    public bool isPlayerTwo;
    public float inputLockedCooldown;
    public float horizontal;
    public float vertical;

    InputScript inputScript;

    [Header("Directions")]
    public Vector2 directionFacing;

    [Header("Variables")]
    public bool isAlive;
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
    public LayerMask voidMask;

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

    [Header("Reviving")]
    public bool touchingOtherPlayer;
    public float reviveTimer;
    public LayerMask playerMask;
    PlayerMovement otherPlayerScript;

    [Header("Lock")]
    public bool isLocked;

    [Header("Sprite")]
    public Transform spriteObj;
    public SpriteRenderer bodyObj;
    public SpriteRenderer leftEye, rightEye;
    public LineRenderer tail;

    Vector2 startPos;

    Vector2 bodyStartScale;
    Coroutine bodySquishCoroutine;

    Vector2 eyeStartScale;
    Coroutine eyeSquishCoroutine;

    Vector2 previousPosition;
    Vector2 currentPosition;

    void Start()
    {
        inputScript = GetComponent<InputScript>();

        weaponScript = GetComponent<PlayerWeapon>();
        healthScript = GetComponent<PlayerHP>();
        bc = GetComponent<BoxCollider2D>();

        previousPosition = transform.position;
        currentPosition = transform.position;

        directionFacing.x = 1;

        bodyStartScale = bodyObj.size;
        eyeStartScale = leftEye.transform.localScale;

        isAlive = true;

        if (isPlayerTwo)
        {
            otherPlayerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        }
        else
        {
            otherPlayerScript = GameObject.FindGameObjectWithTag("PlayerTwo").GetComponent<PlayerMovement>();
        }

        startPos = transform.position;
    }

    private void OnEnable()
    {
        Actions.levelReset += Respawn;
    }

    private void OnDisable()
    {
        Actions.levelReset -= Respawn;
    }

    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            Time.timeScale = 1f;
        }
        if (Input.GetKeyDown("2"))
        {
            Time.timeScale = 1f / 300f;
        }
        if (Input.GetKeyDown("3"))
        {
            Time.timeScale = 0.2f;
        }

        if (GameManagerScript.instance.gameDead) return;

        if ((Input.GetKeyDown("return") && !isPlayerTwo) || inputScript.GetActionDown("Pause"))
        {
            if (!GameManagerScript.instance.gamePaused) GameManagerScript.instance.PauseGame();
        }

        if (Input.GetKeyDown("r"))
        {
            Actions.levelReset?.Invoke();
        }

        if (GameManagerScript.instance.gamePaused) return;

        if (isAlive)
        {
            if (inputLockedCooldown <= 0)
            {
                horizontal = inputScript.GetHorizontal();
                vertical = inputScript.GetVertical();
            }
            else
            {
                horizontal = 0;
                vertical = 0;
                inputLockedCooldown -= Time.deltaTime;
            }

            directionFacing.y = vertical;

            if (isGrounded && !isDashing)
            {
                isLocked = (Input.GetKey("left ctrl") && !isPlayerTwo) || inputScript.GetActionHold("EastB");
            }
            else
            {
                isLocked = false;
            }

            Jump();
            Dash();
            Parry();
            //Revive();
        }
        else
        {
            horizontal = 0;
            vertical = 0;
        }
    }

    private void LateUpdate()
    {
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime; // Interpolate sprite position between previous and current position
        Vector2 interpolatedPosition = Vector2.Lerp(previousPosition, currentPosition, interpolationFactor);
        spriteObj.transform.position = interpolatedPosition + Vector2.up * 0.3125f;
        spriteObj.transform.position = new Vector2(Mathf.Round(spriteObj.transform.position.x * 32f) / 32f, Mathf.Round(spriteObj.transform.position.y * 32f) / 32f);
    }

    private void FixedUpdate()
    {
        HandlePhysics();

        // Update previous and current positions (used for smoothly interpolating player sprite)
        previousPosition = currentPosition;
        currentPosition = transform.position;
    }

    void HandlePhysics()
    {
        float horizontalInput = horizontal > 0 ? Mathf.Ceil(horizontal) : Mathf.Floor(horizontal);
        if (inputLockedCooldown <= 0) velocity.x = horizontalInput * speed * weaponScript.playerSpeedModifier; // x Speed

        if (isDashing) // Dash movement
        {
            if (currentDashTimer > 0) // Apply dash
            {
                velocity = Vector3.right * directionFacing.x * dashLength / dashTime;
                currentDashTimer -= Time.fixedDeltaTime;
                if (currentDashTimer <= 0) // Dash end
                {
                    float overheardPercent = (0.005f - Mathf.Abs(currentDashTimer)) / Time.fixedDeltaTime; // Decrement dash overhead fail-safe
                    velocity.x *= overheardPercent;
                    isDashing = false;
                    currentDashCooldown = dashCooldown;
                }
            }
        }

        if (!isLocked) transform.position += (Vector3)velocity * Time.fixedDeltaTime; // Move

        float wallRayLength = Mathf.Max(bc.size.x * 0.5f, Mathf.Abs(velocity.x) * Time.fixedDeltaTime);
        RaycastHit2D wallToTheLeftBottom = Physics2D.Raycast(transform.position - Vector3.up * bc.size.y * 0.45f, -Vector2.right, wallRayLength, solidGroundMask | boundaryMask);
        RaycastHit2D wallToTheLeftTop = Physics2D.Raycast(transform.position + Vector3.up * bc.size.y * 0.45f, -Vector2.right, wallRayLength, solidGroundMask | boundaryMask);
        RaycastHit2D wallToTheRightBottom = Physics2D.Raycast(transform.position - Vector3.up * bc.size.y * 0.45f, Vector2.right, wallRayLength, solidGroundMask | boundaryMask);
        RaycastHit2D wallToTheRightTop = Physics2D.Raycast(transform.position + Vector3.up * bc.size.y * 0.45f, Vector2.right, wallRayLength, solidGroundMask | boundaryMask);

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

        float groundRayLength = Mathf.Max(0.15f, Mathf.Abs(velocity.y) * Time.fixedDeltaTime);
        RaycastHit2D groundedLeft = Physics2D.Raycast(transform.position + new Vector3(-bc.size.x * 0.49f, -bc.size.y * 0.45f), Vector2.down, groundRayLength, groundMask);
        RaycastHit2D groundedRight = Physics2D.Raycast(transform.position + new Vector3(bc.size.x * 0.49f, -bc.size.y * 0.45f), Vector2.down, groundRayLength, groundMask);

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

        RaycastHit2D voidBelow = Physics2D.Raycast(transform.position, Vector2.down, groundRayLength, voidMask);

        if (voidBelow.collider != null)
        {
            healthScript.TakeDamage(1, false);
            if (healthScript.health > 0)
            {
                isJumping = false;
                CancelDash();
                velocity = new Vector2(velocity.x, 22f);
            }
        }
    }

    private void BecomeGrounded()
    {
        isGrounded = true;
        isJumping = false;
        coyoteTimer = coyoteTimerPreset;
        canDash = true;

        if (isAlive)
        {
            if (velocity.y < -2f)
            {
                StopSquishBody();
                Vector2 targetSquish = new Vector2(1.1f, 1f);
                bodySquishCoroutine = StartCoroutine(SquishBody(targetSquish, 8f));
            }
        }
        else
        {
            velocity.x = 0;
        }
    }

    private void Jump()
    {
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime; // Decrease jump buffer timer

        if ((Input.GetKeyDown("space") && !isPlayerTwo) || inputScript.GetActionDown("SouthB"))
        {
            if (!isDashing && inputLockedCooldown <= 0) jumpBufferTimer = jumpBufferTimerPreset;
        }

        if (coyoteTimer > 0 && jumpBufferTimer > 0)
        {
            RaycastHit2D semiSolidGroundLeft = Physics2D.Raycast(transform.position + new Vector3(-bc.size.x * 0.49f, -bc.size.y * 0.45f), Vector2.down, 0.15f, semiSolidGroundMask);
            RaycastHit2D semiSolidGroundRight = Physics2D.Raycast(transform.position + new Vector3(bc.size.x * 0.49f, -bc.size.y * 0.45f), Vector2.down, 0.15f, semiSolidGroundMask);
            if ((semiSolidGroundLeft.collider != null || semiSolidGroundRight.collider != null) && vertical < 0) // Descend semi solid platform
            {
                transform.position += Vector3.up * -0.05f;
                velocity.y = -3f;
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
                StopSquishBody();
                Vector2 targetSquish = new Vector2(0.85f, 1.15f);
                bodySquishCoroutine = StartCoroutine(SquishBody(targetSquish, 2f));
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
            if (!isDashing && canDash && inputLockedCooldown <= 0) // Dash
            {
                isDashing = true;
                canDash = false;
                currentDashTimer = dashTime;

                StopSquishBody();
                Vector2 targetSquish = new Vector2(1.2f, 0.8f);
                bodySquishCoroutine = StartCoroutine(SquishBody(targetSquish, 4f));
            }
        }
    }

    public void CancelDash()
    {
        isDashing = false;
        currentDashCooldown = 0;
    }

    private void Parry()
    {
        if ((Input.GetKeyDown("w") && !isPlayerTwo) && inputLockedCooldown <= 0) // Parry
        {

        }
    }

    private void Revive()
    {
        if (touchingOtherPlayer)
        {
            if ((Input.GetKeyDown("w") || inputScript.GetActionDown("NorthB")) && inputLockedCooldown <= 0)
            {
                reviveTimer += Time.deltaTime;

                if (reviveTimer >= 1)
                {
                    otherPlayerScript.Respawn();
                }
            }
            else
            {
                reviveTimer = 0;
            }
        }
        else
        {
            reviveTimer = 0;
        }
    }

    void StopSquishBody()
    {
        if (bodySquishCoroutine != null) StopCoroutine(bodySquishCoroutine);
        bodyObj.size = bodyStartScale;
        bodyObj.transform.localPosition = new Vector2(0, -0.25f);
    }

    IEnumerator SquishBody(Vector2 targetSquish, float squishSpeed)
    {
        Vector2 squishFactor = targetSquish;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * squishSpeed;
            Vector2 currentSquish = Vector2.Lerp(bodyStartScale, bodyStartScale * squishFactor, MathFunctions.SineInOut(Mathf.PingPong(t * 2, 1f)));
            bodyObj.size = new Vector2(currentSquish.x, currentSquish.y);

            yield return null;
        }
        bodyObj.size = bodyStartScale;
        bodyObj.transform.localPosition = new Vector2(0, -0.25f);
    }

    public void StartSquishEyes(Vector2 targetSquish, float squishSpeed)
    {
        StopSquishEyes();
        eyeSquishCoroutine = StartCoroutine(SquishEyes(targetSquish, squishSpeed));
    }

    void StopSquishEyes()
    {
        if (eyeSquishCoroutine != null) StopCoroutine(eyeSquishCoroutine);
        leftEye.transform.localScale = eyeStartScale;
        rightEye.transform.localScale = eyeStartScale;
    }

    IEnumerator SquishEyes(Vector2 targetSquish, float squishSpeed)
    {
        Vector2 squishFactor = targetSquish;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * squishSpeed;
            Vector2 currentSquish = Vector2.Lerp(eyeStartScale, eyeStartScale * squishFactor, MathFunctions.SineInOut(Mathf.PingPong(t * 2, 1f)));
            leftEye.transform.localScale = new Vector2(currentSquish.x, currentSquish.y);
            rightEye.transform.localScale = new Vector2(currentSquish.x, currentSquish.y);

            leftEye.color = Color.Lerp(Color.white, new Color(1, 0.3f, 0.3f), MathFunctions.SineInOut(Mathf.PingPong(t * 2, 1f)));
            rightEye.color = Color.Lerp(Color.white, new Color(1, 0.3f, 0.3f), MathFunctions.SineInOut(Mathf.PingPong(t * 2, 1f)));

            yield return null;
        }
        leftEye.transform.localScale = eyeStartScale;
        rightEye.transform.localScale = eyeStartScale;
    }

    public void StartDeath()
    {
        StopSquishBody();
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        float t = 0;

        playersDead++;

        while (t < 1)
        {
            t += Time.deltaTime * 3;
            bodyObj.transform.localPosition = Vector2.Lerp(Vector2.up * -0.25f, new Vector2(0, -0.35f), MathFunctions.EaseIn(t, 2));
            bodyObj.transform.eulerAngles = Vector3.forward * Mathf.Lerp(0, -90 * directionFacing.x, MathFunctions.EaseIn(t, 2));
            bodyObj.color = Color.Lerp(Color.black, new Color(0.5f, 0f, 0f), MathFunctions.EaseIn(t, 2));
            tail.startColor = bodyObj.color;
            tail.endColor = bodyObj.color;

            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

        bool twoPlayers = GameManagerScript.instance.playerTwoExists;

        if ((twoPlayers && playersDead > 1) || !twoPlayers)
        {
            Actions.onGameDeath?.Invoke();
            Time.timeScale = 0;
        }
    }

    public void Respawn()
    {
        isAlive = true;
        GetComponent<PlayerHP>().UpdateHeath(3);

        bodyObj.transform.localPosition = Vector2.up * -0.25f;
        bodyObj.transform.eulerAngles = Vector3.zero;
        bodyObj.color = Color.black;
        tail.startColor = bodyObj.color;
        tail.endColor = bodyObj.color;

        transform.position = startPos;
        velocity = Vector2.zero;

        weaponScript.LoadWeapons();
    }
}
