using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ValuedComponent : MonoBehaviour
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

    [SerializeField] private bool onlyCopyInput;
    public bool OnlyCopyInput
    {
        get => onlyCopyInput;
        set
        {
            if (this.onlyCopyInput == value)
                return;

            this.onlyCopyInput = value;

            if (this.inputComponent != null)
                UpdateValue();
        }
    }

    [HideInInspector] public UnityEvent valueChanged;

    protected Vector3 startPosition;
    protected Quaternion startRotation;

    public Vector3 StartLocalPosition => startPosition;
    public Quaternion StartLocalRotation => startRotation;

    public virtual void Awake()
    {
        this.startPosition = this.transform.localPosition;
        this.startRotation = this.transform.localRotation;

        if (this.inputComponent != null)
        {
            this.inputComponent.valueChanged.AddListener(OnInputComponentValueChanged);
            UpdateValue();
        }
    }

    public virtual void Update()
    {
        if (this.inputComponent != null && !onlyCopyInput)
            UpdateMeshOffset();
    }

    protected abstract void UpdateValueRender();
    public abstract float DistancePerValue();
    public abstract void UpdateMeshOffset();
    public abstract float GetMeshOffsetFor(ValuedComponent valuedComponent, bool withMyOffset = false);

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
