using System.Threading.Tasks;
using Parser.FlowParser.ActionExecutors;

namespace PAMU_CDS.Actions
{
    public class CdsTrigger : DefaultBaseActionExecutor
    {
        public override Task<ActionResult> Execute()
        {
            return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
        }
    }
}