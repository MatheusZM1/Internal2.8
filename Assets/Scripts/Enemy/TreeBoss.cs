using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBoss : MonoBehaviour
{
    EnemyHP healthScript;
    Collider2D col;
    Animator animator;

    bool bossDead;

    [Header("Logic")]
    public int currentState;
    public float stateDuration;
    public float startDelay;
    public Sprite startSprite;

    Coroutine IntroCoroutine;
    Coroutine RootsCoroutine;
    Coroutine ShootCorouite;
    Coroutine SpewCoroutine;

    [Header("Roots Attack")]
    public GameObject roots;

    [Header("Shoot Attack")]
    public GameObject logOne;
    public GameObject logTwo;

    [Header("Apple Attack")]
    public GameObject appleBombPrefab;

    private void Awake()
    {
        healthScript = GetComponent<EnemyHP>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        Actions.startLevel += IntroFunc;
        Actions.levelReset += Restart;
    }

    private void OnDisable()
    {
        Actions.startLevel -= IntroFunc;
        Actions.levelReset -= Restart;
    }

    void IntroFunc()
    {
        StopAllCoroutines();
        IntroCoroutine = StartCoroutine(Intro());
    }

    void Restart()
    {
        col.enabled = false;

        animator.enabled = false;
        currentState = -1;
        GetComponent<SpriteRenderer>().sprite = startSprite;

        roots.transform.localPosition = Vector2.zero;

        logOne.transform.localPosition = Vector2.right * 8.5f;
        logTwo.transform.localPosition = Vector2.right * 13.5f;
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(startDelay);

        animator.enabled = true;
        animator.Play("Intro");

        yield return new WaitForSeconds(startDelay);

        col.enabled = true;
        currentState = 1;
        stateDuration = 0f;
    }

    private void FixedUpdate()
    {
        if (bossDead) return;

        stateDuration += Time.fixedDeltaTime;
        switch (currentState)
        {
            case 1:
                animator.Play("Idle");
                if (stateDuration >= 5.28f)
                {
                    stateDuration = 0f;
                    currentState = Random.Range(2, 5);

                    if (currentState == 2)
                    {
                        RootsCoroutine = StartCoroutine(Roots());
                    }
                    else if (currentState == 3)
                    {
                        ShootCorouite = StartCoroutine(Shoot());
                    }
                    else if(currentState == 4)
                    {
                        SpewCoroutine = StartCoroutine(Spew());
                    }
                }
                break;

            case 2:
                break;

            case 3:
                break;

            case 4:
                break;
        }

        if (healthScript.health <= 0)
        {
            currentState = -1;
            animator.Play("Explode");
            col.enabled = false;

            roots.transform.localPosition = Vector2.zero;

            logOne.transform.localPosition = Vector2.right * 8.5f;
            logTwo.transform.localPosition = Vector2.right * 13.5f;
            Actions.levelEnd?.Invoke();

            StopAllCoroutines();

            bossDead = true;
        }
    }

    IEnumerator Roots()
    {
        animator.Play("Root");

        yield return new WaitForSeconds(0.5f);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 5;
            roots.transform.localPosition = Vector2.Lerp(Vector2.zero, Vector2.up, MathFunctions.EaseOut(t, 3));

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 5;
            roots.transform.localPosition = Vector2.Lerp(Vector2.up, Vector2.up * 7, MathFunctions.EaseOut(t, 3));

            yield return null;
        }

        yield return new WaitForSeconds(1.4f);

        t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 5;
            roots.transform.localPosition = Vector2.Lerp(Vector2.up * 7, Vector2.zero, MathFunctions.EaseIn(t, 3));

            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        currentState = 1;
        stateDuration = 0;
    }

    IEnumerator Shoot()
    {
        animator.Play("Shoot");

        yield return new WaitForSeconds(1f);

        StartCoroutine(MoveLogs());

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        currentState = 1;
        stateDuration = 0f;
    }

    IEnumerator MoveLogs()
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 0.2f;

            logOne.transform.localPosition = Vector2.Lerp(Vector2.right * 8.5f, Vector2.right * -13.5f, t);
            logTwo.transform.localPosition = Vector2.Lerp(Vector2.right * 13.5f, Vector2.right * -8.5f, t);
            logOne.transform.eulerAngles = Vector3.forward * Mathf.Lerp(0, 360f * 2.33f, t);
            logTwo.transform.eulerAngles = Vector3.forward * Mathf.Lerp(0, 360f * 2.33f, t);
            yield return null;
        }
    }

    IEnumerator Spew()
    {
        animator.Play("Spew");

        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < 5; i++)
        {
            AppleBomb appleBomb = Instantiate(appleBombPrefab).GetComponent<AppleBomb>();
            appleBomb.transform.position = transform.position + new Vector3(0, -2f);
            appleBomb.velocity = Quaternion.Euler(new Vector3(0, 0, Random.Range(-18, 18))) * Vector3.up * 15;
            yield return new WaitForSeconds(0.6f);
        }

        float startLoop = Mathf.Floor(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

        while (Mathf.Floor(animator.GetCurrentAnimatorStateInfo(0).normalizedTime) == startLoop)
        {
            yield return null;
        }

        currentState = 1;
        stateDuration = 0f;
    }
}
