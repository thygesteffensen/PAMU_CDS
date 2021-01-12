using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PAMU_CDS.Auxiliary
{
    public class PamuCdsOrganizationService : IOrganizationService
    {
        private readonly IOrganizationService _organizationService;

        public PamuCdsOrganizationService(IOrganizationService organizationService)
        {
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        }

        public Guid Create(Entity entity)
        {
            return _organizationService.Create(entity);
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return _organizationService.Retrieve(entityName, id, columnSet);
        }

        public void Update(Entity entity)
        {
            _organizationService.Update(entity);
        }

        public void Delete(string entityName, Guid id)
        {
            _organizationService.Delete(entityName, id);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            return _organizationService.Execute(request);
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship,
            EntityReferenceCollection relatedEntities)
        {
            _organizationService.Associate(entityName, entityId, relationship, relatedEntities);
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship,
            EntityReferenceCollection relatedEntities)
        {
            _organizationService.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            return _organizationService.RetrieveMultiple(query);
        }
    }
}