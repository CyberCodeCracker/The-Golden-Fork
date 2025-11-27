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

        private readonly AppDbContext _context;
        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        } 

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync() 
            => await _context.Set<T>().AsNoTracking().ToListAsync(); // As no tracking pour les performances en lecture seule

        // Eager loading avec les entites lies ( usables pour les plat et leur photos )
        public async Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().AsQueryable();
            foreach(var item in includes)
            {
                query = query.Include(item);
            }
            return await query.ToListAsync();   
        }

        public async Task<T> GetByIdAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id); 
            return entity;
        }

        // Récupère une entité par son identifiant en chargeant optionnellement ses propriétés de navigation.
        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().AsQueryable();
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
            var entity = await query.FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id);
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
