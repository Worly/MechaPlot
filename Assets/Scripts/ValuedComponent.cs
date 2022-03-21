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
        set
        {
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
    public bool placeOnInput;

    [SerializeField]
    public UnityEvent valueChanged;

    protected Vector3 startPosition;
    protected Quaternion startRotation;

    public virtual void Start()
    {
        this.startPosition = this.transform.localPosition;
        this.startRotation = this.transform.localRotation;
    
        if (this.inputComponent != null)
            this.inputComponent.valueChanged.AddListener(UpdateValueFromInput);

        if (this.placeOnInput && this.inputComponent != null)
            this.PlaceOnInput();
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

    public virtual Vector3 GetLeftEdgePosition()
    {
        return this.transform.position;
    }

    public Vector3 GetPositionToPlaceOnInput()
    {
        if (inputComponent == null)
            return Vector3.zero;


        var inputEdgePosition = inputComponent.GetLeftEdgePosition();

        var myEdgePosition = GetLeftEdgePosition();
        var myOffset = myEdgePosition - this.transform.position;

        return inputEdgePosition + myOffset;
    }

    public void PlaceOnInput()
    {
        if (inputComponent == null)
            return;

        var positionToPlaceOn = GetPositionToPlaceOnInput();

        Vector3 localPosition;
        if (this.transform.parent != null)
            localPosition = this.transform.parent.InverseTransformPoint(positionToPlaceOn);
        else
            localPosition = positionToPlaceOn;

        this.startPosition = localPosition;

        this.transform.localPosition = this.startPosition;
        UpdateValueRender();
    }
}
