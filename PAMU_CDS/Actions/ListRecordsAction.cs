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
    public class ListRecordsAction : OpenApiConnectionActionExecutorBase
    {
        public static readonly string[] OperationId = {"ListRecords"};

        private readonly IOrganizationService _organizationService;
        private readonly IState _state;
        private readonly ILogger<ListRecordsAction> _logger;

        public ListRecordsAction(
            IExpressionEngine expressionEngine,
            IOrganizationService organizationService,
            IState state,
            ILogger<ListRecordsAction> logger) : base(expressionEngine)
        {
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<ActionResult> Execute()
        {
            var entity = new Entity();
            entity.CreateEntityFromParameters(Parameters);

            try
            {
                var response = (RetrieveMultipleResponse) _organizationService.Execute(
                    new RetrieveMultipleRequest
                    {
                        Query = BuildQuery(Parameters.AsDict(), entity)
                    });

                AddResponseToOutput(response.EntityCollection);
            }
            catch (InvalidPluginExecutionException exp)
            {
                // We need to do some experiments on how the error handling works. Take a look at one of your customers.
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

        private void AddResponseToOutput(EntityCollection responseEntityCollection)
        {
            var list = responseEntityCollection.Entities.Select(entity => entity.ToValueContainer()).ToList();

            _state.AddOutputs("", new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {"body/value", new ValueContainer(list)}
            }));
        }

        private QueryBase BuildQuery(IReadOnlyDictionary<string, ValueContainer> paramDict, Entity entity)
        {
            if (paramDict.ContainsKey("fetchXml"))
            {
                return new FetchExpression(paramDict["fetchXml"].GetValue<string>());
            }

            var query = new QueryExpression
            {
                EntityName = entity.LogicalName
            };

            if (paramDict.ContainsKey("$filter"))
            {
                query.Criteria = BuildFilterExpression();
            }

            if (paramDict.ContainsKey("$top"))
            {
                query.TopCount = Parameters["$top"].GetValue<int>();
            }

            query.ColumnSet = paramDict.ContainsKey("$select") ? BuildColumnSet() : new ColumnSet(true);

            if (paramDict.ContainsKey("$orderby"))
            {
                var clauses = Parameters["$orderby"].GetValue<string>().Split(',');
                if (clauses.Length > 1) _logger.LogWarning("Only one orderby is supported.");

                var claus = clauses[0].Split(' ');
                if (claus.Length == 1)
                {
                    query.AddOrder(claus[0], OrderType.Ascending);
                }
                else
                {
                    query.AddOrder(claus[0], claus[1] == "asc" ? OrderType.Ascending : OrderType.Descending);
                }
            }

            return query;
        }

        private FilterExpression BuildFilterExpression()
        {
            var t = new OdataFilter();
            return t.OdataToFilterExpression(Parameters["$filter"].GetValue<string>());
        }

        private RelationshipQueryCollection GetExpandedEntities(string entityName)
        {
            var paras = Parameters.GetValue<Dictionary<string, ValueContainer>>();
            if (!paras.ContainsKey("$expand")) return null;

            var t = new RelationshipQueryCollection();

            var p = new OdataParser();
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

            var columns = Parameters["$select"].GetValue<string>().Split(',');
            foreach (var column in columns)
            {
                columnSet.Columns.Add(column.Trim());
            }

            return columnSet;
        }
    }
}