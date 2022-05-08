using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Divider : MonoBehaviour
{
    [SerializeField] private Rack outputRackWithPivot;
    [SerializeField] private Transform rackPivotArm;
    [SerializeField] private Transform rackPivot;
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private Rack inputRack1;
    [SerializeField] private Rack inputRack2;
    [SerializeField] private Transform pin;

    [SerializeField] private Gear inputGear;
    [SerializeField] private Gear outputGear;

    public Gear InputGear => inputGear;
    public Gear OutputGear => outputGear;

    private float kValue;
    private Vector3 rackPivotStartPosition;

    public void Start()
    {
        rackPivotStartPosition = this.rackPivot.localPosition;

        var pivotArmPosition = this.transform.InverseTransformPoint(rackPivotArm.position);
        var pivotPointPosition = this.transform.InverseTransformPoint(pivotPoint.position);
        kValue = pivotPointPosition.x - pivotArmPosition.x;

        Debug.Log("kValue: " + kValue);
    }

    public void Update()
    {
        var dividerValue = inputRack2.Value != 0 ? inputRack2.Value : 1;

        outputRackWithPivot.Value = inputRack1.Value / dividerValue * kValue;

        pivotPoint.localRotation = Quaternion.Euler(0, 0, Mathf.Atan(outputRackWithPivot.Value / kValue) * Mathf.Rad2Deg);
        var pivotArmPosition = this.transform.InverseTransformPoint(rackPivotArm.position);
        var pivotPointPosition = this.transform.InverseTransformPoint(pivotPoint.position);
        var distance = (new Vector2(pivotArmPosition.x, pivotArmPosition.y) - new Vector2(pivotPointPosition.x, pivotPointPosition.y)).magnitude;
        rackPivot.localPosition = rackPivotStartPosition - Vector3.right * (distance - kValue);

        pin.localPosition = new Vector3(inputRack2.transform.localPosition.x, inputRack1.transform.localPosition.y, pin.localPosition.z);
    }
}
