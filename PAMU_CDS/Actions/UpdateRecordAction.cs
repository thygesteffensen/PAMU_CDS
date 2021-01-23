using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using PAMU_CDS.Auxiliary;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace PAMU_CDS.Actions
{
    public class UpdateRecordAction : OpenApiConnectionActionExecutorBase
    {
        public static readonly string[] OperationId = {"UpdateRecord"};

        private readonly IOrganizationService _organizationService;

        public UpdateRecordAction(
            IExpressionEngine expressionEngine,
            OrganizationServiceContext organizationServiceContext) : base(expressionEngine)
        {
            _organizationService = organizationServiceContext?.GetOrganizationService() ??
                                   throw new ArgumentNullException(nameof(organizationServiceContext));
        }

        public override Task<ActionResult> Execute()
        {
            var result = new ActionResult();
            var entity = new Entity();
            entity = entity.CreateEntityFromParameters(Parameters);

            var entityExists = true;

            try
            {
                _organizationService.Retrieve(entity.LogicalName, entity.Id, new ColumnSet());
            }
            catch (FaultException exp)
            {
                if (exp.Message.Contains("does not exist"))
                {
                    entityExists = false;
                }
                else
                {
                    return Task.FromResult(new ActionResult
                        {ActionStatus = ActionStatus.Failed, ActionExecutorException = exp});
                }
            }
            catch (Exception exp)
            {
                return Task.FromResult(new ActionResult
                    {ActionStatus = ActionStatus.Failed, ActionExecutorException = exp});
            }

            try
            {
                if (entityExists)
                {
                    var request = new UpdateRequest // Should be upsert, but XrmMockup translates Upserts wrong...
                    {
                        Target = entity
                    };
                    _organizationService.Execute(request);

                    var retrievedEntity =
                        _organizationService.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                    
                    result.ActionOutput = retrievedEntity.ToValueContainer();
                    result.ActionStatus = ActionStatus.Succeeded;
                }
                else
                {
                    entity.Id = _organizationService.Create(entity);

                    var retrievedEntity =
                        _organizationService.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                    
                    result.ActionOutput = retrievedEntity.ToValueContainer();
                    result.ActionStatus = ActionStatus.Succeeded;
                }
            }
            catch (InvalidPluginExecutionException exp)
            {
                // We need to do some experiments on how the error handling works. Take a look at one of your customers.
                return Task.FromResult(new ActionResult
                    {ActionStatus = ActionStatus.Failed, ActionExecutorException = exp});
            }
            catch (FaultException exp)
            {
                return Task.FromResult(new ActionResult
                    {ActionStatus = ActionStatus.Failed, ActionExecutorException = exp});
            }

            return Task.FromResult(result);
        }
    }
}