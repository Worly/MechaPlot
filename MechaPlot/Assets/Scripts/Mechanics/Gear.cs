using UnityEngine;

public class Gear : ValuedComponent
{
    [SerializeField] private GearMeshGenerator gearMeshGenerator;
    [SerializeField] private float circumference;

    public GearMeshGenerator GearMeshGenerator => gearMeshGenerator;

    private float AnglePerTooth => 360f / circumference;

    public float Circumference
    {
        get => circumference;
        set
        {
            if (circumference == value)
                return;

            circumference = value;
            GenerateMesh();

            UpdateValue();
        }
    }

    public override void Awake()
    {
        base.Awake();

        this.GenerateMesh();
    }

    public void GenerateMesh()
    {
        this.gearMeshGenerator.circumference = circumference;
        this.gearMeshGenerator.Generate();
    }

    protected override void UpdateValueRender()
    {
        if (this.startRotation == null)
            this.startRotation = transform.localRotation;

        transform.localRotation = this.startRotation * Quaternion.AngleAxis(Value * 360, Vector3.forward);
    }

    public override float DistancePerValue()
    {
        return this.circumference;
    }

    public override void UpdateMeshOffset()
    {
        var myOffset = this.GetMeshOffsetFor(InputComponent);
        var otherOffset = InputComponent.GetMeshOffsetFor(this, true);

        this.gearMeshGenerator.transform.localRotation = Quaternion.Euler(0, 0, (otherOffset + myOffset) * AnglePerTooth);
    }

    public override float GetMeshOffsetFor(ValuedComponent valuedComponent, bool withMyOffset = false)
    {
        float angleOffset = 0;
        if (valuedComponent is Gear gear)
            angleOffset = ValuedComponentsHelper.AngleBetween(this, gear);
        else if (valuedComponent is Rack rack)
            angleOffset = ValuedComponentsHelper.AngleBetween(this, rack);

        if (withMyOffset)
            angleOffset -= this.GearMeshGenerator.transform.localRotation.eulerAngles.z;

        return (angleOffset % AnglePerTooth) / AnglePerTooth;
    }

    public override ConnectionDirection GetConnectionDirection()
    {
        if (InputComponent is Gear gear && gear.gearMeshGenerator.gearType == GearType.Belt)
            return ConnectionDirection.NORMAL;
        else
            return base.GetConnectionDirection();
    }
}
