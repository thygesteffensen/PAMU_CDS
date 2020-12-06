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
    public class GetItemAction : OpenApiConnectionActionExecutorBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IState _state;

        public GetItemAction(
            IExpressionEngine expressionEngine,
            IOrganizationService organizationService,
            IState state) : base(expressionEngine)
        {
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override Task<ActionResult> Execute()
        {
            var entityName = Parameters["entityName"].GetValue<string>();
            var recordId = new Guid(Parameters["recordId"].GetValue<string>());


            var columnSet = BuildColumnSet();

            var expandedEntities = GetExpandedEntities();

            try
            {
                var retrieveRequest = new RetrieveRequest
                {
                    Target = new EntityReference(entityName, recordId),
                    ColumnSet = columnSet,
                    RelatedEntitiesQuery = expandedEntities
                };

                var response = (RetrieveResponse) _organizationService.Execute(retrieveRequest);

                
                // _state.AddOutputs(ActionName, response.ToValueContainer());
            }
            catch (InvalidPluginExecutionException)
            {
                return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Failed});
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
            }

            return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
        }

        private RelationshipQueryCollection GetExpandedEntities()
        {
            var t  = new RelationshipQueryCollection();

            var expand = Parameters["$expand"];
            
            
            

            return t;
        }

        private ColumnSet BuildColumnSet()
        {
            var columnSet = new ColumnSet();
            if (Parameters["$select"].Type() == ValueContainer.ValueType.Null)
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