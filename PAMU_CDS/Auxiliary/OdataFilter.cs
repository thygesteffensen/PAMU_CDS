using Microsoft.Xrm.Sdk.Query;
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


        private static readonly Parser<INode> Stm =
            Parse.Ref(() => AndGroup).Contained(OpenP, CloseP)
                .Or
                (from attr in SimpleString
                    from op in Operators.Contained(Space, Space)
                    from val in SimpleString.Contained(Quote, Quote)
                    select new Statement(attr, op, val));


        private static readonly Parser<INode> AndGroup =
            from n in (
                from stmt in Stm
                from and in And.Or(Or).Contained(Space, Space)
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
            var filter = new FilterExpression();
            filter.FilterOperator = b.Operator;

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
        private string Value { get; }

        public Statement(string attribute, ConditionOperator op, string value)
        {
            Attribute = attribute;
            Op = op;
            Value = value;
        }

        public override ConditionExpression ToCondition()
        {
            return new ConditionExpression(Attribute, Op, Value);
        }
    }
}