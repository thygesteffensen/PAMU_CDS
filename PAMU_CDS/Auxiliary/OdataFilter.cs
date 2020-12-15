using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Query;
using Sprache;

namespace PAMU_CDS.Auxiliary
{
    public class OdataFilter
    {
        Parser<ConditionExpression> stm;
        Parser<Temp1> cond;

        public OdataFilter()
        {
            var except =
                Parse.Char('$').Or(
                    Parse.Char('=')).Or(
                    Parse.Char('(')).Or(
                    Parse.Char(')')).Or(
                    Parse.Char(';')).Or(
                    Parse.Char(' ')).Or(
                    Parse.Char('\'')).Or(
                    Parse.Char(','));

            var simpleString = Parse.AnyChar.Except(except).AtLeastOnce().Text();

            var equal = Parse.String("eq").Return(ConditionOperator.Equal);
            var lessThan = Parse.String("lt").Return(ConditionOperator.LessThan);
            var greaterThan = Parse.String("gt").Return(ConditionOperator.GreaterThan);
            var greaterOrEqual = Parse.String("ge").Return(ConditionOperator.GreaterEqual);
            var lessOrEqual = Parse.String("le").Return(ConditionOperator.LessEqual);
            var notEqual = Parse.String("ne").Return(ConditionOperator.NotEqual);

            var operators = equal.Or(lessThan).Or(greaterThan).Or(greaterOrEqual).Or(lessOrEqual).Or(notEqual);

            var and = Parse.String("and").Return(LogicalOperator.And);
            var or = Parse.String("or").Return(LogicalOperator.Or);

            var lOperators = and.Or(or);

            var str = Parse.AnyChar.AtLeastOnce().Text();

            var space = Parse.Char(' ');
            var quote = Parse.Char('\'');

            stm = from attr in simpleString
                from op in operators.Contained(space, space)
                from val in simpleString.Contained(quote, quote)
                select new ConditionExpression(attr, op, val);

            cond = from t in stm
                from dep in (
                    from op in lOperators
                    from t1 in stm
                    select new Temp(op, t1)).Many()
                select new Temp1(t, dep);
        }

        public ConditionExpression ParseInput(string input)
        {
            return stm.Parse(input);
        }

        public FilterExpression ParseToFiler(string input)
        {


            return null;
        }

        public void T()
        {
            var exp = new FilterExpression()
            {
                FilterOperator = LogicalOperator.And,
                Conditions =
                {
                   new ConditionExpression(), 
                   new ConditionExpression(),
                   
                }
            };
            
            var q = new QueryExpression()
            {
                
            };
            q.Criteria.AddFilter(new FilterExpression());
            q.Criteria.AddFilter(new FilterExpression());

            var f = new ConditionExpression();
        }
    }

    public class Temp1
    {
        public Temp1(ConditionExpression conditionExpression, IEnumerable<Temp> dep)
        {
        }
    }

    public class Temp
    {
        public Temp(LogicalOperator op, ConditionExpression t1)
        {
            
        }
    }
}