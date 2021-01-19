using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using PAMU_CDS.Auxiliary;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace PAMU_CDS.Actions
{
    public class GetItemAction : OpenApiConnectionActionExecutorBase
    {
        public static readonly string[] OperationId = {"GetItem"};

        private readonly IOrganizationService _organizationService;
        private readonly IState _state;
        private readonly ILogger<GetItemAction> _logger;

        public GetItemAction(
            IExpressionEngine expressionEngine,
            OrganizationServiceFactory organizationServiceFactory,
            IState state,
            ILogger<GetItemAction> logger) : base(expressionEngine)
        {
            _organizationService = organizationServiceFactory?.GetOrganizationService() ?? 
                                   throw new ArgumentNullException(nameof(organizationServiceFactory));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<ActionResult> Execute()
        {
            var entity = new Entity();
            entity.CreateEntityFromParameters(Parameters);

            var expandedEntities = GetExpandedEntities(entity.LogicalName);

            try
            {
                var retrieveRequest = new RetrieveRequest
                {
                    Target = new EntityReference(entity.LogicalName, entity.Id),
                    ColumnSet = BuildColumnSet(),
                    RelatedEntitiesQuery = expandedEntities
                };

                var response = (RetrieveResponse) _organizationService.Execute(retrieveRequest);


                _state.AddOutputs(ActionName, response.Entity.ToValueContainer());
            }
            catch (InvalidPluginExecutionException exp)
            {
                return Task.FromResult(new ActionResult
                    {ActionStatus = ActionStatus.Failed, ActionExecutorException = exp});
            }
            catch (Exception exp) // MockupException
            {
                if (exp.Message.Contains("entity doesn't contain attribute"))
                {
                    var messageDivided = exp.Message.Split('\'');
                    throw new PowerAutomateException(
                        $"0x0 | Could not find a property named '{messageDivided[1]}' on type 'Microsoft.Dynamics.CRM.{messageDivided[3]}'",
                        exp);
                }
                return Task.FromResult(new ActionResult
                    {ActionStatus = ActionStatus.Failed, ActionExecutorException = exp});
            }

            return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
        }

        private RelationshipQueryCollection GetExpandedEntities(string entityName)
        {
            var paras = Parameters.GetValue<Dictionary<string, ValueContainer>>();
            if (!paras.ContainsKey("$expand")) return null;
            
            var t = new RelationshipQueryCollection();

            var p = new OdataParser();
            // TODO: Refactor with version alpha.18
            var expand = p.Get(Parameters["$expand"].GetValue<string>());


            foreach (var value in expand)
            {
                var relationship = ((RetrieveRelationshipResponse) _organizationService.Execute(
                        new RetrieveRelationshipRequest
                        {
                            Name = value.Option
                        }))
                    .RelationshipMetadata;

                var query = new QueryExpression
                {
                    ColumnSet = new ColumnSet(true),
                    EntityName = relationship switch
                    {
                        OneToManyRelationshipMetadata r when r.ReferencingEntity == entityName => r.ReferencedEntity,
                        OneToManyRelationshipMetadata r when r.ReferencedEntity == entityName => r.ReferencingEntity,
                        ManyToManyRelationshipMetadata r when r.Entity1LogicalName == entityName =>
                            r.Entity2LogicalName,
                        ManyToManyRelationshipMetadata r when r.Entity2LogicalName == entityName =>
                            r.Entity1LogicalName,
                        _ => throw new PowerAutomateException("Relationship not known...")
                    }
                };

                var select = value.Parameters.FirstOrDefault(x => x.Name == "$select");
                if (select != null)
                {
                    query.ColumnSet = new ColumnSet(string.Join(",", select.Properties));
                }

                t.Add(new Relationship(value.Option), query);
            }

            return t;
        }

        private ColumnSet BuildColumnSet()
        {
            var columnSet = new ColumnSet();
            // TODO: Refactor with version alpha.18
            var paras = Parameters.GetValue<Dictionary<string, ValueContainer>>();
            if (!paras.ContainsKey("$select"))
            {
                columnSet.AllColumns = true;
            }
            else
            {
                var columns = Parameters["$select"].GetValue<string>().Split(',');
                foreach (var column in columns)
                {
                    columnSet.Columns.Add(column.Trim());
                }
            }

            return columnSet;
        }
    }
}