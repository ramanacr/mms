using MeetingScheduler.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MeetingScheduler.Api.Repositories;

public sealed class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;

    public EfRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<T> Query() => _dbContext.Set<T>();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Set<T>().FindAsync([id], cancellationToken).AsTask();

    public Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<T>().AsQueryable();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        _dbContext.Set<T>().AddAsync(entity, cancellationToken).AsTask();

    public void Update(T entity) => _dbContext.Set<T>().Update(entity);

    public void Remove(T entity) => _dbContext.Set<T>().Remove(entity);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
