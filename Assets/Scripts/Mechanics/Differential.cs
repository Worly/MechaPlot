using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Differential : MonoBehaviour
{
    [SerializeField] private Gear endGear1;
    [SerializeField] private Gear endGear2;
    [SerializeField] private Gear spiderGear1;
    [SerializeField] private Gear spiderGear2;
    [SerializeField] private Gear inputGear1;
    [SerializeField] private Gear inputGear2;
    [SerializeField] private Gear outputGear;
    [SerializeField] private Gear correctOutputGear;

    public Gear InputGear1 => inputGear1;
    public Gear InputGear2 => inputGear2;
    public Gear OutputGear => correctOutputGear;

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
