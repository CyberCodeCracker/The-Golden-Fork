using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.IRepositorie;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {

        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }
        public async Task<IQueryable<T>> GetAllAsyncWithFilter(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool noTracking = false)
        {
            IQueryable<T> query = _dbSet;

            if (noTracking)
                query = query.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await Task.FromResult(query);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> GetByIdAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }

        public async Task<T?> GetBylastAsync(Expression<Func<T, string>> orderBySelector)
        {
            return await _dbSet
                .OrderByDescending(orderBySelector)
                .FirstOrDefaultAsync();
        }

        public IQueryable<T> GetQueryable()
        {
            return _context.Set<T>().AsQueryable();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            // SaveChangesAsync doit être appelé dans le service ou UnitOfWork
        }

        public async Task AddListAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteByStringIdAsync(string id)
        {
            var result = await GetByIdAsync(id);

            if (result == null)
                throw new KeyNotFoundException($"{typeof(T).Name} avec id '{id}' non trouvé.");

            _dbSet.Remove(result);
            await Task.CompletedTask;
        }

        public async Task DeleteByIntIdAsync(int id)
        {
            var result = await GetByIdAsync(id);

            if (result == null)
                throw new KeyNotFoundException($"{typeof(T).Name} avec id '{id}' non trouvé.");

            _dbSet.Remove(result);
            await Task.CompletedTask;
        }

        public async Task DeleteListAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<string> CreateAndLogAsync(T entity)
        {
            try
            {
                var properties = typeof(T).GetProperties();
                var defaultInstance = Activator.CreateInstance<T>();

                List<string> fieldValues = new List<string>();

                foreach (var property in properties)
                {
                    var value = property.GetValue(entity);
                    var defaultValue = property.GetValue(defaultInstance);

                    if (value != null && !value.Equals(defaultValue))
                    {
                        fieldValues.Add($"{property.Name}={value}");
                    }
                }

                string logMessage = $"Created {typeof(T).Name} with " + string.Join(", ", fieldValues);
                return await Task.FromResult(logMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating entity {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        public async Task<string> UpdateGeneral(T source, T dest, List<string>? fieldsToUpdate = null)
        {
            try
            {
                var properties = typeof(T).GetProperties();
                List<string> changes = new List<string>();

                foreach (var property in properties)
                {
                    if (fieldsToUpdate != null && !fieldsToUpdate.Contains(property.Name))
                        continue;

                    var sourceValue = property.GetValue(source);
                    var destValue = property.GetValue(dest);

                    if (!Equals(sourceValue, destValue) && destValue != null)
                    {
                        changes.Add($"Changed {property.Name} from '{sourceValue}' to '{destValue}'");
                        property.SetValue(source, destValue);
                    }
                }

                await _context.SaveChangesAsync();

                return changes.Count > 0 ? string.Join(", ", changes) : "No changes made.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur update : {ex.Message}", ex);
            }
        }

        // Alias pour AddAsync si tu veux garder InsertAsync
        public Task InsertAsync(T entity)
        {
            return AddAsync(entity);
        }
    }
}
