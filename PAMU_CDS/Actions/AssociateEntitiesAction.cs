using System;
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
    public class AssociateEntitiesAction : OpenApiConnectionActionExecutorBase
    {
        public static readonly string[] OperationId = {"AssociateEntitiesAction","DisassociateEntities"};

        private readonly IOrganizationService _organizationService;

        public AssociateEntitiesAction(
            IExpressionEngine expressionEngine,
            IOrganizationService organizationService) : base(expressionEngine)
        {
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        }


        public override Task<ActionResult> Execute()
        {
            var entity = new Entity();
            entity = entity.CreateEntityFromParameters(Parameters);

            try
            {
                // TODO: Figure out how this handle bad associations and error handling.
                // assignees: thygesteffensen
                var relatedEntity = ExtractEntityReferenceFromOdataId();

                var associateRequest = new AssociateRequest
                {
                    Target = entity.ToEntityReference(),
                    Relationship = new Relationship(Parameters["associationEntityRelationship"].GetValue<string>()),
                    RelatedEntities = new EntityReferenceCollection {relatedEntity}
                };

                _organizationService.Execute(associateRequest);
            }
            catch (InvalidPluginExecutionException)
            {
                // We need to do some experiments on how the error handling works. Take a look at one of your customers.
                return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Failed});
            }

            return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
        }

        private EntityReference ExtractEntityReferenceFromOdataId()
        {
            // https://dglab6.crm4.dynamics.com/api/data/v9.1/contacts(8c711383-b933-eb11-a813-000d3ab11761)

            var oDataId = Parameters["item/@odata.id"].GetValue<string>();
            var entityName =
                oDataId.Substring(oDataId.LastIndexOf('/')+1, oDataId.IndexOf('(') - oDataId.LastIndexOf('/')-2);
            var entityId = oDataId.Substring(oDataId.IndexOf('(')+1, oDataId.IndexOf(')') - oDataId.IndexOf('(')-1);

            return new EntityReference(entityName, new Guid(entityId));
        }
    }
}