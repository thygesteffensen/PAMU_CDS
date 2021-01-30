using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using PAMU_CDS.Enums;

namespace PAMU_CDS.Auxiliary
{
    public static class TriggerParser
    {
        public static void AddTo(this List<TriggerSkeleton> list, string flowDefinitionPath)
        {
            var flowJson = JToken.Parse(File.ReadAllText(flowDefinitionPath));

            var triggerJson = flowJson.SelectToken("$..triggers");
            if (triggerJson == null ||
                triggerJson.SelectToken("$..apiId")?.Value<string>() !=
                "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps")
                return;

            var trigger = new TriggerSkeleton
            {
                FlowName = Path.GetFileName(flowDefinitionPath),
                TriggerCondition =
                    ToCondition(triggerJson.SelectToken("$..subscriptionRequest/message")),
                Table = triggerJson.SelectToken("$..subscriptionRequest/entityname").Value<string>(),
                Scope = ToScope(triggerJson.SelectToken("$..subscriptionRequest/scope")),
                SetTriggeringAttributes =
                    triggerJson.SelectToken("$..subscriptionRequest/filteringattributes")?.Value<string>(),
                FilterExpression = triggerJson.SelectToken("$..subscriptionRequest/filterexpression")?.ToString(),
                RunAs = ToRunAs(triggerJson.SelectToken("$..subscriptionRequest/runas")),
                FlowDescription = new Uri(flowDefinitionPath),
            };
            list.Add(trigger);
        }

        private static RunAs ToRunAs(JToken selectToken)
        {
            return selectToken?.Value<int>() switch
            {
                1 => RunAs.TriggeringUser,
                2 => RunAs.RecordOwner,
                3 => RunAs.ProcessOwner,
                _ => RunAs.ProcessOwner
            };
        }

        private static Scope ToScope(JToken selectToken)
        {
            return selectToken.Value<int>() switch
            {
                1 => Scope.User,
                2 => Scope.BusinessUnit,
                3 => Scope.ParentChildBusinessUnit,
                4 => Scope.Organization,
                _ => throw new Exception("Scope enum value is out of range.")
            };
        }

        private static TriggerCondition ToCondition(JToken jToken)
        {
            return jToken.Value<int>() switch
            {
                1 => TriggerCondition.Create,
                2 => TriggerCondition.Delete,
                3 => TriggerCondition.Update,
                4 => TriggerCondition.CreateUpdate,
                5 => TriggerCondition.CreateDelete,
                6 => TriggerCondition.UpdateDelete,
                7 => TriggerCondition.CreateUpdateDelete,
                _ => throw new Exception("TriggerCondition enum value is out of range.")
            };
        }
    }
}