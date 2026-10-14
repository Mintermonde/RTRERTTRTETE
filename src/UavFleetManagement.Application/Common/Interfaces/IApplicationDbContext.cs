using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Domain.Entities.Catalog;
using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Entities.Identity;
using UavFleetManagement.Domain.Entities.Insurance;
using UavFleetManagement.Domain.Entities.Maintenance;
using UavFleetManagement.Domain.Entities.Organizations;
using UavFleetManagement.Domain.Entities.Outbox;
using UavFleetManagement.Domain.Entities.Pilots;
using UavFleetManagement.Domain.Entities.Registration;
using UavFleetManagement.Domain.Entities.Storage;

namespace UavFleetManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Manufacturer> Manufacturers { get; }
    DbSet<UavOwner> UavOwners { get; }
    DbSet<UavUser> UavUsers { get; }
    DbSet<UavModel> UavModels { get; }
    DbSet<UserModelPermission> UserModelPermissions { get; }
    DbSet<UavInstance> UavInstances { get; }
    DbSet<ComponentCatalog> ComponentsCatalog { get; }
    DbSet<InstanceComponent> InstanceComponents { get; }
    DbSet<MaintenanceLog> MaintenanceLogs { get; }
    DbSet<MaintenanceComponentReplacement> MaintenanceComponentReplacements { get; }
    DbSet<Engineer> Engineers { get; }
    DbSet<UserCredential> UserCredentials { get; }
    DbSet<SuperuserCredential> SuperuserCredentials { get; }

    // New EDA & Enterprise Expansion
    DbSet<Organization> Organizations { get; }
    DbSet<MinimumWageRate> MinimumWageRates { get; }
    DbSet<FileAttachment> FileAttachments { get; }
    DbSet<UavRegistrationApplication> UavRegistrationApplications { get; }
    DbSet<InsurancePolicy> InsurancePolicies { get; }
    DbSet<BillingInvoice> BillingInvoices { get; }
    DbSet<EngineerRepairAuthorization> EngineerRepairAuthorizations { get; }
    DbSet<OutboxEvent> OutboxEvents { get; }
    DbSet<InboxEvent> InboxEvents { get; }
    DbSet<UserRefreshToken> UserRefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
