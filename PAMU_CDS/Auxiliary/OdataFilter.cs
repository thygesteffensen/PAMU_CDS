using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Sprache;

namespace PAMU_CDS.Auxiliary
{
    public class OdataFilter
    {
        Parser<ParsedFilter> cond1;

        private static readonly Parser<char> Except =
            Parse.Char('$').Or(
                Parse.Char('=')).Or(
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
        

        private static readonly Parser<Node> Stm =
            from attr in SimpleString
            from op in Operators.Contained(Space, Space)
            from val in SimpleString.Contained(Quote, Quote)
            select new Statement(attr, op, val);
        
        private static readonly Parser<Node> AndGroup =
            from n in (
                from stmt in Stm
                from and in And.Or(Or).Contained(Space, Space)
                from n1 in AndGroup // Watch out for this
                select new Branch((Leaf) stmt, and, n1)
            ).Or(
                from stmt in Stm
                select stmt)
            select n;

        public FilterExpression OdataToFilterExpression(string input)
        {
            var t = AndGroup.Parse(input);


            return null;
        }
    }

    public interface Node
    {
    };

    public abstract class Leaf : Node
    {
    }

    public class Statement : Leaf
    {
        public string Attribute { get; private set; }
        public ConditionOperator Op { get; private set; }
        public string Value { get; private set; }

        public Statement(string attribute, ConditionOperator op, string value)
        {
            Attribute = attribute;
            Op = op;
            Value = value;
        }
    }

    public class Branch : Node
    {
        public Leaf LeftSide { get; set; }
        public LogicalOperator Operator { get; set; }
        public Node RightSide { get; set; }

        public Branch(Leaf leftSide, LogicalOperator op, Node rightSide)
        {
            LeftSide = leftSide;
            Operator = op;
            RightSide = rightSide;
        }

        // public FilterExpression ToFilerExpression()
        // {
        // }
    }

    public class Constant : Node
    {
        private readonly string _s;

        public Constant(string s)
        {
            _s = s;
        }

        public string Value()
        {
            return _s;
        }
    }

    public class ParsedFilter
    {
        public ConditionExpression ConditionExpression { get; }
        public IEnumerable<Additional> Additional { get; }

        public ParsedFilter(ConditionExpression conditionExpression, IEnumerable<Additional> additional)
        {
            ConditionExpression = conditionExpression;
            Additional = additional;
        }
    }

    public class Additional
    {
        public LogicalOperator Op { get; }
        public ConditionExpression ConditionExpression { get; }

        public Additional(LogicalOperator op, ConditionExpression conditionExpression)
        {
            Op = op;
            ConditionExpression = conditionExpression;
        }
    }
}