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
            this.startPosition = transform.position;

        transform.position = this.startPosition + transform.up * Value;
    }

    public override float DistancePerValue()
    {
        return 1;
    }
}
