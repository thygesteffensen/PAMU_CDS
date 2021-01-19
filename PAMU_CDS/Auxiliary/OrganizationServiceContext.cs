using Microsoft.Xrm.Sdk;

namespace PAMU_CDS.Auxiliary
{
    public class OrganizationServiceContext
    {
        public IOrganizationService OrganizationService { get; set; }

        public IOrganizationService GetOrganizationService()
        {
            return OrganizationService;
        }
    }
}