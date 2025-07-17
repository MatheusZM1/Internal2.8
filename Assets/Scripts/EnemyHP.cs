using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    [Header("Health")]
    public float startHealth;
    public float health;
    public BoxCollider2D targetWall;

    [Header("Flash")]
    public SpriteRenderer spriteRenderer;
    Material flashMaterial;
    Coroutine flashCoroutine;

    private void Start()
    {
        flashMaterial = spriteRenderer.material;
        Respawn();
    }

    private void OnEnable()
    {
        Actions.levelReset += Respawn;
    }

    private void OnDisable()
    {
        Actions.levelReset -= Respawn;
    }

    public void TakeDamage(float damage)
    {
        health = Mathf.Max(health - damage, 0);

        if (health <= 0)
        {
            spriteRenderer.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            if (targetWall != null) targetWall.gameObject.SetActive(false);
        }
        else
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(Flash(0.2f));
        }
    }

    IEnumerator Flash(float flashDuration)
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / flashDuration;
            flashMaterial.SetFloat("_BrightFx", Mathf.Lerp(0.2f, 0, t));
            yield return null;
        }

        flashMaterial.SetFloat("_BrightFx", 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile"))
        {
            ProjectileBehaviour projectileScript = other.GetComponent<ProjectileBehaviour>();
            if (projectileScript != null)
            {
                TakeDamage(projectileScript.currentDamage);
                projectileScript.CollideWithEnemy();
            }

        }
    }

    void Respawn()
    {
        health = startHealth;
        spriteRenderer.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        if (targetWall != null) targetWall.gameObject.SetActive(true);
    }
}
