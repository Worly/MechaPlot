using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crank : MonoBehaviour
{
    [SerializeField] private Gear gear;
    [SerializeField] private float crankSpeed;

    private float valueLimit = 10;
    private bool stop = true;

    public Gear Gear => gear;

    void Update()
    {
        if (!stop)
        {
            var newValue = gear.Value + crankSpeed * Time.deltaTime;
            if (newValue > valueLimit)
                newValue = valueLimit;

            gear.Value = newValue;
        }

        if (Input.GetKeyDown(KeyCode.G))
            stop = !stop;
    }
}
