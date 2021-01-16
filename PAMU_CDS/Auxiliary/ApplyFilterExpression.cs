using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PAMU_CDS.Auxiliary
{
    public static class ApplyFilterExpression
    {
        public static bool ApplyFilterExpressionToEntity(Entity entity, FilterExpression filterExpression)
        {
            var andGroup = filterExpression.FilterOperator == LogicalOperator.And;

            foreach (var filterExpressionCondition in filterExpression.Conditions)
            {
                if (!entity.Attributes.TryGetValue(filterExpressionCondition.AttributeName, out var value))
                {
                    return false;
                }

                bool boolToReturn;

                switch (filterExpressionCondition.Operator)
                {
                    case ConditionOperator.Equal:
                        if (
                            IsDecidingCondition(andGroup,
                                value.Equals(filterExpressionCondition.Values.First()),
                                out boolToReturn)
                        ) return boolToReturn;
                        break;
                    case ConditionOperator.NotEqual:
                        if (
                            IsDecidingCondition(andGroup,
                                !value.Equals(filterExpressionCondition.Values.First()),
                                out boolToReturn)
                        ) return boolToReturn;
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        private static bool IsDecidingCondition(bool andGroup, bool conditionResult,
            out bool boolToReturn)
        {
            if (andGroup)
            {
                if (!conditionResult)
                {
                    {
                        boolToReturn = false;
                        return true;
                    }
                }
            }
            else
            {
                if (conditionResult)
                {
                    {
                        boolToReturn = true;
                        return true;
                    }
                }
            }

            boolToReturn = false;
            return false;
        }
    }
}