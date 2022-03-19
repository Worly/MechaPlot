using UnityEngine;

public class Rack : ValuedComponent
{
    [SerializeField]
    public RackMeshGenerator rackMeshGenerator;

    [SerializeField]
    public float length;

    private Vector3? startPosition;

    public override void Start()
    {
        base.Start();

        GenerateMesh();

        this.startPosition = transform.localPosition;
    }

    public void GenerateMesh()
    {
        this.rackMeshGenerator.length = length;
        this.rackMeshGenerator.GenerateMesh();
    }

    protected override void UpdateValueRender()
    {
        if (this.startPosition == null)
            this.startPosition = transform.localPosition;

        transform.localPosition = this.startPosition.Value + transform.up * Value;
    }

    public override float DistancePerValue()
    {
        return 1;
    }
}
