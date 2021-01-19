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
    public class CommonDataServiceCurrentEnvironment : IMockUpExtension
    {
        private readonly FlowRunner _flowRunner;
        private readonly IState _state;
        private readonly OrganizationServiceFactory _organizationServiceFactory;
        private readonly ILogger<CommonDataServiceCurrentEnvironment> _logger;
        private readonly CdsFlowSettings _cdsFlowSettings;
        private readonly CdsFlowSettings _settings;
        private List<TriggerSkeleton> _triggers = new List<TriggerSkeleton>();

        public CommonDataServiceCurrentEnvironment(
            FlowRunner flowRunner,
            IState state, // Remove this in the future
            OrganizationServiceFactory organizationServiceFactory,
            ILogger<CommonDataServiceCurrentEnvironment> logger,
            IOptions<CdsFlowSettings> cdsFlowSettings)
        {
            _flowRunner = flowRunner ?? throw new ArgumentNullException(nameof(flowRunner));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _organizationServiceFactory = organizationServiceFactory ??
                                          throw new ArgumentNullException(nameof(organizationServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cdsFlowSettings = cdsFlowSettings?.Value;
        }
        
        public void AddFlows(Uri flowFolderPath)
        {
            var files = Directory.GetFiles(flowFolderPath.AbsolutePath);

            foreach (var file in files)
            {
                _triggers.AddTo(file);
            }
        }

        public void RemoveFlows()
        {
            _triggers = new List<TriggerSkeleton>();
        }

        /*private void CommonConstructor(Uri flowFolderPath, IServiceScopeFactory factory)
        {
            factory.CreateScope();
            var files = Directory.GetFiles(flowFolderPath.AbsolutePath);

            _triggers = new List<TriggerSkeleton>();

            foreach (var file in files)
            {
                _triggers.AddTo(file);
            }
        }*/

        /*public CommonDataServiceCurrentEnvironment(Uri flowFolderPath, Action<IServiceCollection> reg = null)
        {
            CommonConstructor(flowFolderPath);

            reg?.Invoke(Services); // Action

            var flowSettings = (FlowSettings) settings;


            Services.Configure<FlowSettings>();
            _settings = settings;
        }*/

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

            _organizationServiceFactory.OrganizationService = organizationService;

            var flows = ApplyCriteria(request, currentEntity ?? preEntity);

            // var flowRunner = sp.GetRequiredService<FlowRunner>();

            foreach (var triggerSkeleton in flows)
            {
                // var state = sp.GetRequiredService<IState>();

                _state.AddTriggerOutputs(currentEntity.ToValueContainer());

                // var flowRunner = sp.GetRequiredService<FlowRunner>();
                _flowRunner.InitializeFlowRunner(triggerSkeleton.FlowDescription.AbsolutePath);
                await _flowRunner.Trigger();
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
    }
}