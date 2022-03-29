using UnityEngine;

public class Rack : ValuedComponent
{
    [SerializeField] private RackMeshGenerator rackMeshGenerator;
    [SerializeField] private float length;

    public override void Start()
    {
        base.Start();

        GenerateMesh();
    }

    public void GenerateMesh()
    {
        this.rackMeshGenerator.length = length;
        this.rackMeshGenerator.Generate();
    }

    protected override void UpdateValueRender()
    {
        if (this.startPosition == null)
            this.startPosition = transform.localPosition;

        transform.localPosition = this.startPosition + transform.up * Value;
    }

    public override float DistancePerValue()
    {
        return 1;
    }

    public Vector3 GetPositionOfEdge(Vector3 direction)
    {
        var globalPosition = this.transform.TransformPoint(this.startPosition);

        return globalPosition + direction * (rackMeshGenerator.width / 2f);
    }
}
