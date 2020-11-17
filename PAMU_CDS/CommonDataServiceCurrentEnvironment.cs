using System;
using System.Collections.Generic;
using System.Linq;
using IXrmMockupExtension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using PAMU_CDS.Actions;
using PAMU_CDS.Enums;
using Parser;
using Parser.ExpressionParser.Functions.Base;
using Parser.FlowParser;

namespace PAMU_CDS
{
    public class CommonDataServiceCurrentEnvironment : IMockUpExtension
    {
        private readonly List<TriggerSkeleton> _triggers;

        public CommonDataServiceCurrentEnvironment()
        {
            _triggers = new List<TriggerSkeleton>
            {
                new TriggerSkeleton
                {
                    TriggerCondition = TriggerCondition.Create,
                    Table = "account",
                    Scope = Scope.Organization,
                    SetTriggeringAttributes = "statuscode,name",
                    FlowDescription =
                        new Uri(
                            @"C:\git\opensource\PowerAutomateMockUp\Test\FlowSamples\PowerAutomateMockUpSampleFlow.json")
                }
            };
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
            var flowRunner = sp.GetRequiredService<FlowRunner>();

            var state = sp.GetRequiredService<IState>();

            foreach (var triggerSkeleton in flows)
            {
                // TODO: Populate the flowRunner with relevant variables before triggering.
                flowRunner.InitializeFlowRunner(triggerSkeleton.FlowDescription.ToString());
                flowRunner.Trigger();
            }
        }

        private IEnumerable<TriggerSkeleton> ApplyCriteria(OrganizationRequest request)
        {
            IEnumerable<TriggerSkeleton> flows;
            if (request.RequestName == "Delete")
            {
                var target = (EntityReference) request.Parameters["target"];
                flows = _triggers.Where(x =>
                    x.Table == target.LogicalName &&
                    x.TriggerCondition == TriggerCondition.Delete ||
                    x.TriggerCondition == TriggerCondition.CreateDelete ||
                    x.TriggerCondition == TriggerCondition.UpdateDelete ||
                    x.TriggerCondition == TriggerCondition.CreateUpdateDelete);
            }
            else
            {
                var target = (Entity) request.Parameters["target"];
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
                    x.GetTriggeringAttributes.Length == 0 ||
                    x.GetTriggeringAttributes.Any(y => target.Attributes.Keys.Contains(y)));
            }

            return flows;
        }

        private static ServiceCollection BuildServiceCollection(IOrganizationService organizationService)
        {
            var services = new ServiceCollection();
            // services.AddFlowRunner(settings);
            services.Configure<FlowSettings>(x => x.IgnoreActions.Add("Hej med dig"));

            services.AddSingleton(organizationService);

            services.AddFlowActionByFlowType<CreateRecordAction>("CreateRecord");
            services.AddFlowActionByFlowType<CreateRecordAction>("DeleteRecord");
            services.AddFlowActionByFlowType<CreateRecordAction>("ExecuteChangeset");
            services.AddFlowActionByFlowType<CreateRecordAction>("GetItem");
            services.AddFlowActionByFlowType<CreateRecordAction>("ListRecords");
            // services.AddFlowActionByFlowType<>("PerformBoundAction");
            // services.AddFlowActionByFlowType<>("PerformUnboundAction");
            // services.AddFlowActionByFlowType<>("PredictV2");
            services.AddFlowActionByFlowType<CreateRecordAction>("AssociateEntities");
            services.AddFlowActionByFlowType<CreateRecordAction>("DisassociateEntities");
            services.AddFlowActionByFlowType<CreateRecordAction>("UpdateRecord");
            // services.AddFlowActionByFlowType<>("UpdateEntityFileImageFieldContent");

            services.AddFlowRunner();
            return services;
        }
    }
}