using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MathParser
{
    internal enum Operator
    {
        LEFT_BRACKET, RIGHT_BRACKET, PLUS, MINUS, TIMES, DIVIDED_BY, POWER, UNARY_MINUS, UNARY_PLUS
    }

    public class Parser
    {
        public static Node Parse(string expression)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < expression.Length; i++)
            {
                if (expression[i] == ' ')
                    continue;

                stringBuilder.Append(expression[i]);
            }

            var sanitizedExpression = stringBuilder.ToString();

            var parser = new Parser(sanitizedExpression);

            var topNode = parser.ParseExpression();

            // simplify (ex. x * 2 * 2 = x * 4)
            return topNode.Evaluate();
        }

        private Parser(string expression)
        {
            this.expression = expression;
        }

        private string expression;
        private int index = 0;

        // E -> T+E | T-E | T
        private Node ParseExpression()
        {
            var left = ParseTerm();

            if (Match("-"))
            {
                var right = ParseExpression();
                return new OperationNode(Operation.SUBTRACTION, left, right);
            }

            if (Match("+"))
            {
                var right = ParseExpression();
                return new OperationNode(Operation.ADDITION, left, right);
            }

            return left;
        }

        // T -> P*T | P/T | P
        private Node ParseTerm()
        {
            var left = ParsePower();

            if (Match("*"))
            {
                var right = ParseTerm();

                return new OperationNode(Operation.MULTIPLICATION, left, right);
            }

            if (Match("/"))
            {
                var right = ParseTerm();

                return new OperationNode(Operation.DIVISION, left, right);
            }

            return left;
        }

        // P -> F^P | F
        private Node ParsePower()
        {
            var left = ParseFactor();

            if (Match("^"))
            {
                var right = ParsePower();

                return new OperationNode(Operation.POWER, left, right);
            }

            return left;
        }

        // F -> -F | +F | (E) | a
        private Node ParseFactor()
        {
            var next = GetNext(true);

            if (next.Value == "-")
            {
                var node = ParseFactor();
                return new UnaryOperationNode(UnaryOperation.NEGATIVE, node);
            }

            if (next.Value == "+")
            {
                var node = ParseFactor();
                return node;
            }

            if (next.Value == "(")
            {
                var node = ParseExpression();
                if (!Match(")"))
                    throw new SyntaxException("Missing closing brackets!");

                return node;
            }

            if (next.Type == UnitType.ARGUMENT)
            {
                if (next.Value == "x")
                    return new InputNode();
                else
                    return new ValueNode(next.Value);
            }


            throw new SyntaxException("Expected +, -, ( or argument!");
        }

        private bool Match(string unit)
        {
            if (index >= expression.Length)
                return false;

            var next = GetNext();
            if (unit == next.Value)
            {
                GetNext(true);
                return true;
            }

            return false;
        }

        private Unit GetNext(bool consume = false)
        {
            var regex = new Regex(@"\(|\)|[0-9]+(\.[0-9]*)?|x|\+|\-|\*|\/|\^");

            var match = regex.Match(expression, index);

            if (match.Success == false || match.Index != index)
                throw new LexicalErrorException(expression);

            if (consume)
                index = match.Index + match.Length;

            UnitType unitType;
            if (char.IsNumber(match.Value[0]) || match.Value == "x")
                unitType = UnitType.ARGUMENT;
            else
                unitType = UnitType.OPERATOR;

            return new Unit()
            {
                Type = unitType,
                Value = match.Value
            };
        }

        private class Unit
        {
            public string Value { get; set; }
            public UnitType Type { get; set; }
        }

        private enum UnitType
        {
            ARGUMENT,
            OPERATOR
        }
    }

    public class LexicalErrorException : Exception
    {
        public string Expression { get; set; }
        public int Offset { get; set; }

        public LexicalErrorException(string expression) : base("Uknown character at " + expression)
        {
            Expression = expression;
        }
    }

    public class SyntaxException : Exception
    {
        public SyntaxException(string message) : base(message)
        {

        }
    }
}
