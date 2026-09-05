using Infrastructure.InterFace;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    internal class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDBContext context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDBContext context)
        {
            this.context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetById(string Id)
        {
            return await _dbSet.FindAsync(Id);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity); 
            return entity;
        }

        public async Task<bool> Any(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public bool Delete(T entity)
        {
            if (entity == null) return false;
            _dbSet.Remove(entity);    
            return true;
        }

        public T? Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).FirstOrDefault();
        }

        public async Task<object?> Find(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> Object)
        {
            return await _dbSet.Where(predicate).Select(Object).FirstOrDefaultAsync();
        }

        public async Task<List<object>> FindAll(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> Object)
        {
            return await _dbSet.Where(predicate).Select(Object).ToListAsync();
        }

        public async Task<List<object>> FindAll(Expression<Func<T, object>> Object)
        {
            return await _dbSet.Select(Object).ToListAsync();
        }

        public async Task<List<T>> FindAll(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<List<string>> FindAll(Expression<Func<T, bool>> predicate, Expression<Func<T, string>> Object)
        {
            return await _dbSet.Where(predicate).Select(Object).ToListAsync();
        }

        public Task<object> Mapping(Expression<Func<T, object>> Object)
        {
            return Task.FromResult<object>(_dbSet.Select(Object));
        }

        public Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.FromResult(entity);
        }
    }
}
