using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crank : MonoBehaviour
{
    [SerializeField] private Gear gear;
    [SerializeField] private float crankSpeed;

    void Update()
    {
        gear.Value += crankSpeed * Time.deltaTime;
    }
}
