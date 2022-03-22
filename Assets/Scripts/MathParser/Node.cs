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
        public virtual Node Evaluate()
        {
            return this;
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
            switch(Operation)
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

        public override Node Evaluate()
        {
            var left = Left.Evaluate();
            var right = Right.Evaluate();

            var leftValue = left as ValueNode;
            var rightValue = right as ValueNode;

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

        public override Node Evaluate()
        {
            var node = Node.Evaluate();

            var valueNode = node as ValueNode;

            if (valueNode == null)
                return new UnaryOperationNode(Operation, node);
            else
                return new ValueNode(-valueNode.Value);
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
