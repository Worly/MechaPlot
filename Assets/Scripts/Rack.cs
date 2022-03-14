using UnityEngine;

public class Rack : ValuedComponent
{
    [SerializeField]
    public RackMeshGenerator rackMeshGenerator;

    [SerializeProperty("Length")]
    public float length;
    public float Length
    {
        get => length;
        set
        {
            if (this.length == value)
                return;

            this.length = value;
            this.rackMeshGenerator.length = length;
            this.rackMeshGenerator.GenerateMesh();
        }
    }

    private Vector3? startPosition;

    public override void Start()
    {
        base.Start();

        this.rackMeshGenerator.length = length;
        this.rackMeshGenerator.GenerateMesh();

        this.startPosition = transform.localPosition;
    }

    protected override void UpdateValueRender()
    {
        if (this.startPosition == null)
            this.startPosition = transform.localPosition;

        transform.localPosition = this.startPosition.Value + new Vector3(0, Value, 0);
    }

    public override float DistancePerValue()
    {
        return 0.98f;
    }
}
