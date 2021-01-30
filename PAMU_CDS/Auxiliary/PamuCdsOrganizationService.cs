using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PAMU_CDS.Auxiliary
{
    public class PamuCdsOrganizationService : IOrganizationService
    {
        private readonly IOrganizationService _organizationService;
        private readonly RecursionChecker _recursionChecker;

        public PamuCdsOrganizationService(IOrganizationService organizationService, RecursionChecker recursionChecker)
        {
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            _recursionChecker = recursionChecker ?? throw new ArgumentNullException(nameof(recursionChecker));
        }

        public Guid Create(Entity entity)
        {
            _recursionChecker.Add(entity.Id, "create");
            return _organizationService.Create(entity);
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return _organizationService.Retrieve(entityName, id, columnSet);
        }

        public void Update(Entity entity)
        {
            _recursionChecker.Add(entity.Id, "update");
            _organizationService.Update(entity);
        }

        public void Delete(string entityName, Guid id)
        {
            _recursionChecker.Add(id, "delete");
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

    public class RecursionChecker
    {
        private List<Entries> _entriesList;
        public RecursionChecker()
        {
            _entriesList = new List<Entries>();
        }

        public void Add(Guid entityId, string requestName)
        {
            _entriesList.Add(new Entries{Guid = entityId, RequestName = requestName});
        }

        public bool IsRecursiveCall(Guid entityId, string requestName)
        {
            return 10 < _entriesList.Count(x => x.Guid.Equals(entityId) && x.RequestName.Equals(requestName.ToLower()));
        }
    }

    public class Entries
    {
        public Guid Guid { get; set; }
        public string RequestName { get; set; }
    }
}