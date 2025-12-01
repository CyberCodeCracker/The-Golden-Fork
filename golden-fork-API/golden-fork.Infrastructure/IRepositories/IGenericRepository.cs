using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.IRepositorie 
{
    public interface IGenericRepository<T> where T : class 
    {

        Task<IReadOnlyList<T>> GetAllAsync();

        Task<IQueryable<T>> GetAllAsyncWithFilter(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool noTracking = false);

        Task<T?> GetByIdAsync(int id);
        public Task<T?> GetBylastAsync(Expression<Func<T, string>> orderBySelector);

        Task<T?> GetByIdAsync(params object[] keyValues);

        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);

        T Get(Expression<Func<T, bool>> predicate);

        IQueryable<T> GetQueryable();


        Task AddAsync(T entity);

        Task AddListAsync(IEnumerable<T> entities);

        Task InsertAsync(T entity); // alias optionnel

        Task UpdateAsync(T entity);

        Task<string> UpdateGeneral(T source, T dest, List<string>? fieldsToUpdate = null);

        Task DeleteAsync(T entity);

        Task DeleteByStringIdAsync(string id);

        Task DeleteByIntIdAsync(int id);

        Task DeleteListAsync(IEnumerable<T> entities);

        Task SaveChangesAsync();

        Task<string> CreateAndLogAsync(T entity); // si tu as un système d'historique
    }
}
