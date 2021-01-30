using System;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PAMU_CDS.Auxiliary;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace PAMU_CDS.Actions
{
    public class CreateRecordAction : OpenApiConnectionActionExecutorBase
    {
        public static readonly string[] OperationId = {"CreateRecord"};

        private readonly IOrganizationService _organizationService;

        public CreateRecordAction(
            IExpressionEngine expressionEngine,
            OrganizationServiceContext organizationServiceContext) : base(
            expressionEngine)
        {
            _organizationService = organizationServiceContext?.GetOrganizationService() ??
                                   throw new ArgumentNullException(nameof(organizationServiceContext));
        }

        public override Task<ActionResult> Execute()
        {
            var result = new ActionResult();
            var entity = new Entity();
            entity = entity.CreateEntityFromParameters(Parameters);

            try
            {
                entity.Id = _organizationService.Create(entity);

                var retrievedEntity = _organizationService.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                result.ActionOutput = retrievedEntity.ToValueContainer();
                result.ActionStatus = ActionStatus.Succeeded;
            }
            catch (InvalidPluginExecutionException exp)
            {
                // We need to do some experiments on how the error handling works. Take a look at one of your customers.
                return Task.FromResult(new ActionResult
                    {ActionStatus = ActionStatus.Failed, ActionExecutorException = exp});
            }

            return Task.FromResult(result);
        }
    }
}