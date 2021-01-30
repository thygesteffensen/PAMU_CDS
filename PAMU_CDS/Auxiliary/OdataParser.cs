using System.Linq;
using Sprache;

namespace PAMU_CDS.Auxiliary
{
    public class OdataParser
    {
        private readonly Parser<Value[]> _values;

        public OdataParser()
        {
            var except =
                Parse.Char('$').Or(
                    Parse.Char('=')).Or(
                    Parse.Char('(')).Or(
                    Parse.Char(')')).Or(
                    Parse.Char(';')).Or(
                    Parse.Char(','));

            var simpleString = Parse.AnyChar.Except(except).AtLeastOnce().Text();
            var functionString = Parse.AnyChar.Except(Parse.Char('(').Or(Parse.Char(')'))).AtLeastOnce().Text();
            
            var function =
                from functionName in simpleString
                from functionParameter in functionString.Contained(Parse.Char('('), Parse.Char(')'))
                select new[] {$"{functionName}({functionParameter})"};

            var parameters =
                from paras in (
                    from name in simpleString.Contained(Parse.Char('$'), Parse.Char('='))
                    from props in function.Or(simpleString.DelimitedBy(Parse.Char(',')))
                    select new ODataValue {Name = name, Properties = props.ToArray()}
                ).DelimitedBy(Parse.Char(';'))
                select paras.ToArray();

            _values = from values in (
                    from optionName in simpleString
                    from paras in parameters.Contained(Parse.Char('('), Parse.Char(')')).Optional()
                    select new Value {Option = optionName, Parameters = paras.IsEmpty ? null : paras.Get()}
                ).DelimitedBy(Parse.Char(','))
                select values.ToArray();
        }

        public Value[] Get(string input)
        {
            return _values.Parse(input);
        }
    }

    public class Value
    {
        public string Option { get; set; }
        public ODataValue[] Parameters { get; set; }
    }

    public class ODataValue
    {
        public string Name { get; set; }
        public string[] Properties { get; set; }
    }
}