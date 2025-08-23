using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    [Header("Health")]
    public float health;
    public float maxHealth;
    public float healthPercent;
    Material healthMat;

    [Header("Variables")]
    public float time;
    public float waveSpeed;

    [Header("Animation")]
    public float lerpSpeed;
    public float lerpRoughness;
    public float minAmplitude, maxAmplitude;
    public float minFrequency, maxFrequency;

    //[Header("Heart Beat")]
    //float beatTimer;
    //public float startbeatSpeed, endBeatSpeed;
    //public float startBeatDelay, endBeatDelay;
    //public float firstBeatScale = 1.2f;
    //public float secondBeatScale = 1.1f;
    //public float beatSpeed;
    //public float beatDelay;

    //public AudioSource source;
    //public AudioClip beatOne;
    //public AudioClip beatTwo;
    //float soundsPlayed;

    private void Start()
    {
        healthMat = GetComponent<SpriteRenderer>().material;
        healthMat.SetFloat("_WaveSpeed", 2);
    }

    private void Update()
    {
        float diff = Mathf.Abs((healthPercent - health) / maxHealth);
        if (healthPercent < health)
        {
            healthPercent += lerpSpeed * Time.deltaTime * (diff + lerpRoughness) / (100 / maxHealth);
            if (healthPercent > health) healthPercent = health;
        }
        else if (healthPercent > health)
        {
            healthPercent -= (lerpSpeed * Time.deltaTime * (diff + lerpRoughness)) / (100 / maxHealth);
            if (healthPercent < health) healthPercent = health;
        }

        waveSpeed = Mathf.Lerp(3f, 30f, diff);
        healthMat.SetFloat("_WaveAmplitude", Mathf.Lerp(minAmplitude, maxAmplitude, diff));
        healthMat.SetFloat("_WaveFrequency", Mathf.Lerp(-minFrequency, -maxFrequency, diff));

        time += Time.deltaTime * waveSpeed;
        healthMat.SetFloat("_TimeValue", time);
        healthMat.SetFloat("_FillAmount", healthPercent / maxHealth);
    }
}
