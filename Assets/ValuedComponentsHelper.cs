using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

static class ValuedComponentsHelper
{
    public static float AngleBetween(Gear gear1, Gear gear2)
    {
        return Vector3.SignedAngle(gear1.StartLocalRotation * Vector3.up, gear2.transform.position - gear1.transform.position, gear1.transform.forward);
    }

    public static float AngleBetween(Gear gear, Rack rack)
    {
        return Vector3.SignedAngle(gear.StartLocalRotation * Vector3.up, -rack.transform.right, gear.transform.forward);
    }
}
