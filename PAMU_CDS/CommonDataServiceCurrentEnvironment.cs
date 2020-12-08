using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IXrmMockupExtension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using PAMU_CDS.Actions;
using PAMU_CDS.Auxiliary;
using PAMU_CDS.Enums;
using Parser;
using Parser.ExpressionParser.Functions.Base;
using Parser.FlowParser;

namespace PAMU_CDS
{
    public class CommonDataServiceCurrentEnvironment : IMockUpExtension
    {
        private readonly List<TriggerSkeleton> _triggers;

        public CommonDataServiceCurrentEnvironment(Uri flowFolderPath)
        {
            var files = Directory.GetFiles(flowFolderPath.AbsolutePath);

            _triggers = new List<TriggerSkeleton>();

            foreach (var file in files)
            {
                _triggers.AddTo(file);
            }
        }

        public void TriggerExtension(
            IOrganizationService organizationService,
            OrganizationRequest request,
            Entity currentEntity,
            EntityReference userRef)
        {
            if (!new[] {"Create", "Delete", "Update"}.Contains(request.RequestName))
            {
                throw new InvalidOperationException("PAMU_CDS does not support the request.");
            }

            var flows = ApplyCriteria(request);

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
                flowRunner.Trigger();
            }
        }

        private IEnumerable<TriggerSkeleton> ApplyCriteria(OrganizationRequest request)
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

            return flows;
        }

        private static ServiceCollection BuildServiceCollection(IOrganizationService organizationService)
        {
            var apiId = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps";

            var services = new ServiceCollection();
            services.AddFlowRunner();

            services.AddSingleton(organizationService);
            services.Configure<FlowSettings>(x => { });

            services.AddFlowActionByName<UpdateRecordAction>("Update_a_record_-_Set_job_title_to_Technical_Supervisor");

            services.AddFlowActionByApiIdAndOperationsName<CdsTrigger>(apiId,
                new[] {"SubscribeWebhookTrigger"});

            services.AddFlowActionByApiIdAndOperationsName<CreateRecordAction>(apiId,
                new[] {CreateRecordAction.OperationId});

            services.AddFlowActionByApiIdAndOperationsName<UpdateRecordAction>(apiId,
                new[] {UpdateRecordAction.OperationId});

            services.AddFlowActionByApiIdAndOperationsName<DeleteRecordAction>(apiId,
                new[] {DeleteRecordAction.OperationId});

            services.AddFlowActionByApiIdAndOperationsName<GetItemAction>(apiId,
                new[] {GetItemAction.OperationId});

            services.AddFlowActionByApiIdAndOperationsName<AssociateEntitiesAction>(apiId,
                AssociateEntitiesAction.OperationId);
            // services.AddFlowActionByFlowType<CreateRecordAction>("ExecuteChangeset");
            // services.AddFlowActionByFlowType<CreateRecordAction>("ListRecords");
            // // services.AddFlowActionByFlowType<>("PerformBoundAction");
            // // services.AddFlowActionByFlowType<>("PerformUnboundAction");
            // // services.AddFlowActionByFlowType<>("PredictV2");
            // services.AddFlowActionByFlowType<CreateRecordAction>("AssociateEntities");
            // services.AddFlowActionByFlowType<CreateRecordAction>("DisassociateEntities");
            // // services.AddFlowActionByFlowType<>("UpdateEntityFileImageFieldContent");

            return services;
        }
    }
}