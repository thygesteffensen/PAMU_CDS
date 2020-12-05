using System.Linq;
using Sprache;

namespace PAMU_CDS
{
    public class OdataParser
    {
        private readonly Parser<OData> _inputParser;

        public OdataParser()
        {
            var except =
                Parse.Char('$').Or(
                    Parse.Char('=')).Or(
                    Parse.Char('(')).Or(
                    Parse.Char(')')).Or(
                    Parse.Char(','));

            var simpleString = Parse.AnyChar.Except(except).AtLeastOnce().Text();

            var oDataValues =
                from paras in (
                    from name in simpleString.Contained(Parse.Char('$'), Parse.Char('='))
                    from props in simpleString.DelimitedBy(Parse.Char(','))
                    select new ODatavalue {Name = name, Properties = props.ToArray()}
                ).DelimitedBy(Parse.Char(','))
                select paras.ToArray();

            var options =
                from vals in (
                    from optionName in simpleString
                    from paras in oDataValues.Contained(Parse.Char('('), Parse.Char(')')).Optional()
                    select new ODataOption {OptionName = optionName, Parameters = paras.IsEmpty ? null : paras.Get()}
                ).DelimitedBy(Parse.Char(','))
                select vals.ToArray();

            _inputParser = from name in simpleString.Contained(Parse.Char('$'), Parse.Char('='))
                from vals in options
                select new OData {Name = name, Values = vals};
        }

        public OData Get(string input)
        {
            return _inputParser.Parse(input);
        }
    }

    public class OData
    {
        public string Name { get; set; }
        public ODataOption[] Values { get; set; }
    }

    public class ODataOption
    {
        public string OptionName { get; set; }
        public ODatavalue[] Parameters { get; set; }
    }

    public class ODatavalue
    {
        public string Name { get; set; }
        public string[] Properties { get; set; }
    }
}