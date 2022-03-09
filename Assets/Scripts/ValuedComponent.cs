using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ValuedComponent : MonoBehaviour
{
    [SerializeProperty("Value")]
    public float value;
    public float Value
    {
        get => value;
        set
        {
            if (this.value == value)
                return;

            this.value = value;
            this.valueChanged.Invoke();
            this.UpdateValueRender();
        }
    }

    [SerializeField]
    public ValuedComponent inputComponent;
    public ValuedComponent InputComponent
    {
        get => inputComponent;
        set {
            if (this.inputComponent == value)
                return;

            if (this.inputComponent != null)
                this.inputComponent.valueChanged.RemoveListener(UpdateValueFromInput);

            this.inputComponent = value;

            if (this.inputComponent != null)
                this.inputComponent.valueChanged.AddListener(UpdateValueFromInput);
        }
    }

    [SerializeField]
    public bool onlyCopyInput;

    [SerializeField]
    public UnityEvent valueChanged;

    public virtual void Start()
    {
        if (this.inputComponent != null)
            this.inputComponent.valueChanged.AddListener(UpdateValueFromInput);
    }

    protected virtual void UpdateValueRender()
    {
    }

    public virtual float DistancePerValue()
    {
        return 1;
    }

    private void UpdateValueFromInput()
    {
        if (this.onlyCopyInput)
            this.Value = this.inputComponent.value;
        else
            this.Value = -this.inputComponent.Value * this.inputComponent.DistancePerValue() / DistancePerValue();
    }
}
