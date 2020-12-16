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

        private static readonly Parser<IOdata> SimpleString =
            Parse.AnyChar.Except(Except).AtLeastOnce().Text().Select(x => new Constant(x));

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

        private static readonly Parser<IOdata> Func =
            from funcName in SimpleString
            from paras in SimpleString.Contained(OpenP, CloseP)
            select new Function(funcName, paras);

        private static readonly Parser<IOdata> Value =
            Func.Or(SimpleString.Contained(Quote, Quote));

        private static readonly Parser<IOdata> Stm =
            Func.Or(Parse.Ref(() => OrGroup).Contained(OpenP, CloseP)).Or(
                from attr in SimpleString
                from op in Operators.Contained(Space, Space)
                from val in Value
                select new Statement((Constant) attr, op, val)
            );

        private static readonly Parser<IOdata> AndGroup =
            from stmt in Stm
            from groups in (
                from o in And
                from a1 in Stm
                select new Group(o, a1)).Many()
            select new Group(stmt, groups);

        private static readonly Parser<IOdata> OrGroup =
            from andGroup in AndGroup
            from groups in (
                from o in Or
                from a1 in AndGroup
                select new Group(o, a1)).Many()
            select new Group(andGroup, groups);

        public FilterExpression OdataToFilterExpression(string input)
        {
            var t = (Group) OrGroup.Parse(input);


            return null;
        }
    }

    public interface IOdata
    {
    };

    public class Statement : IOdata
    {
        private readonly Constant _attr;
        private readonly ConditionOperator _op;
        private readonly IOdata _val;

        public Statement(Constant attr, ConditionOperator op, IOdata val)
        {
            _attr = attr;
            _op = op;
            _val = val;
        }

        public ConditionExpression ToConditionExpression()
        {
            return new ConditionExpression(_attr.Value(), _op, _val);
        }
    }

    public class Function : IOdata
    {
        private readonly IOdata _funcName;
        private readonly IOdata _paras;

        public Function(IOdata funcName, IOdata paras)
        {
            _funcName = funcName;
            _paras = paras;
        }
    }

    public class Group : IOdata
    {
        public LogicalOperator LogicalOperator { get; }
        public readonly IOdata Grp;
        public readonly IOdata Stmt;
        public readonly IEnumerable<Group> SameGrp;

        public Group(LogicalOperator logicalOperator, IOdata grp)
        {
            LogicalOperator = logicalOperator;
            Grp = grp;
        }

        public Group(IOdata stmt, IEnumerable<Group> sameGrp)
        {
            Stmt = stmt;
            SameGrp = sameGrp;
        }

        public FilterExpression ToFilerExpression()
        {
            var filter = new FilterExpression()
            {
                FilterOperator = LogicalOperator,
                Conditions =
                {
                    Stmt.ToConditionExpression()
                },
            };

            foreach (var group in SameGrp)
            {
                filter.Filters.Add(group.ToFilerExpression());
            }

            return filter;
        }
    }

    public class Constant : IOdata
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