﻿using System;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PAMU_CDS.Auxiliary;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace PAMU_CDS.Actions
{
    public class AssociateEntitiesAction : OpenApiConnectionActionExecutorBase
    {
        public const string OperationId = "AssociateEntitiesAction";

        private readonly IOrganizationService _organizationService;
        private readonly IState _state;

        public AssociateEntitiesAction(
            IExpressionEngine expressionEngine, 
            IOrganizationService organizationService,
            IState state) : base(
            expressionEngine)
        {
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }


        public override Task<ActionResult> Execute()
        {

            var entity = new Entity();
            entity = entity.CreateEntityFromParameters(Parameters);

            try
            {
                entity.Id = _organizationService.Create(entity);
                
                var retrievedEntity = _organizationService.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                _state.AddOutputs(ActionName, retrievedEntity.ToValueContainer());
            }
            catch (InvalidPluginExecutionException)
            {
                // We need to do some experiments on how the error handling works. Take a look at one of your customers.
                return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Failed});
            }

            return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
        }
    }
}