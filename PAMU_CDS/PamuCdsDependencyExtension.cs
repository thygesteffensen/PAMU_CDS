using Microsoft.Extensions.DependencyInjection;
using PAMU_CDS.Actions;
using PAMU_CDS.Auxiliary;
using Parser;
using Parser.FlowParser.ActionExecutors.Implementations;

namespace PAMU_CDS
{
    public static class PamuCdsDependencyExtension
    {
        public static void AddPamuCds(this IServiceCollection services)
        {
            const string apiId = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps";

            services.AddScoped<OrganizationServiceContext>();

            services.AddScoped<XrmMockupCdsTrigger>();

            services.AddFlowActionByApiIdAndOperationsName<CdsTrigger>(apiId, CdsTrigger.OperationId);

            services.AddFlowActionByApiIdAndOperationsName<CreateRecordAction>(apiId, CreateRecordAction.OperationId);

            services.AddFlowActionByApiIdAndOperationsName<UpdateRecordAction>(apiId, UpdateRecordAction.OperationId);

            services.AddFlowActionByApiIdAndOperationsName<DeleteRecordAction>(apiId, DeleteRecordAction.OperationId);

            services.AddFlowActionByApiIdAndOperationsName<GetItemAction>(apiId, GetItemAction.OperationId);

            services.AddFlowActionByApiIdAndOperationsName<ListRecordsAction>(apiId, ListRecordsAction.OperationId);

            services.AddFlowActionByApiIdAndOperationsName<DisAndAssociateEntitiesAction>(apiId,
                DisAndAssociateEntitiesAction.OperationId);

            services.AddFlowActionByApiIdAndOperationsName<ScopeActionExecutor>(apiId,
                new[] {"ExecuteChangeset"});
        }
    }
}