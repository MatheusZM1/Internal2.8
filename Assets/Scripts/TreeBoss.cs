using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBoss : MonoBehaviour
{
    EnemyHP healthScript;
    Animator animator;

    [Header("Logic")]
    public int currentState;
    public float stateDuration;
    public float startDelay;

    Coroutine IntroCoroutine;

    [Header("Spike Attack")]
    public GameObject[] spikes;

    private void Awake()
    {
        healthScript = GetComponent<EnemyHP>();

        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    private void OnEnable()
    {
        Actions.startLevel += IntroFunc;
    }

    private void OnDisable()
    {
        Actions.startLevel -= IntroFunc;
    }

    void IntroFunc()
    {
        if (IntroCoroutine != null) StopCoroutine(IntroCoroutine);
        IntroCoroutine = StartCoroutine(Intro());
    }

    IEnumerator Intro()
    {
        currentState = -1;
        yield return new WaitForSeconds(startDelay);

        animator.enabled = true;
        animator.Play("Intro");

        yield return new WaitForSeconds(startDelay);

        currentState = 1;
    }

    private void FixedUpdate()
    {
        stateDuration += Time.fixedDeltaTime;
        switch (currentState)
        {
            case 1:
                animator.Play("Idle");
                if (stateDuration >= 5f)
                {
                    stateDuration = Random.Range(2, 3);

                    if (currentState == 2)
                    {
                        StartCoroutine(Spikes());
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
    }

    IEnumerator Spikes()
    {
        animator.Play("Spikes");

        yield return new WaitForSeconds(1f);
    }
}
