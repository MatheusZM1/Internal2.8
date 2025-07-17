using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextObj : MonoBehaviour
{
    SpriteFontMesh text;
    public float alphaSpeed;
    public float scrollSpeed;
    int isVisible;

    void Start()
    {
        text = transform.GetComponentInChildren<SpriteFontMesh>();
        text.alpha = 0;
        text.UpdateTextPercent(0);
    }

    void Update()
    {
        if (isVisible > 0)
        {
            if (text.percent < 1 || text.alpha < 1)
            {
                if (text.percent < 1)
                {
                    text.percent += (scrollSpeed / text.actualCharCount) * Time.deltaTime;
                    //AudioManager.instance.Play("Text");
                    if (text.percent >= 1)
                    {
                        text.percent = 1;
                        //AudioManager.instance.Stop("Text");
                    }
                }
                text.alpha = Mathf.Clamp01(text.alpha + alphaSpeed * Time.deltaTime);
                text.UpdateTextPercent(text.percent);
            }
        }
        else if (text.percent > 0 || text.alpha > 0)
        {
            text.alpha = Mathf.Clamp01(text.alpha - alphaSpeed * Time.deltaTime);
            text.percent = Mathf.Clamp01(text.percent - (scrollSpeed / text.actualCharCount) * 3 * Time.deltaTime);
            text.UpdateTextPercent(text.percent);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerTwo"))
        {
            isVisible++;
            if (text.alpha <= 0) text.percent = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerTwo")) isVisible--;
    }
}