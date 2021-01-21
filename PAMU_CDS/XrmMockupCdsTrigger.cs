using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IXrmMockupExtension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using PAMU_CDS.Auxiliary;
using PAMU_CDS.Enums;
using Parser;
using Parser.FlowParser;

namespace PAMU_CDS
{
    public class XrmMockupCdsTrigger : IMockUpExtension
    {
        private readonly ILogger<XrmMockupCdsTrigger> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CdsFlowSettings _cdsFlowSettings;
        private List<TriggerSkeleton> _triggers = new List<TriggerSkeleton>();

        public XrmMockupCdsTrigger(
            ILogger<XrmMockupCdsTrigger> logger,
            IOptions<CdsFlowSettings> cdsFlowSettings,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _cdsFlowSettings = cdsFlowSettings?.Value ?? new CdsFlowSettings();
        }

        public void AddFlows(Uri flowFolderPath)
        {
            if (Directory.Exists(flowFolderPath.AbsolutePath))
            {
                var files = Directory.GetFiles(flowFolderPath.AbsolutePath);

                foreach (var file in files)
                {
                    _triggers.AddTo(file);
                }
            }
            else if (File.Exists(flowFolderPath.AbsolutePath))
            {
                _triggers.AddTo(flowFolderPath.AbsolutePath);
            }
            else
            {
                throw new DirectoryNotFoundException(
                    "Could not find either directory or file from the given path.");
            }
        }

        public void RemoveFlows()
        {
            _triggers = new List<TriggerSkeleton>();
        }

        public void TriggerExtension(
            IOrganizationService organizationService,
            OrganizationRequest request,
            Entity currentEntity,
            Entity preEntity,
            EntityReference userRef)
        {
            _logger.LogInformation("Non-async Trigger event occured");
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
            _logger.LogInformation("Async Trigger event occured");
            if (!new[] {"Create", "Delete", "Update"}.Contains(request.RequestName))
            {
                throw new InvalidOperationException(
                    $"PAMU_CDS does not support the request: {request.RequestName}.");
            }

            using var scope = _scopeFactory.CreateScope();

            scope.ServiceProvider.GetRequiredService<OrganizationServiceContext>().OrganizationService =
                organizationService;

            var flows = ApplyCriteria(request, currentEntity ?? preEntity);

            foreach (var triggerSkeleton in flows)
            {
                var state = scope.ServiceProvider.GetRequiredService<IState>();

                state.AddTriggerOutputs(currentEntity.ToValueContainer());

                var flowRunner = scope.ServiceProvider.GetRequiredService<IFlowRunner>();
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
            flows = flows.Where(x => !_cdsFlowSettings.DontExecuteFlows.Contains(x.FlowName));
            flows = flows.Where(x => FulfillFilterExpression(x.FilterExpression, entity, t));

            return flows;
        }

        private bool FulfillFilterExpression(string filterExpression, Entity entity, OdataFilter odataFilter)
        {
            if (filterExpression == null) return true;
            var filterExpresion = odataFilter.OdataToFilterExpression(filterExpression);
            return ApplyFilterExpression.ApplyFilterExpressionToEntity(entity, filterExpresion);
        }
    }
}