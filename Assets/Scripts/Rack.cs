using UnityEngine;

public class Rack : ValuedComponent
{
    [SerializeField]
    public RackMeshGenerator rackMeshGenerator;

    [SerializeField]
    public float length;

    public override void Start()
    {
        base.Start();

        GenerateMesh();
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

        transform.localPosition = this.startPosition + transform.up * Value;
    }

    public override float DistancePerValue()
    {
        return 1;
    }

    public override Vector3 GetLeftEdgePosition()
    {
        return this.transform.position - this.transform.right * (rackMeshGenerator.width / 2f + rackMeshGenerator.toothWidth / 2f);
    }
}
