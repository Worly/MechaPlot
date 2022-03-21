using UnityEngine;

public class Gear : ValuedComponent
{
    [SerializeField]
    public GearMeshGenerator gearMeshGenerator;

    [SerializeField]
    public float circumference;

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
            this.startRotation = transform.localRotation;

        transform.localRotation = this.startRotation * Quaternion.Euler(0, 0, Value);
    }

    public override float DistancePerValue()
    {
        return this.circumference / 360;
    }

    public override Vector3 GetLeftEdgePosition()
    {
        var radius = this.circumference / (2 * Mathf.PI);
        var right = Vector3.Cross(Vector3.up, transform.forward);
        return this.transform.position - right * (radius + this.gearMeshGenerator.toothHeight / 2);
    }
}
