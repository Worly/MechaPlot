using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [SerializeField] private float lineSegmentLength = 0.1f;
    [SerializeField] private Gear gear1;
    [SerializeField] private Gear gear2;

    private LineRenderer lineRenderer;

    public void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Start()
    {
        if (gear1 != null && gear2 != null)
            SetGears(gear1, gear2);
    }

    public void SetGears(Gear gear1, Gear gear2)
    {
        if (gear1 == null || gear2 == null)
            throw new ArgumentNullException();

        if (this.gear1 != null)
            this.gear1.valueChanged.RemoveListener(OnGear1ValueChanged);

        this.gear1 = gear1;
        this.gear2 = gear2;

        this.gear1.valueChanged.AddListener(OnGear1ValueChanged);

        Generate();
    }

    public void Generate()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;

        if (gear1 == null || gear2 == null)
        {
            Debug.LogError("Gears cannot be null!");
            return;
        }

        if (gear1.transform.forward != gear2.transform.forward || Mathf.Abs(Vector3.Angle(gear1.transform.forward, gear2.transform.position - gear1.transform.position) - 90) > 0.01)
        {
            Debug.LogError("Gears must be in a same plane to be connected by a rope!");
            return;
        }

        var gear1Radius = gear1.Circumference / (2 * Mathf.PI) + lineRenderer.startWidth / 2f;
        var gear2Radius = gear2.Circumference / (2 * Mathf.PI) + lineRenderer.startWidth / 2f;

        var distance = Vector3.Distance(gear1.transform.position, gear2.transform.position);

        // get direct common tangent https://www.math-only-math.com/important-properties-of-direct-common-tangents.html
        var extraAngle = 90 - Mathf.Acos((gear1Radius - gear2Radius) / distance) * Mathf.Rad2Deg;

        var gear1Circumference = gear1Radius * (2 * Mathf.PI);
        var gear2Circumference = gear2Radius * (2 * Mathf.PI);

        var gear1PointCount = Mathf.CeilToInt((gear1Circumference / 2f) / lineSegmentLength);
        var gear2PointCount = Mathf.CeilToInt((gear2Circumference / 2f) / lineSegmentLength);

        var gear1StartVector = Vector3.Cross(gear1.transform.forward, gear2.transform.position - gear1.transform.position).normalized;
        var gear2StartVector = Vector3.Cross(gear2.transform.forward, gear1.transform.position - gear2.transform.position).normalized;

        lineRenderer.positionCount = gear1PointCount + gear2PointCount;

        var positions = new Vector3[lineRenderer.positionCount];

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 centerPosition;
            float radius;
            Vector3 startVector;
            float angle;
            if (i < gear1PointCount)
            {
                centerPosition = gear1.transform.position;
                radius = gear1Radius;
                startVector = gear1StartVector;
                angle = i / (gear1PointCount - 1f) * (180f + extraAngle * 2f) - extraAngle;
            }
            else
            {
                centerPosition = gear2.transform.position;
                radius = gear2Radius;
                startVector = gear2StartVector;
                angle = (i - gear1PointCount) / (gear2PointCount - 1f) * (180f - extraAngle * 2f) + extraAngle;
            }

            positions[i] = centerPosition + Quaternion.AngleAxis(angle, gear1.transform.forward) * startVector * radius;
        }

        lineRenderer.SetPositions(positions);
    }

    private void OnGear1ValueChanged()
    {
        var tiling = lineRenderer.material.GetTextureScale("_MainTex");
        lineRenderer.material.SetTextureOffset("_MainTex", new Vector2(-gear1.Value * gear1.DistancePerValue() * tiling.x, 0));
    }
}
