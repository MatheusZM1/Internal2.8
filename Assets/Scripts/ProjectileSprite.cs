using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSprite : MonoBehaviour
{

    ProjectileBehaviour projectileBehaviour;
    private void Start()
    {
        projectileBehaviour = transform.GetComponentInParent<ProjectileBehaviour>();
    }

    private void OnBecameInvisible()
    {
        projectileBehaviour.OffscreenFunc();
    }
}
