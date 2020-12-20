using System;
using Microsoft.Xrm.Sdk.Query;
using Parser.ExpressionParser.Functions.CustomException;
using Sprache;

namespace PAMU_CDS.Auxiliary
{
    public class OdataFilter
    {
        private static readonly Parser<char> Except =
            Parse.Char('$').Or(
                Parse.Char('=')).Or(
                Parse.Char('\'')).Or(
                Parse.Char('(')).Or(
                Parse.Char(')')).Or(
                Parse.Char(';')).Or(
                Parse.Char(' ')).Or(
                Parse.Char('\'')).Or(
                Parse.Char(','));

        private static readonly Parser<string> SimpleString =
            Parse.AnyChar.Except(Except).AtLeastOnce().Text().Select(x => x);

        private static readonly Parser<decimal> Decimal =
            (
                from n in Parse.Number
                from di in Parse.Char(',').Or(Parse.Char('.'))
                from dec in Parse.Number
                select decimal.Parse($"{n},{dec}"))
            .Or(
                Parse.Number.Select(decimal.Parse));

        private static readonly Parser<bool> Bool =
            Parse.String("true").Select(x => true)
                .Or(Parse.String("false").Select(x => false));

        private static readonly Parser<object> Null =
            Parse.String("null").Select(x => (object) null);

        private static readonly Parser<ConditionOperator> Equal = Parse.String("eq").Return(ConditionOperator.Equal);

        private static readonly Parser<ConditionOperator> LessThan =
            Parse.String("lt").Return(ConditionOperator.LessThan);

        private static readonly Parser<ConditionOperator> GreaterThan =
            Parse.String("gt").Return(ConditionOperator.GreaterThan);

        private static readonly Parser<ConditionOperator> GreaterOrEqual =
            Parse.String("ge").Return(ConditionOperator.GreaterEqual);

        private static readonly Parser<ConditionOperator> LessOrEqual =
            Parse.String("le").Return(ConditionOperator.LessEqual);

        private static readonly Parser<ConditionOperator> NotEqual =
            Parse.String("ne").Return(ConditionOperator.NotEqual);

        private static readonly Parser<ConditionOperator> Operators =
            Equal.Or(LessThan).Or(GreaterThan).Or(GreaterOrEqual).Or(LessOrEqual).Or(NotEqual);

        private static readonly Parser<LogicalOperator> And = Parse.String("and").Return(LogicalOperator.And);
        private static readonly Parser<LogicalOperator> Or = Parse.String("or").Return(LogicalOperator.Or);

        private static readonly Parser<LogicalOperator> LOperators = And.Or(Or);

        private static readonly Parser<char> Space = Parse.Char(' ');
        private static readonly Parser<char> Quote = Parse.Char('\'');
        private static readonly Parser<char> OpenP = Parse.Char('(');
        private static readonly Parser<char> CloseP = Parse.Char(')');

        private static readonly Parser<INode> Func =
            from function in SimpleString.Token()
            from op in OpenP.Token()
            from attr in SimpleString.Token()
            from comma in Parse.Char(',').Token()
            from value in SimpleString.Contained(Quote, Quote).Token()
            from cp in CloseP.Token()
            select new Function(function, attr, value); 

        private static readonly Parser<INode> Stm =
            Parse.Ref(() => AndGroup).Contained(OpenP, CloseP)
                .Or(Func)
                .Or
                (from attr in SimpleString
                    from op in Operators.Contained(Space, Space)
                    from val in Null
                    select new Statement(attr, op, null))
                .Or
                (from attr in SimpleString
                    from op in Operators.Contained(Space, Space)
                    from val in SimpleString.Contained(Quote, Quote)
                    select new Statement(attr, op, val))
                .Or
                (from attr in SimpleString
                    from op in Operators.Contained(Space, Space)
                    from val in Bool
                    select new Statement(attr, op, val))
                .Or
                (from attr in SimpleString
                    from op in Operators.Contained(Space, Space)
                    from val in Decimal
                    select new Statement(attr, op, val));


