using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crank : MonoBehaviour
{
    [SerializeField] private Gear gear;

    public static readonly float crankSpeed = 0.5f;
    public static readonly float valueLimit = 10;
    private bool stop = true;

    public Gear Gear => gear;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            stop = !stop;
    }

    void FixedUpdate()
    {
        if (!stop)
        {
            var newValue = gear.Value + crankSpeed * Time.fixedDeltaTime;
            if (newValue > valueLimit)
                newValue = valueLimit;

            gear.Value = newValue;
        }
    }
}
