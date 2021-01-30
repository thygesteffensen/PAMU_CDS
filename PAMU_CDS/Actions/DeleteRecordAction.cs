using System;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using PAMU_CDS.Auxiliary;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace PAMU_CDS.Actions
{
    public class DeleteRecordAction : OpenApiConnectionActionExecutorBase
    {
        public static readonly string[] OperationId = {"CreateRecord"};
        
        private readonly IOrganizationService _organizationService;

        public DeleteRecordAction(
            IExpressionEngine expressionEngine, 
            OrganizationServiceContext organizationServiceContext) : base(expressionEngine)
        {
            _organizationService = organizationServiceContext?.GetOrganizationService() ?? 
                                   throw new ArgumentNullException(nameof(organizationServiceContext));
        }

        public override Task<ActionResult> Execute()
        {
            var entity = new Entity();
            entity = entity.CreateEntityFromParameters(Parameters);

            try
            {
                _organizationService.Delete(entity.LogicalName, entity.Id);
            }
            catch (InvalidPluginExecutionException exp)
            {
                // We need to do some experiments on how the error handling works. Take a look at one of your customers.
                return Task.FromResult(new ActionResult
                    {ActionStatus = ActionStatus.Failed, ActionExecutorException = exp});
            }

            return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
        }
    }
}