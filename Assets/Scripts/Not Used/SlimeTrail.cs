using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SlimeTrail : MonoBehaviour
{
    [Header("Player")]
    public PlayerMovement playerMovement;
    private LineRenderer lineRenderer;
    public bool isActive = true;

    [Header("Distances")]
    [SerializeField] private float minDistance = .05f;
    [SerializeField] private float trailLifetime = 1.2f;

    private List<Vector3> points = new List<Vector3>();
    private List<float> birthTimes = new List<float>();

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (isActive)
            AddPointOnMovement();
        FadeOldPoints();
    }

    private void AddPointOnMovement()
    {
        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], transform.position) >= minDistance)
        {
            points.Add(transform.position);
            birthTimes.Add(Time.time);

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
        else
        {
            lineRenderer.SetPositions(points.ToArray());
            points[points.Count - 1] = transform.position;
        }
    }

    private void FadeOldPoints()
    {
        float current = Time.time;

        while (points.Count > 0 && current - birthTimes[0] > trailLifetime)
        {
            points.RemoveAt(0);
            birthTimes.RemoveAt(0);

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        if (points.Count > 1)
        {
            float age = current - birthTimes[0];
            float alpha = 1f - Mathf.Clamp01(age / trailLifetime);

            Color c = lineRenderer.startColor;
            lineRenderer.startColor = c;

            c.a = alpha;
            lineRenderer.endColor = c;
        }
    }
}
