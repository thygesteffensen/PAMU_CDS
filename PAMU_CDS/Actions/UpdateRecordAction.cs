﻿using System;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using PAMU_CDS.Auxiliary;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace PAMU_CDS.Actions
{
    public class UpdateRecordAction : OpenApiConnectionActionExecutorBase
    {
        private readonly IOrganizationService _organizationService;

        public UpdateRecordAction(ExpressionEngine expressionEngine, IOrganizationService organizationService) : base(
            expressionEngine)
        {
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        }

        public override Task<ActionResult> Execute()
        {
            var entity = new Entity();
            entity = entity.CreateEntityFromParameters(Parameters);

            try
            {
                var request = new UpsertRequest
                {
                    Target = entity
                };
                _organizationService.Execute(request);
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