        private static readonly Parser<INode> AndGroup =
            from n in (
                from stmt in Stm
                from and in LOperators.Contained(Space, Space)
                from n1 in AndGroup // Watch out for this
                select new Branch(stmt, and, n1)
            ).Or(
                from stmt in Stm
                select stmt)
            select n;


        public FilterExpression OdataToFilterExpression(string input)
        {
            switch (AndGroup.Parse(input))
            {
                case Leaf l:
                    return new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions = {l.ToCondition()}
                    };
                case Branch b:
                    return FilterExpression(b);
                default:
                    return null;
            }
        }

        private static FilterExpression FilterExpression(Branch b)
        {
            var filter = new FilterExpression {FilterOperator = b.Operator};

            switch (b.RightSide)
            {
                case Branch br when br.Operator == b.Operator:
                    AddBranchToConditions(filter, br);
                    break;
                case Branch br:
                    filter.Filters.Add(FilterExpression(br));
                    break;
                case Leaf lr:
                    filter.Conditions.Add(lr.ToCondition());
                    break;
            }

            switch (b.LeftSide)
            {
                case Branch bl when bl.Operator == b.Operator:
                    AddBranchToConditions(filter, bl);
                    break;
                case Branch bl:
                    filter.Filters.Add(FilterExpression(bl));
                    break;
                case Leaf ll:
                    filter.Conditions.Add(ll.ToCondition());
                    break;
            }

            return filter;
        }

        private static void AddBranchToConditions(FilterExpression filter, Branch b)
        {
            if (b.Operator == filter.FilterOperator)
            {
                switch (b.LeftSide)
                {
                    case Leaf ll:
                        filter.Conditions.Add(ll.ToCondition());
                        break;
                    case Branch bl:
                        AddBranchToConditions(filter, bl);
                        break;
                }

                switch (b.LeftSide)
                {
                    case Leaf lr:
                        filter.Conditions.Add(lr.ToCondition());
                        break;
                    case Branch br:
                        AddBranchToConditions(filter, br);
                        break;
                }
            }
            else
            {
                filter.Filters.Add(FilterExpression(b));
            }
        }
    }

    public interface INode
    {
    }

    public class Branch : INode
    {
        public INode LeftSide { get; }
        public LogicalOperator Operator { get; }
        public INode RightSide { get; }

        public Branch(INode leftSide, LogicalOperator op, INode rightSide)
        {
            LeftSide = leftSide;
            Operator = op;
            RightSide = rightSide;
        }
    }

    public abstract class Leaf : INode
    {
        public abstract ConditionExpression ToCondition();
    }

    public class Statement : Leaf
    {
        private string Attribute { get; }
        private ConditionOperator Op { get; }
        private dynamic Value { get; }

        public Statement(string attribute, ConditionOperator op, string value)
        {
            Attribute = attribute;
            Op = op;
            Value = value;
        }

        public Statement(string attribute, ConditionOperator op, bool value)
        {
            Attribute = attribute;
            Op = op;
            Value = value;
        }

        public Statement(string attribute, ConditionOperator op, decimal value)
        {
            Attribute = attribute;
            Op = op;

            if (value % 1 == 0)
            {
                Value = (int) value;
            }
            else
            {
                Value = value;
            }
        }

        public override ConditionExpression ToCondition()
        {
            if (Value == null)
            {
                return new ConditionExpression(Attribute, Op == ConditionOperator.Equal
                    ? ConditionOperator.Null
                    : ConditionOperator.NotNull);
            }

            return new ConditionExpression(Attribute, Op, Value);
        }
    }

    public class Function : Leaf
    {
        private readonly string _function;
        private readonly string _attribute;
        private readonly string _value;

        public Function(string function, string attribute, string value)
        {
            _function = function;
            _attribute = attribute;
            _value = value;
        }
        public override ConditionExpression ToCondition()
        {
            return _function switch
            {
                "startswith" => new ConditionExpression(_attribute, ConditionOperator.BeginsWith, _value),
                "endswith" => new ConditionExpression(_attribute, ConditionOperator.EndsWith, _value),
                "substringof" => new ConditionExpression(_attribute, ConditionOperator.Contains, _value),
                _ => throw new NotImplementedException($"{_function} is not yet supported...")
            };
        }
    }
}