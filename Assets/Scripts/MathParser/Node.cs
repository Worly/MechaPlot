using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MathParser
{
    public abstract class Node
    {
        public float MaxValue { get; private set; }
        public float MinValue { get; private set; }
        public float MaxAbsoluteValue { get; private set; }
        public float MinAbsoluteValue { get; private set; }

        public virtual Node Evaluate(float? x = null)
        {
            return this;
        }

        public virtual Node Transform()
        {
            return this;
        }

        public virtual Node LimitMultiplication(float xFrom, float xTo)
        {
            return this;
        }

        public virtual Node LimitDivision(float xFrom, float xTo)
        {
            return this;
        }

        public void FindExtremes(float xFrom, float xTo)
        {
            float xRange = xTo - xFrom;
            float xStep = xRange * ((Crank.crankSpeed * Time.fixedDeltaTime) / Crank.valueLimit);

            MaxValue = float.MinValue;
            MinValue = float.MaxValue;
            MaxAbsoluteValue = 0;
            MinAbsoluteValue = float.MaxValue;


            float x = xFrom;
            do
            {
                var value = (this.Evaluate(x) as ValueNode).Value;
                if (value > MaxValue)
                    MaxValue = value;
                if (value < MinValue)
                    MinValue = value;

                var absValue = Mathf.Abs(value);
                if (absValue > MaxAbsoluteValue)
                    MaxAbsoluteValue = absValue;
                if (absValue < MinAbsoluteValue)
                    MinAbsoluteValue = absValue;

                if (x == xTo)
                    break;

                x += xStep;

                if (x > xTo)
                    x = xTo;

            } while (x <= xTo);
        }
    }

    public class ValueNode : Node
    {
        public float Value { get; set; }

        public ValueNode(string value)
        {
            this.Value = float.Parse(value);
        }

        public ValueNode(float value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class InputNode : Node
    {
        public override string ToString()
        {
            return "x";
        }

        public override Node Evaluate(float? x = null)
        {
            if (x == null)
                return this;
            else
                return new ValueNode(x.Value);
        }
    }

    public class OperationNode : Node
    {
        public Operation Operation { get; set; }

        public Node Left { get; set; }
        public Node Right { get; set; }

        public OperationNode(Operation op, Node left, Node right)
        {
            Operation = op;
            this.Left = left;
            this.Right = right;
        }

        public override string ToString()
        {
            switch (Operation)
            {
                case Operation.POWER:
                    return $"({Left}^{Right})";
                case Operation.MULTIPLICATION:
                    return $"({Left} * {Right})";
                case Operation.DIVISION:
                    return $"({Left} / {Right})";
                case Operation.ADDITION:
                    return $"({Left} + {Right})";
                case Operation.SUBTRACTION:
                    return $"({Left} - {Right})";
                default:
                    return "null";
            }
        }

        public override Node Evaluate(float? x = null)
        {
            var left = Left.Evaluate(x);
            var right = Right.Evaluate(x);

            var leftValue = left as ValueNode;
            var rightValue = right as ValueNode;

            if (IsConstantOperation(out float value, out Node variableNode))
            {
                var variableNodeEvaluated = variableNode == Left ? left : right;

                if (Operation == Operation.MULTIPLICATION && value == 1)
                    return variableNodeEvaluated;
                else if (Operation == Operation.ADDITION && value == 0)
                    return variableNodeEvaluated;
            }

            if (leftValue == null || rightValue == null)
                return new OperationNode(Operation, left, right);

            switch (Operation)
            {
                case Operation.POWER:
                    return new ValueNode(Mathf.Pow(leftValue.Value, rightValue.Value));
                case Operation.MULTIPLICATION:
                    return new ValueNode(leftValue.Value * rightValue.Value);
                case Operation.DIVISION:
                    return new ValueNode(leftValue.Value / rightValue.Value);
                case Operation.ADDITION:
                    return new ValueNode(leftValue.Value + rightValue.Value);
                case Operation.SUBTRACTION:
                    return new ValueNode(leftValue.Value - rightValue.Value);
                default:
                    return null;
            }
        }

        public override Node Transform()
        {
            var left = Left.Transform();
            var right = Right.Transform();

            if (Operation == Operation.SUBTRACTION)
            {
                var negativeRight = new OperationNode(Operation.MULTIPLICATION, new ValueNode(-1), right);
                return new OperationNode(Operation.ADDITION, left, negativeRight);
            }
            else if (Operation == Operation.POWER)
            {
                var evaluatedRight = right.Evaluate();
                var valueNode = evaluatedRight as ValueNode;
                if (valueNode == null)
                    throw new ArgumentException("Power cannot contain x!");

                int valueNodeInt = Mathf.RoundToInt(valueNode.Value);
                if (valueNode.Value != valueNodeInt)
                    throw new ArgumentException("Power cannot decimal values!");

                if (valueNode.Value < 0)
                    throw new ArgumentException("Power cannot be negative!");

                if (valueNodeInt == 1)
                    return left;

                var newNode = new OperationNode(Operation.MULTIPLICATION, left, left);
                for (int i = 2; i < valueNodeInt; i++)
                    newNode = new OperationNode(Operation.MULTIPLICATION, newNode, left);

                return newNode;
            }
            else if (Operation == Operation.DIVISION)
            {
                var reciprocalRight = new OperationNode(Operation.DIVISION, new ValueNode(1), right);
                return new OperationNode(Operation.MULTIPLICATION, left, reciprocalRight);
            }

            return new OperationNode(Operation, left, right);
        }

        public override Node LimitMultiplication(float xFrom, float xTo)
        {
            var limitedLeft = Left.LimitMultiplication(xFrom, xTo);
            var limitedRight = Right.LimitMultiplication(xFrom, xTo);

            if (Operation != Operation.MULTIPLICATION || IsConstantOperation(out _, out _))
                return new OperationNode(Operation, limitedLeft, limitedRight);

            limitedLeft.FindExtremes(xFrom, xTo);
            limitedRight.FindExtremes(xFrom, xTo);

            var leftMax = limitedLeft.MaxAbsoluteValue;
            var rightMax = limitedRight.MaxAbsoluteValue;

            // return leftMax * rightMax * ((limitedLeft / leftMax) * (limitedRight / rightMax))
            return new OperationNode(Operation.MULTIPLICATION, new ValueNode(leftMax * rightMax), // leftMax * rightMax *
                new OperationNode(Operation.MULTIPLICATION, // ( * )
                    new OperationNode(Operation.MULTIPLICATION, limitedLeft, new ValueNode(1f / leftMax)), // limitedLeft / leftMax
                    new OperationNode(Operation.MULTIPLICATION, limitedRight, new ValueNode(1f / rightMax)) // limitedRight / rightMax
                    )
                );
        }

        public override Node LimitDivision(float xFrom, float xTo)
        {
            var limitedLeft = Left.LimitDivision(xFrom, xTo);
            var limitedRight = Right.LimitDivision(xFrom, xTo);

            if (Operation != Operation.DIVISION)
                return new OperationNode(Operation, limitedLeft, limitedRight);

            if ((limitedLeft as ValueNode)?.Value != 1)
                throw new Exception("Division must only be reciprocal!");

            limitedRight.FindExtremes(xFrom, xTo);

            if (limitedRight.MinAbsoluteValue == 0 || 
                (limitedRight.MinValue < 0 && limitedRight.MaxValue > 0))
                throw new Exception("Division by zero is not allowed!");

            var rightMax = limitedRight.MaxAbsoluteValue;
            // division must be limited between 0.1 and 1
            if (limitedRight.MinAbsoluteValue / rightMax < 0.1)
                throw new Exception($"Division is out of physically possible range! Divisors max value({limitedRight.MaxAbsoluteValue}) must be 10 or less times bigger than its min value({limitedRight.MinAbsoluteValue}), but its {limitedRight.MaxAbsoluteValue / limitedRight.MinAbsoluteValue} times bigger!");

            // return (1 / rightMax) * (1 / (limitedRight / rightMax))
            return new OperationNode(Operation.MULTIPLICATION, new ValueNode(1f / rightMax), // rightMax *
                new OperationNode(Operation.DIVISION, // ( / )
                    new ValueNode(1), // 1
                    new OperationNode(Operation.MULTIPLICATION, limitedRight, new ValueNode(1f / rightMax)) // limitedRight / rightMax
                    )
                );
        }

        public bool IsConstantOperation(out float value, out Node variableNode)
        {
            if (Left is ValueNode leftValueNode)
            {
                value = leftValueNode.Value;
                variableNode = Right;
                return true;
            }
            else if (Right is ValueNode rightValueNode)
            {
                value = rightValueNode.Value;
                variableNode = Left;
                return true;
            }
            else
            {
                value = 0;
                variableNode = null;
                return false;
            }
        }
    }

    public class UnaryOperationNode : Node
    {
        public UnaryOperation Operation { get; set; }

        public Node Node { get; set; }

        public UnaryOperationNode(UnaryOperation unaryOperation, Node node)
        {
            Operation = unaryOperation;
            Node = node;
        }

        public override string ToString()
        {
            return $"-{Node}";
        }

        public override Node Evaluate(float? x = null)
        {
            var node = Node.Evaluate(x);

            var valueNode = node as ValueNode;

            if (valueNode == null)
                return new UnaryOperationNode(Operation, node);
            else
                return new ValueNode(-valueNode.Value);
        }

        public override Node Transform()
        {
            var node = Node.Transform();

            if (Operation == UnaryOperation.NEGATIVE)
                return new OperationNode(MathParser.Operation.MULTIPLICATION, new ValueNode(-1), node);

            return new UnaryOperationNode(Operation, node);
        }
    }

    public enum Operation
    {
        ADDITION,
        SUBTRACTION,
        MULTIPLICATION,
        DIVISION,
        POWER
    }

    public enum UnaryOperation
    {
        NEGATIVE,
    }
}
