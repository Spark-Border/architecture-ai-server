using Finbuckle.MultiTenant;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;

namespace ArchitectureAI.Infrastructure.Tenancy;

public class FirestoreMultiTenantStore(
    Persistence.Context.FirestoreDbContext context,
    ILogger<FirestoreMultiTenantStore> logger
) : IMultiTenantStore<TenantInfo>
{
    private readonly FirestoreDb _firestore = context.Db;
    private readonly ILogger<FirestoreMultiTenantStore> _logger = logger;
    private const string CollectionName = "Tenants";

    public async Task<bool> TryAddAsync(TenantInfo tenantInfo)
    {
        try
        {
            var docRef = _firestore.Collection(CollectionName).Document(tenantInfo.Id);
            var snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return false;
            }

            await docRef.SetAsync(tenantInfo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tenant {TenantId}", tenantInfo.Id);
            return false;
        }
    }

    public async Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
    {
        try
        {
            var docRef = _firestore.Collection(CollectionName).Document(tenantInfo.Id);
            await docRef.SetAsync(tenantInfo, SetOptions.MergeAll);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", tenantInfo.Id);
            return false;
        }
    }

    public async Task<bool> TryRemoveAsync(string identifier)
    {
        try
        {
            // Case 1: Identifier is ID
            var docRef = _firestore.Collection(CollectionName).Document(identifier);
            var snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                await docRef.DeleteAsync();
                return true;
            }

            // Case 2: Identifier is custom field 'Identifier'
            var query = _firestore
                .Collection(CollectionName)
                .WhereEqualTo(nameof(TenantInfo.Identifier), identifier);
            var querySnap = await query.Limit(1).GetSnapshotAsync();
            if (querySnap.Count > 0)
            {
                await querySnap[0].Reference.DeleteAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tenant {Identifier}", identifier);
            return false;
        }
    }

    public async Task<TenantInfo?> TryGetAsync(string id)
    {
        try
        {
            var docRef = _firestore.Collection(CollectionName).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<TenantInfo>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant {TenantId}", id);
            return null;
        }
    }

    public async Task<TenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        try
        {
            // Case 1: Identifier might match ID directly (often used interchangeably)
            var docRef = _firestore.Collection(CollectionName).Document(identifier);
            var snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var t = snapshot.ConvertTo<TenantInfo>();
                // Verify it actually matches (though if ID matches, it's the tenant)
                return t;
            }

            // Case 2: Custom Identifier field
            var query = _firestore
                .Collection(CollectionName)
                .WhereEqualTo(nameof(TenantInfo.Identifier), identifier);
            var qSnap = await query.Limit(1).GetSnapshotAsync();
            if (qSnap.Count > 0)
            {
                return qSnap[0].ConvertTo<TenantInfo>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant identifier {Identifier}", identifier);
            return null;
        }
    }

    public async Task<IEnumerable<TenantInfo>> GetAllAsync()
    {
        try
        {
            var query = _firestore.Collection(CollectionName);
            var snapshot = await query.GetSnapshotAsync();

            var tenants = new List<TenantInfo>();
            foreach (var doc in snapshot.Documents)
            {
                tenants.Add(doc.ConvertTo<TenantInfo>());
            }
            return tenants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tenants");
            return [];
        }
    }
}
