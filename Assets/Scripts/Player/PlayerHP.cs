using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    PlayerMovement playerScript;
    BoxCollider2D bc;

    [Header("Health")]
    public int health;
    public float invulnarabilityDuration;
    float currentInvulnaribilityDuration;
    public LayerMask hurtMask;

    [Header("UI Visuals")]
    public HealthScript healthScript;
    public SpriteFontMesh healthText;

    [Header("Player Visuals")]
    public GameObject playerSprite;
    public LineRenderer playerTail;

    private void Start()
    {
        playerScript = GetComponent<PlayerMovement>();
        bc = GetComponent<BoxCollider2D>();

        health = 3;
    }

    private void FixedUpdate()
    {
        if (currentInvulnaribilityDuration > 0)
        {
            currentInvulnaribilityDuration = Mathf.Max(currentInvulnaribilityDuration - Time.fixedDeltaTime, 0);
            float flickerDuration = 0.2f;
            playerSprite.SetActive(Mathf.Pow(currentInvulnaribilityDuration, 1.25f) * 0.8f % flickerDuration < flickerDuration * 0.5f);
            playerTail.enabled = playerSprite.activeSelf;
        }

        bool isColliding = Physics2D.OverlapBox(transform.position, bc.size, transform.eulerAngles.z, hurtMask);

        if (isColliding)
        {
            TakeDamage(1);
        }
    }

    public void UpdateHeath(int newHealth)
    {
        health = newHealth;
        healthScript.health = health;
        healthText.GenerateText(health.ToString());
    }

    public void TakeDamage(int damage, bool freezeInput = true)
    {
        if (health <= 0 || currentInvulnaribilityDuration > 0) return;

        health = Mathf.Max(health - damage, 0);
        if (health > 0) currentInvulnaribilityDuration = invulnarabilityDuration;

        if (freezeInput) playerScript.inputLockedCooldown = 0.3f;
        playerScript.velocity = Vector2.up * 6;
        playerScript.isJumping = false;
        playerScript.CancelDash();

        if (health <= 0)
        {
            playerScript.isAlive = false;
            playerScript.StartDeath();
        }
        else
        {
            Vector2 targetSquish = new Vector2(1.5f, 1.5f);
            playerScript.StartSquishEyes(targetSquish, 4);
        }

        UpdateHeath(health);
    }
}
