using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Differential : MonoBehaviour
{
    [SerializeField]
    public Gear endGear1;

    [SerializeField]
    public Gear endGear2;

    [SerializeField]
    public Gear spiderGear1;

    [SerializeField]
    public Gear spiderGear2;

    [SerializeField]
    public Gear outputGear;

    public void Start()
    {
        endGear1.valueChanged.AddListener(UpdateValues);
        endGear2.valueChanged.AddListener(UpdateValues);
    }

    public void UpdateValues()
    {
        outputGear.Value = (endGear1.Value + endGear2.Value) / 2f;
        spiderGear1.Value = (-endGear1.Value + endGear2.Value) / 2f;
        spiderGear2.Value = (-endGear1.Value + endGear2.Value) / 2f;
    }
}
