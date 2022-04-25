using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crank : MonoBehaviour
{
    [SerializeField] private Gear gear;
    [SerializeField] private float crankSpeed;

    private bool stop = true;

    public Gear Gear => gear;

    void Update()
    {
        if (!stop)
            gear.Value += crankSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.G))
            stop = !stop;
    }
}
