using UnityEngine;

public class Rack : ValuedComponent
{
    [SerializeField] private RackMeshGenerator rackMeshGenerator;
    [SerializeField] private float length;

    private float DistancePerTooth => rackMeshGenerator.toothWidth * 2f;

    public override void Awake()
    {
        base.Awake();

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

    public override void UpdateMeshOffset()
    {
        var myOffset = this.GetMeshOffsetFor(InputComponent);
        var otherOffset = InputComponent.GetMeshOffsetFor(this, true);

        this.rackMeshGenerator.transform.localPosition = new Vector3(0, (myOffset + otherOffset) * DistancePerTooth, 0);
    }

    public override float GetMeshOffsetFor(ValuedComponent valuedComponent, bool withMyOffset = false)
    {
        if (valuedComponent is Gear gear)
        {
            Vector3 startGlobalPosition;
            if (transform.parent == null)
                startGlobalPosition = StartLocalPosition;
            else
                startGlobalPosition = transform.parent.TransformPoint(this.StartLocalPosition);

            var inverseTransformMatrix = Matrix4x4.TRS(startGlobalPosition, this.transform.rotation, Vector3.one).inverse;
            var offset = inverseTransformMatrix.MultiplyPoint3x4(gear.transform.position).y;

            if (withMyOffset)
                offset -= rackMeshGenerator.transform.localPosition.y;

            return (offset % DistancePerTooth) / DistancePerTooth;
        }

        return 0;
    }
}
