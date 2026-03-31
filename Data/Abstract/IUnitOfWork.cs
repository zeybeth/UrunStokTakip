using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entity;

namespace Data.Abstract;

public interface IUnitOfWork : IAsyncDisposable
{
 
    IRepository<Product> Products { get; }
    IRepository<Category> Categories { get; }
    IRepository<Cart> Carts { get; }
    IRepository<Sale> Sales { get; }

    IRepository<T> GetRepository<T>() where T : class;

    Task<int> SaveAsync();
}