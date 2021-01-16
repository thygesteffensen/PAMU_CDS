using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IXrmMockupExtension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using PAMU_CDS.Actions;
using PAMU_CDS.Auxiliary;
using PAMU_CDS.Enums;
using Parser;
using Parser.ExpressionParser.Functions.Base;
using Parser.FlowParser;
using Parser.FlowParser.ActionExecutors.Implementations;

namespace PAMU_CDS
{
    public class CommonDataServiceCurrentEnvironment : IMockUpExtension
    {
        private readonly List<TriggerSkeleton> _triggers;
        public ServiceCollection Services { get; }


        public CommonDataServiceCurrentEnvironment(Uri flowFolderPath)
        {
            var files = Directory.GetFiles(flowFolderPath.AbsolutePath);

            _triggers = new List<TriggerSkeleton>();

            foreach (var file in files)
            {
                _triggers.AddTo(file);
            }

            Services = new ServiceCollection();
        }

        public void TriggerExtension(
            IOrganizationService organizationService,
            OrganizationRequest request,
            Entity currentEntity,
            Entity preEntity,
            EntityReference userRef)
        {
            var triggerObject = TriggerExtensionAsync(organizationService, request, currentEntity, preEntity, userRef);
            triggerObject.Wait();
        }

        public async Task TriggerExtensionAsync(
            IOrganizationService organizationService,
            OrganizationRequest request,
            Entity currentEntity,
            Entity preEntity,
            EntityReference userRef)
        {
            if (!new[] {"Create", "Delete", "Update"}.Contains(request.RequestName))
            {
                throw new InvalidOperationException("PAMU_CDS does not support the request.");
            }

            var flows = ApplyCriteria(request, preEntity ?? currentEntity);

            var sp = BuildServiceCollection(organizationService).BuildServiceProvider();

            // var flowRunner = sp.GetRequiredService<FlowRunner>();

            foreach (var triggerSkeleton in flows)
            {
                using var scope = sp.CreateScope();
                var isp = scope.ServiceProvider;
                var state = sp.GetRequiredService<IState>();

                state.AddTriggerOutputs(currentEntity.ToValueContainer());

                var flowRunner = sp.GetRequiredService<FlowRunner>();
                flowRunner.InitializeFlowRunner(triggerSkeleton.FlowDescription.AbsolutePath);
                await flowRunner.Trigger();
            }
        }

        private IEnumerable<TriggerSkeleton> ApplyCriteria(OrganizationRequest request, Entity entity)
        {
            IEnumerable<TriggerSkeleton> flows;
            if (request.RequestName == "Delete")
            {
                var target = (EntityReference) request.Parameters["Target"];
                flows = _triggers.Where(x =>
                    x.Table == target.LogicalName &&
                    x.TriggerCondition == TriggerCondition.Delete ||
                    x.TriggerCondition == TriggerCondition.CreateDelete ||
                    x.TriggerCondition == TriggerCondition.UpdateDelete ||
                    x.TriggerCondition == TriggerCondition.CreateUpdateDelete);
            }
            else
            {
                var target = (Entity) request.Parameters["Target"];
                flows = _triggers.Where(x => x.Table == target.LogicalName);
                if (request.RequestName == "Create")
                {
                    flows = flows.Where(x =>
                        x.TriggerCondition == TriggerCondition.Create ||
                        x.TriggerCondition == TriggerCondition.CreateDelete ||
                        x.TriggerCondition == TriggerCondition.CreateUpdate ||
                        x.TriggerCondition == TriggerCondition.CreateUpdateDelete);
                }
                else
                {
                    flows = flows.Where(x =>
                        x.TriggerCondition == TriggerCondition.Update ||
                        x.TriggerCondition == TriggerCondition.UpdateDelete ||
                        x.TriggerCondition == TriggerCondition.CreateUpdate ||
                        x.TriggerCondition == TriggerCondition.CreateUpdateDelete);
                }

                flows = flows.Where(x =>
                    x.GetTriggeringAttributes == null ||
                    x.GetTriggeringAttributes.Length == 0 ||
                    x.GetTriggeringAttributes.Any(y => target.Attributes.Keys.Contains(y)));
            }

            var t = new OdataFilter();
            flows = flows.Where(x => FulfillFilterExpression(x.FilterExpression, entity, t));

            return flows;
        }

        private bool FulfillFilterExpression(string filterExpression, Entity entity, OdataFilter odataFilter)
        {
            if (filterExpression == null) return true;
            var filterExpresion = odataFilter.OdataToFilterExpression(filterExpression);
            return ApplyFilterExpression.ApplyFilterExpressionToEntity(entity, filterExpresion);
        }

        private ServiceCollection BuildServiceCollection(IOrganizationService organizationService)
        {
            const string apiId = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps";

            Services.AddFlowRunner();

            Services.AddSingleton(organizationService);

            Services.Configure<FlowSettings>(x => { });

            Services.AddFlowActionByApiIdAndOperationsName<CdsTrigger>(apiId, CdsTrigger.OperationId);

            Services.AddFlowActionByApiIdAndOperationsName<CreateRecordAction>(apiId, CreateRecordAction.OperationId);

            Services.AddFlowActionByApiIdAndOperationsName<UpdateRecordAction>(apiId, UpdateRecordAction.OperationId);

            Services.AddFlowActionByApiIdAndOperationsName<DeleteRecordAction>(apiId, DeleteRecordAction.OperationId);

            Services.AddFlowActionByApiIdAndOperationsName<GetItemAction>(apiId, GetItemAction.OperationId);

            Services.AddFlowActionByApiIdAndOperationsName<ListRecordsAction>(apiId, ListRecordsAction.OperationId);

            Services.AddFlowActionByApiIdAndOperationsName<DisAndAssociateEntitiesAction>(apiId,
                DisAndAssociateEntitiesAction.OperationId);

            Services.AddFlowActionByApiIdAndOperationsName<ScopeActionExecutor>(apiId, new[] {"ExecuteChangeset"});

            return Services;
        }
    }
}