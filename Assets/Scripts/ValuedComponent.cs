using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ValuedComponent : MonoBehaviour
{
    private float value;
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
                this.inputComponent.positionChanged.RemoveListener(OnInputComponentPositionChanged);
            }

            this.inputComponent = value;

            if (this.inputComponent != null)
            {
                this.inputComponent.valueChanged.AddListener(OnInputComponentValueChanged);
                this.inputComponent.positionChanged.AddListener(OnInputComponentPositionChanged);
            }
        }
    }

    [SerializeField] protected bool onlyCopyInput;
    [SerializeField] protected bool placeOnInput;


    [HideInInspector] public UnityEvent valueChanged;
    [HideInInspector] public UnityEvent positionChanged;


    private Vector3 _lastFrameStartPosition;

    protected Vector3 startPosition;
    protected Quaternion startRotation;

    public virtual void Start()
    {
        this.startPosition = this.transform.position;
        this.startRotation = this.transform.rotation;

        if (this.inputComponent != null)
        {
            this.inputComponent.valueChanged.AddListener(OnInputComponentValueChanged);
            this.inputComponent.positionChanged.AddListener(OnInputComponentPositionChanged);
        }

        if (this.placeOnInput && this.inputComponent != null)
            this.PlaceOnInput();
    }

    public virtual void Update()
    {
        if (_lastFrameStartPosition != startPosition)
        {
            this.positionChanged.Invoke();
            _lastFrameStartPosition = startPosition;
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
        if (this.onlyCopyInput)
            this.Value = this.inputComponent.value;
        else
        {

            this.Value = (int)GetConnectionDirection() * this.inputComponent.Value * this.inputComponent.DistancePerValue() / DistancePerValue();
        }
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

        this.transform.position = this.startPosition = GetPositionToPlaceOnInput();
        UpdateValueRender();
    }

    private void OnInputComponentPositionChanged()
    {
        if (placeOnInput)
            PlaceOnInput();
    }

    public enum ConnectionDirection : int
    {
        INVERSE = -1,
        NORMAL = 1
    }
}
