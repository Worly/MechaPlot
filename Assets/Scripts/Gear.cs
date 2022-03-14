using UnityEngine;

public class Gear : ValuedComponent
{
    [SerializeField]
    public GearMeshGenerator gearMeshGenerator;

    [SerializeField]
    public float circumference;

    private Quaternion? startRotation;

    public override void Start()
    {
        base.Start();

        this.GenerateMesh();

        this.startRotation = transform.localRotation;
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

        transform.localRotation = this.startRotation.Value * Quaternion.Euler(0, 0, Value);
    }

    public override float DistancePerValue()
    {
        return this.circumference / 360;
    }
}
