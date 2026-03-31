using Data.Abstract;
using Entity;
using System.Collections.Concurrent;

namespace Data.Concrete;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
   
    private readonly ConcurrentDictionary<Type, object> _repositories;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _repositories = new ConcurrentDictionary<Type, object>();
    }

    //  her seferinde 'new'lemek yerine merkezi metot
    public IRepository<Product> Products => GetRepository<Product>();
    public IRepository<Category> Categories => GetRepository<Category>();
    public IRepository<Cart> Carts => GetRepository<Cart>();
    public IRepository<Sale> Sales => GetRepository<Sale>();

    public IRepository<T> GetRepository<T>() where T : class
    {
       
        return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_context));
    }

    public async Task<int> SaveAsync()
    {
       
        return await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}