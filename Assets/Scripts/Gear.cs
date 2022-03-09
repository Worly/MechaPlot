using UnityEngine;

public class Gear : ValuedComponent
{
    [SerializeField]
    public GearMeshGenerator gearMeshGenerator;

    [SerializeProperty("Circumference")]
    public float circumference;
    public float Circumference
    {
        get => circumference;
        set
        {
            if (this.circumference == value)
                return;

            this.circumference = value;
            this.gearMeshGenerator.circumference = circumference;
            this.gearMeshGenerator.GenerateMesh();
        }
    }

    private Quaternion? startRotation;

    public override void Start()
    {
        base.Start();

        this.gearMeshGenerator.circumference = circumference;
        this.gearMeshGenerator.GenerateMesh();

        this.startRotation = transform.localRotation;
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
