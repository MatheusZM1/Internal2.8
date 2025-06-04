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

        //if (healthPercent <= 50f)
        //{
        //    beatSpeed = Mathf.Lerp(endBeatSpeed, startbeatSpeed, healthPercent / 50f);
        //    beatDelay = Mathf.Lerp(endBeatDelay, startBeatDelay, healthPercent / 50f);
        //    beatTimer += Time.deltaTime * beatSpeed;
        //    if (beatTimer > 0f && soundsPlayed == 0)
        //    {
        //        source.PlayOneShot(beatOne);
        //        soundsPlayed = 1;
        //    }
        //    if (beatTimer > 0.5f && soundsPlayed == 1)
        //    {
        //        source.PlayOneShot(beatTwo);
        //        soundsPlayed = 2;
        //    }
        //    if (beatTimer > beatDelay)
        //    {
        //        beatTimer -= beatDelay;
        //        soundsPlayed = 0;
        //    }
        //    transform.localScale =  Vector3.one * GetHeartbeatScale(beatTimer);
        //}
        //else
        //{
        //    beatTimer = 0f;
        //    soundsPlayed = 0f;
        //}
    }

    //float GetHeartbeatScale(float t)
    //{
    //    if (t < 1f / 4f)
    //    {
    //        return Mathf.Lerp(1f, firstBeatScale, EaseOut(t * 4f));
    //    }
    //    else if (t < 2f / 4f)
    //    {
    //        float phaseT = (t - 1f / 4f) * 4f;
    //        return Mathf.Lerp(firstBeatScale, secondBeatScale, EaseIn(phaseT));
    //    }
    //    else if (t < 3f / 4f)
    //    {
    //        float phaseT = (t - 2f / 4f) * 4f;
    //        return Mathf.Lerp(secondBeatScale, secondBeatScale + 0.05f, EaseOut(phaseT));
    //    }
    //    else
    //    {
    //        float phaseT = (t - 3f / 4f) * 4f;
    //        return Mathf.Lerp(secondBeatScale + 0.05f, 1f, EaseIn(phaseT));
    //    }
    //}

    //float EaseIn(float x)
    //{
    //    return x; // Quadratic ease-in (slower start, faster end)
    //}

    //float EaseOut(float x)
    //{
    //    return x; // Quadratic ease-out (fast start, slow end)
    //}
}
