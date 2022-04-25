using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplier : MonoBehaviour
{
    [SerializeField] private Rack inputRackWithPivot;
    [SerializeField] private Transform rackPivotArm;
    [SerializeField] private Transform rackPivot;
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private Rack inputRack;
    [SerializeField] private Rack outputRack;
    [SerializeField] private Transform pin;

    [SerializeField] private Gear inputGear1;
    [SerializeField] private Gear inputGear2;
    [SerializeField] private Gear outputGear;

    public Gear InputGear1 => inputGear1;
    public Gear InputGear2 => inputGear2;
    public Gear OutputGear => outputGear;

    private float kValue;
    private Vector3 rackPivotStartPosition;

    public void Start()
    {
        rackPivotStartPosition = this.rackPivot.localPosition;

        var pivotArmPosition = this.transform.InverseTransformPoint(rackPivotArm.position);
        var pivotPointPosition = this.transform.InverseTransformPoint(pivotPoint.position);
        kValue = pivotArmPosition.x - pivotPointPosition.x;
    }

    public void Update()
    {
        outputRack.Value = -inputRack.Value * inputRackWithPivot.Value / kValue;

        pivotPoint.localRotation = Quaternion.Euler(0, 0, Mathf.Atan(inputRackWithPivot.Value / kValue) * Mathf.Rad2Deg);
        var pivotArmPosition = this.transform.InverseTransformPoint(rackPivotArm.position);
        var pivotPointPosition = this.transform.InverseTransformPoint(pivotPoint.position);
        var distance = (new Vector2(pivotArmPosition.x, pivotArmPosition.y) - new Vector2(pivotPointPosition.x, pivotPointPosition.y)).magnitude;
        rackPivot.localPosition = rackPivotStartPosition + Vector3.right * (distance - kValue);

        pin.localPosition = new Vector3(inputRack.transform.localPosition.x, outputRack.transform.localPosition.y, pin.localPosition.z);
    }
}
