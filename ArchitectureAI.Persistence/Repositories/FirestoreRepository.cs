using System.Linq.Expressions;
using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Domain.Audit;
using ArchitectureAI.Domain.Common;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;

namespace ArchitectureAI.Persistence.Repositories;

public class FirestoreRepository<T> : IGenericRepository<T>
    where T : Entity
{
    private readonly Context.FirestoreDbContext _context;
    private readonly CollectionReference _collection;
    private readonly ITenantService _tenantService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditService _auditService;

    private string _tenantId =>
        _tenantService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID is missing.");

    public FirestoreRepository(
        Context.FirestoreDbContext context,
        ITenantService tenantService,
        IHttpContextAccessor httpContextAccessor,
        IAuditService auditService
    )
    {
        _context = context;
        _tenantService = tenantService;
        _httpContextAccessor = httpContextAccessor;
        _auditService = auditService;
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

        await LogAuditAsync("Created", entity);
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
        // Simple Expression Parser for "x => x.Prop == Value"
        if (expression.Body is BinaryExpression binaryExpression
            && binaryExpression.NodeType == ExpressionType.Equal)
        {
            var left = binaryExpression.Left;
            var right = binaryExpression.Right;

            string propertyName = null;
            object value = null;

            // Case 1: x.Prop == Value
            if (left is MemberExpression memberLeft && right is ConstantExpression constantRight)
            {
                propertyName = memberLeft.Member.Name;
                value = constantRight.Value;
            }
            // Case 2: Value == x.Prop
            else if (left is ConstantExpression constantLeft && right is MemberExpression memberRight)
            {
                propertyName = memberRight.Member.Name;
                value = constantLeft.Value;
            }
            // Case 3: Handle captured variables (closures)
            else if (left is MemberExpression memberLeftClosure && right is MemberExpression memberRightClosure) 
            {
                 // Check which side is the parameter
                 if (memberLeftClosure.Expression is ParameterExpression) 
                 {
                     propertyName = memberLeftClosure.Member.Name;
                     value = GetValue(memberRightClosure);
                 }
                 else if (memberRightClosure.Expression is ParameterExpression)
                 {
                     propertyName = memberRightClosure.Member.Name;
                     value = GetValue(memberLeftClosure);
                 }
            }

            if (!string.IsNullOrEmpty(propertyName))
            {
                var query = _collection.WhereEqualTo(propertyName, value);
                // Enforce Tenant Isolation
                query = query.WhereEqualTo(nameof(Entity.TenantId), _tenantId);

                var snapshot = await query.Limit(1).GetSnapshotAsync();
                if (snapshot.Count > 0)
                {
                    return snapshot.Documents[0].ConvertTo<T>();
                }
                return null;
            }
        }

        throw new NotImplementedException("Only simple equality queries (e.g. x => x.Name == 'Value') are supported in FindAsync for now.");
    }

    private object GetValue(MemberExpression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        return getter();
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

    public async Task<IEnumerable<T>> FindAndIncludeAsync(
        Expression<Func<T, bool>> expression,
        params string[] includeProperties
    )
    {
        throw new NotImplementedException();
    }

    public async Task<int> RemoveAsync(T entity)
    {
        // Enforce Tenant Check
        if (entity.TenantId != _tenantId)
            return 0;

        await _collection.Document(entity.Id).DeleteAsync();
        await LogAuditAsync("Deleted", entity);
        return 1;
    }

    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        var batch = _context.Db.StartBatch();
        foreach (var entity in entities)
        {
            // Enforce Tenant Check
            if (entity.TenantId != _tenantId)
                continue;

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
        await LogAuditAsync("Updated", entity);
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

    public Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        params string[] includeProperties
    )
    {
        throw new NotImplementedException();
    }

    private async Task LogAuditAsync(string action, T entity)
    {
        // Prevent infinite loop: Don't audit the audit trail itself
        if (typeof(T) == typeof(AuditTrail))
            return;

        try
        {
            var user = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System/Anonymous";
            // Also try to get claim if Name is null
            if (user == "System/Anonymous")
            {
                var uid = _httpContextAccessor
                    .HttpContext?.User?.Claims.FirstOrDefault(c =>
                        c.Type == "user_id" || c.Type == "sub"
                    )
                    ?.Value;
                if (!string.IsNullOrEmpty(uid))
                    user = uid;
            }

            var audit = new AuditTrail
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = _tenantId,
                ActionName = action,
                ActionDescription = $"Entity {typeof(T).Name} (ID: {entity.Id}) was {action}.",
                Type = "Data",
                Module = typeof(T).Name,
                LoggedInUser = user,
                CreatedBy = user,
                Origin =
                    _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                    ?? "Unknown",
                ActionTime = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
            };

            // Async Push to Queue (Fast, with backpressure if full)
            await _auditService.EnqueueAuditLogAsync(audit);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audit Error: {ex.Message}");
        }
    }
}
