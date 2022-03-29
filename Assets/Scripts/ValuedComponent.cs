using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ValuedComponent : MonoBehaviour
{
    [SerializeField] private float value;
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

    [SerializeField] private ValuedComponent inputComponent;
    public ValuedComponent InputComponent
    {
        get => inputComponent;
        set
        {
            if (this.inputComponent == value)
                return;

            if (this.inputComponent != null)
            {
                this.inputComponent.valueChanged.RemoveListener(OnInputComponentValueChanged);
            }

            this.inputComponent = value;

            if (this.inputComponent != null)
            {
                this.inputComponent.valueChanged.AddListener(OnInputComponentValueChanged);
                UpdateValue();
            }
        }
    }

    [SerializeField] public bool onlyCopyInput;

    [HideInInspector] public UnityEvent valueChanged;

    protected Vector3 startPosition;
    protected Quaternion startRotation;

    public virtual void Start()
    {
        this.startPosition = this.transform.localPosition;
        this.startRotation = this.transform.localRotation;

        if (this.inputComponent != null)
        {
            this.inputComponent.valueChanged.AddListener(OnInputComponentValueChanged);
            UpdateValue();
        }
    }

    protected virtual void UpdateValueRender()
    {
    }

    public virtual float DistancePerValue()
    {
        return 1;
    }

    public virtual ConnectionDirection GetConnectionDirection()
    {
        return ConnectionDirection.INVERSE;
    }

    private void OnInputComponentValueChanged()
    {
        UpdateValue();
    }

    protected void UpdateValue()
    {
        if (this.InputComponent == null)
            return;

        if (this.onlyCopyInput)
            this.Value = this.inputComponent.value;
        else
            this.Value = (int)GetConnectionDirection() * this.inputComponent.Value * this.inputComponent.DistancePerValue() / DistancePerValue();
    }

    public void SetPositionLocal(Vector3 localPosition)
    {
        this.startPosition = this.transform.localPosition = localPosition;
    }

    public void SetPositionGlobal(Vector3 position)
    {
        this.transform.position = position;
        this.startPosition = this.transform.localPosition;
    }

    public enum ConnectionDirection : int
    {
        INVERSE = -1,
        NORMAL = 1
    }
}
