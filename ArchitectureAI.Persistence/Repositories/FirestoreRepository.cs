using Google.Cloud.Firestore;
using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Domain.Common;
using System.Linq.Expressions;
// using ArchitectureAI.Persistence.Context; // Namespace might need adjustment

namespace ArchitectureAI.Persistence.Repositories;

public class FirestoreRepository<T> : IGenericRepository<T> where T : Entity
{
    private readonly Context.FirestoreDbContext _context;
    private readonly CollectionReference _collection;

    public FirestoreRepository(Context.FirestoreDbContext context)
    {
        _context = context;
        // Convention: Collection name matches Class name mostly
        _collection = _context.Collection(typeof(T).Name);
    }

    public async Task<T> AddAsync(T entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }
        
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
            
            var docRef = _collection.Document(entity.Id);
            batch.Set(docRef, entity);
        }
        await batch.CommitAsync();
    }
    
    // Note: Firestore doesn't support complex Expression compilation easily.
    // implementing a basic version or throwing not supported for now.
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges)
    {
         throw new NotImplementedException("Direct IQueryable not fully supported with Firestore SDK yet. Use FindAsync with specific logic or getAll.");
    }

    public async Task<T> FindAsync(Expression<Func<T, bool>> expression)
    {
        // Very limited implementation - usually we'd fetch all or use specific queries
        // For now, let's assume we might filtering in memory if dataset is small, OR valid implementation requires
        // a expression visitor.
         throw new NotImplementedException("Complex FindAsync not supported. Use GetByIdAsync.");
    }
    
    // Assuming simple property equality checks 
    // Implementing a basic GetById as primary method
    public async Task<T> GetByIdAsync(string id)
    {
        var docRef = _collection.Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<T>();
        }
        return null;
    }

    public async Task<IEnumerable<T>> GetAllAsync(params string[] includeProperties)
    {
        var snapshot = await _collection.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
    }
    
    // Stub for include properties compatibility
    public async Task<IEnumerable<T>> FindAndIncludeAsync(Expression<Func<T, bool>> expression, params string[] includeProperties)
    {
        throw new NotImplementedException();
    }

    public async Task<int> RemoveAsync(T entity)
    {
        await _collection.Document(entity.Id).DeleteAsync();
        return 1;
    }
    
    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
         var batch = _context.Db.StartBatch();
        foreach (var entity in entities)
        {
            var docRef = _collection.Document(entity.Id);
            batch.Delete(docRef);
        }
        await batch.CommitAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        var docRef = _collection.Document(entity.Id);
        entity.DateModified = DateTime.UtcNow;
        await docRef.SetAsync(entity, SetOptions.MergeAll);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
    {
        // Simple implementation: Fetch all and count (Expensive!) or use Aggregation queries if available in SDK
        var query = _collection.WhereEqualTo("Id", "dummy"); // Needs replacement
        // For now:
        throw new NotImplementedException("Count with filter not efficiently implemented yet.");
    }

    public async Task<int> CountAsync()
    {
        var snapshot = await _collection.Count().GetSnapshotAsync();
        return (int)snapshot.Count;
    }

    public Task<bool> SaveAsync()
    {
        return Task.FromResult(true); // Auto-save in Firestore
    }

    public Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params string[] includeProperties)
    {
         throw new NotImplementedException();
    }
}
