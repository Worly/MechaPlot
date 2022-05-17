using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class GearPositioning
{
    public static void PlaceOn(this Gear myGear, Gear gear, Vector3 direction)
    {
        if (myGear.transform.forward != gear.transform.forward)
        {
            Debug.LogError("Gears must face the same direction!");
            return;
        }

        if (Vector3.Dot(myGear.transform.forward, direction) > 0.001)
        {
            Debug.LogError("Direction must be perpendicular to rotation of the gear (tranform.forward)");
            return;
        }

        myGear.PlaceOn(gear.GetPositionOfEdge(direction), -direction);
    }

    public static void PlaceNextTo(this Gear myGear, Gear gear, Vector3 direction)
    {
        if (myGear.transform.forward != gear.transform.forward)
        {
            Debug.LogError("Gears must face the same direction!");
            return;
        }

        if (Mathf.Abs(Vector3.Dot(myGear.transform.forward, direction.normalized)) < 0.999)
        {
            Debug.LogError("Direction must be paralel to rotation of the gear (tranform.forward)");
            return;
        }

        myGear.transform.position = gear.transform.position + direction.normalized * (myGear.GearMeshGenerator.thickness / 2f + myGear.GearMeshGenerator.thickness / 2f);
        myGear.SetPositionLocal(myGear.transform.localPosition);
        myGear.GearMeshGenerator.innerCircumference = gear.GearMeshGenerator.innerCircumference;
    }

    private static void PlaceOn(this Gear gear, Vector3 position, Vector3 direction)
    {
        var myOffset = gear.GetPositionOfEdge(direction) - gear.transform.position;
        gear.transform.position = position - myOffset;
        gear.SetPositionLocal(gear.transform.localPosition);
    }

    public static Vector3 GetPositionOfEdge(this Gear gear, Vector3 direction)
    {
        var radius = gear.Circumference / (2 * Mathf.PI) + gear.GearMeshGenerator.toothHeight / 2f;
        return gear.transform.position + direction * radius;
    }
}
