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

    [SerializeField] private Rack inputRackY;
    [SerializeField] private Rack inputRackX;

    [SerializeField] private NumberLine xNumberLine;
    [SerializeField] private NumberLine yNumberLine;

    public Gear InputGearY => inputGearY;
    public Gear InputGearX => inputGearX;

    public Rack InputRackY => inputRackY;
    public Rack InputRackX => inputRackX;

    public void Start()
    {
        lineRenderer.positionCount = 0;
    }

    public void Update()
    {
        var position = GetCurrentPenPosition();

        if (lineRenderer.positionCount == 0 || (lineRenderer.GetPosition(lineRenderer.positionCount - 1) - position).sqrMagnitude > drawPeriod * drawPeriod)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, GetCurrentPenPosition());
        }

        if (Input.GetKeyDown(KeyCode.R))
            lineRenderer.positionCount = 0;
    }

    public void GenerateCoordinateSystem(float xFrom, float xTo, float yFrom, float yTo)
    {
        xNumberLine.fromValue = xFrom;
        xNumberLine.toValue = xTo;
        xNumberLine.Generate();

        yNumberLine.fromValue = yFrom;
        yNumberLine.toValue = yTo;
        yNumberLine.Generate();
    }

    private Vector3 GetCurrentPenPosition()
    {
        return lineRenderer.transform.InverseTransformPoint(penPoint.position);
    }
}
