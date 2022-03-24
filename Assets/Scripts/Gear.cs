using UnityEngine;

public class Gear : ValuedComponent
{
    [SerializeField] private GearMeshGenerator gearMeshGenerator;
    [SerializeField] private float circumference;

    public float Circumference { get => circumference; }

    public override void Start()
    {
        base.Start();

        this.GenerateMesh();
    }

    public void GenerateMesh()
    {
        this.gearMeshGenerator.circumference = circumference;
        this.gearMeshGenerator.GenerateMesh();
    }

    protected override void UpdateValueRender()
    {
        if (this.startRotation == null)
            this.startRotation = transform.rotation;

        transform.rotation = this.startRotation * Quaternion.AngleAxis(Value, Vector3.forward);
    }

    public override float DistancePerValue()
    {
        return this.circumference / 360;
    }

    public override ConnectionDirection GetConnectionDirection()
    {
        if (InputComponent is Gear gear && gear.gearMeshGenerator.gearType == GearType.Belt)
            return ConnectionDirection.NORMAL;
        else
            return base.GetConnectionDirection();
    }

    public override Vector3 GetLeftEdgePosition()
    {
        var radius = this.circumference / (2 * Mathf.PI);
        var right = Vector3.Cross(Vector3.up, transform.forward);
        return this.transform.position - right * (radius + this.gearMeshGenerator.toothHeight / 2);
    }
}
