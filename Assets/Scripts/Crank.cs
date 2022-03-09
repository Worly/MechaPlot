using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crank : MonoBehaviour
{
    [SerializeField]
    public Gear gear;

    [SerializeField]
    public float crankSpeed;

    void Update()
    {
        gear.Value += crankSpeed * Time.deltaTime;
    }
}
