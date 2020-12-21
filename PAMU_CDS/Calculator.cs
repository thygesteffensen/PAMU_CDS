using System.Linq;
using Sprache;

namespace PAMU_CDS
{
    public class Calculator
    {
        private readonly Parser<int> _expr;

        public Calculator()
        {
            var add = Parse.Char('+');
            var mult = Parse.Char('*');

            var factor = Parse.Number.Select(int.Parse);

            var term =
                (from n1 in factor
                    from m in
                        (from list in (
                                from op in mult
                                from n2 in factor
                                select n2).AtLeastOnce()
                            select list.Aggregate((acc, x) => acc * x)).Optional()
                    select m.IsEmpty ? n1 : n1 * m.Get()).Or(factor);

            _expr =
                (from t1 in term
                    from m in (from list in (
                            from op in add
                            from t2 in term
                            select t2).AtLeastOnce()
                        select list.Aggregate((acc, x) => acc + x)).Optional()
                    select m.IsEmpty ? t1 : t1 + m.Get()).Or(term);
        }

        public int Calculate(string s)
        {
            return _expr.Parse(s);
        }
    }
}