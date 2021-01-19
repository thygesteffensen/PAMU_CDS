using Microsoft.Xrm.Sdk;

namespace PAMU_CDS.Auxiliary
{
    public class OrganizationServiceFactory
    {
        public IOrganizationService OrganizationService { get; set; }

        public IOrganizationService GetOrganizationService()
        {
            return OrganizationService;
        }
    }
}