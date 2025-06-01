using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TailRenderer : MonoBehaviour
{
    [Header("Tail Joint Transforms")]
    public List<Transform> tailJoints;

    [Header("Line Renderer Settings")]
    public float startWidth = 0.5f;
    public float endWidth = 0.1f;
    public Material tailMaterial;
    public int interpolationSteps = 5; // More = smoother curve

    private LineRenderer lineRenderer;
    private List<Vector3> smoothPoints = new List<Vector3>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = tailMaterial;
        lineRenderer.widthCurve = AnimationCurve.Linear(0, startWidth, 1, endWidth);
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (tailJoints == null || tailJoints.Count < 2)
            return;

        smoothPoints.Clear();

        for (int i = 0; i < tailJoints.Count - 1; i++)
        {
            Vector3 p0 = tailJoints[Mathf.Max(i - 1, 0)].position;
            Vector3 p1 = tailJoints[i].position;
            Vector3 p2 = tailJoints[i + 1].position;
            Vector3 p3 = tailJoints[Mathf.Min(i + 2, tailJoints.Count - 1)].position;

            for (int j = 0; j < interpolationSteps; j++)
            {
                float t = j / (float)interpolationSteps;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                smoothPoints.Add(point);
            }
        }

        // Add the last joint
        smoothPoints.Add(tailJoints[tailJoints.Count - 1].position);

        lineRenderer.positionCount = smoothPoints.Count;
        lineRenderer.SetPositions(smoothPoints.ToArray());
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Catmull-Rom spline
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}
