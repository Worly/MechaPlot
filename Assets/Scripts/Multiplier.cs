using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplier : MonoBehaviour
{
    [SerializeField]
    public Rack inputRackWithPivot;

    [SerializeField]
    public Transform rackPivotArm;

    [SerializeField]
    public Transform rackPivot;

    [SerializeField]
    public Transform pivotPoint;

    [SerializeField]
    public Rack inputRack;

    [SerializeField]
    public Rack outputRack;

    [SerializeField]
    public Transform pin;

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
