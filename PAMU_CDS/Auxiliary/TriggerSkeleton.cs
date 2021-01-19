using System;
using PAMU_CDS.Enums;

namespace PAMU_CDS.Auxiliary
{
    public class TriggerSkeleton
    {
        public TriggerCondition TriggerCondition { get; set; }
        public string Table { get; set; }
        public Scope Scope { get; set; }

        public string SetTriggeringAttributes
        {
            set => GetTriggeringAttributes = value?.Split(',');
        }

        public string[] GetTriggeringAttributes { get; private set; }

        public string FilterExpression { get; set; }
        public RunAs RunAs { get; set; } = RunAs.TriggeringUser;
        public Uri FlowDescription { get; set; }
    }
}