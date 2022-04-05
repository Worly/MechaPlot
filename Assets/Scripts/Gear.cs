using UnityEngine;

public class Gear : ValuedComponent
{
    [SerializeField] private GearMeshGenerator gearMeshGenerator;
    [SerializeField] private float circumference;

    public GearMeshGenerator GearMeshGenerator => gearMeshGenerator;

    public float Circumference { 
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

    public override void Start()
    {
        base.Start();

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

    public override ConnectionDirection GetConnectionDirection()
    {
        if (InputComponent is Gear gear && gear.gearMeshGenerator.gearType == GearType.Belt)
            return ConnectionDirection.NORMAL;
        else
            return base.GetConnectionDirection();
    }

    public Vector3 GetPositionOfEdge(Vector3 direction)
    {
        var radius = this.circumference / (2 * Mathf.PI) + this.gearMeshGenerator.toothHeight / 2f;
        return this.transform.position + direction * radius;
    }

    public void PlaceOn(Vector3 position, Vector3 direction)
    {
        var myOffset = GetPositionOfEdge(direction) - this.transform.position;
        this.transform.position = position - myOffset;
        this.startPosition = this.transform.localPosition;
    }

    public void PlaceOn(Gear gear, Vector3 direction)
    {
        if (this.transform.forward != gear.transform.forward)
        {
            Debug.LogError("Gears must face the same direction!");
            return;
        }

        if (Vector3.Dot(transform.forward, direction) > 0.001)
        {
            Debug.LogError("Direction must be perpendicular to rotation of the gear (tranform.forward)");
            return;
        }

        this.PlaceOn(gear.GetPositionOfEdge(direction), -direction);
    }

    public void PlaceNextTo(Gear gear, Vector3 direction)
    {
        if (this.transform.forward != gear.transform.forward)
        {
            Debug.LogError("Gears must face the same direction!");
            return;
        }

        if (Mathf.Abs(Vector3.Dot(transform.forward, direction.normalized)) < 0.999)
        {
            Debug.LogError("Direction must be paralel to rotation of the gear (tranform.forward)");
            return;
        }

        this.transform.position = gear.transform.position + direction.normalized * (this.gearMeshGenerator.thickness / 2f + gear.gearMeshGenerator.thickness / 2f);
        this.startPosition = this.transform.localPosition;
        this.gearMeshGenerator.innerCircumference = gear.gearMeshGenerator.innerCircumference;
    }

    public void PlaceOn(Rack rack, Vector3 direction)
    {
        if (this.transform.forward != rack.transform.forward)
        {
            Debug.LogError("Gear and a rack must face the same direction!");
            return;
        }

        if (Vector3.Dot(transform.forward, direction) > 0.001)
        {
            Debug.LogError("Direction must be perpendicular to rotation of the gear (tranform.forward)");
            return;
        }

        this.PlaceOn(rack.GetPositionOfEdge(direction), -direction);
    }

    public void PlaceOn(ValuedComponent valuedComponent, Vector3 direction)
    {
        if (valuedComponent is Gear gear)
            PlaceOn(gear, direction);
        else if (valuedComponent is Rack rack)
            PlaceOn(rack, direction);
    }
}
