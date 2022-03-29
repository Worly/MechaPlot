using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plotter : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform penPoint;
    [SerializeField] private float drawPeriod;
    [SerializeField] private Gear inputGearY;
    [SerializeField] private Gear inputGearX;

    public Gear InputGearY => inputGearY;
    public Gear InputGearX => inputGearX;

    public void Start()
    {
        lineRenderer.positionCount = 0;
        //lineRenderer.positionCount = 1;
        //lineRenderer.SetPosition(0, GetCurrentPenPosition());
    }

    public void Update()
    {
        var position = GetCurrentPenPosition();

        if (lineRenderer.positionCount == 0 || (lineRenderer.GetPosition(lineRenderer.positionCount - 1) - position).sqrMagnitude > drawPeriod * drawPeriod)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, GetCurrentPenPosition());
        }
    }

    private Vector3 GetCurrentPenPosition()
    {
        return lineRenderer.transform.InverseTransformPoint(penPoint.position);
    }
}
