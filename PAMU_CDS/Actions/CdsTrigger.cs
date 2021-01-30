using System.Threading.Tasks;
using Parser.FlowParser.ActionExecutors;

namespace PAMU_CDS.Actions
{
    public class CdsTrigger : DefaultBaseActionExecutor
    {
        public static readonly string[] OperationId = {"SubscribeWebhookTrigger"};

        public override Task<ActionResult> Execute()
        {
            return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
        }
    }
}