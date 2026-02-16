using Google.Cloud.Firestore;
using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Domain.Common;
using System.Linq.Expressions;

namespace ArchitectureAI.Persistence.Repositories;

public class FirestoreRepository<T> : IGenericRepository<T> where T : Entity
{
    private readonly Context.FirestoreDbContext _context;
    private readonly CollectionReference _collection;
    private readonly ITenantService _tenantService;
    private string _tenantId => _tenantService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID is missing.");

    public FirestoreRepository(Context.FirestoreDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
        _collection = _context.Collection(typeof(T).Name);
    }

    public async Task<T> AddAsync(T entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }
        
        // Enforce TenantId
        entity.TenantId = _tenantId;

        var docRef = _collection.Document(entity.Id);
        await docRef.SetAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        var batch = _context.Db.StartBatch();
        foreach (var entity in entities)
        {
            if (string.IsNullOrEmpty(entity.Id))
                entity.Id = Guid.NewGuid().ToString();
            
            // Enforce TenantId
            entity.TenantId = _tenantId;
            
            var docRef = _collection.Document(entity.Id);
            batch.Set(docRef, entity);
        }
        await batch.CommitAsync();
    }
    
    // Note: Firestore doesn't support complex Expression compilation easily.
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges)
    {
         throw new NotImplementedException("Direct IQueryable not fully supported. Use FindAsync.");
    }

    public async Task<T> FindAsync(Expression<Func<T, bool>> expression)
    {
         throw new NotImplementedException("Complex FindAsync not supported. Use GetByIdAsync.");
    }
    
    public async Task<T> GetByIdAsync(string id)
    {
        var docRef = _collection.Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (snapshot.Exists)
        {
            var entity = snapshot.ConvertTo<T>();
            // Enforce Tenant Isolation on Read
            if (entity.TenantId == _tenantId)
            {
                return entity;
            }
        }
        return null;
    }

    public async Task<IEnumerable<T>> GetAllAsync(params string[] includeProperties)
    {
        // Filter by TenantId
        var query = _collection.WhereEqualTo(nameof(Entity.TenantId), _tenantId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
    }
    
    public async Task<IEnumerable<T>> FindAndIncludeAsync(Expression<Func<T, bool>> expression, params string[] includeProperties)
    {
        throw new NotImplementedException();
    }

    public async Task<int> RemoveAsync(T entity)
    {
        // Enforce Tenant Check
        if (entity.TenantId != _tenantId) return 0;
        
        await _collection.Document(entity.Id).DeleteAsync();
        return 1;
    }
    
    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
         var batch = _context.Db.StartBatch();
        foreach (var entity in entities)
        {
            // Enforce Tenant Check
            if (entity.TenantId != _tenantId) continue;
            
            var docRef = _collection.Document(entity.Id);
            batch.Delete(docRef);
        }
        await batch.CommitAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        // Enforce Tenant Id persistence (prevent switching tenants)
        entity.TenantId = _tenantId;
        entity.DateModified = DateTime.UtcNow;
        
        var docRef = _collection.Document(entity.Id);
        await docRef.SetAsync(entity, SetOptions.MergeAll);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
    {
        throw new NotImplementedException("Count with filter not efficiently implemented yet.");
    }

    public async Task<int> CountAsync()
    {
        // Count only tenant documents
        var query = _collection.WhereEqualTo(nameof(Entity.TenantId), _tenantId);
        var snapshot = await query.Count().GetSnapshotAsync();
        return (int)snapshot.Count;
    }

    public Task<bool> SaveAsync()
    {
        return Task.FromResult(true); 
    }

    public Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params string[] includeProperties)
    {
         throw new NotImplementedException();
    }
}
