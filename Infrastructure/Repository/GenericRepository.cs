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

        public async Task<T> GetById(string Id)
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
            var qurey = await _dbSet.AnyAsync(predicate);
            return qurey;
        }

        public bool Delete(T entity)
        {
            _dbSet.Remove(entity);    
            return true;
        }

        public T Find(Expression<Func<T, bool>> predicate)
        {
            var qurey = _dbSet.Where(predicate).FirstOrDefault();
            return qurey;

            
        }

        public async Task<object> Find(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> Object)
        {
            var qurey = await _dbSet.Where(predicate).Select(predicate).FirstOrDefaultAsync();
            return qurey;
        }

        public Task<List<object>> FindAll(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> Object)
        {
            var qurey = _dbSet.Where(predicate).Select(Object).ToListAsync();
            return qurey;
        }

        public async Task<List<object>> FindAll(Expression<Func<T, object>> Object)
        {
            var qurey = await _dbSet.Select(Object).ToListAsync();
            return qurey;
        }

        public async Task<List<T>> FindAll(Expression<Func<T, bool>> predicate)
        {
            var qurey = await _dbSet.Where(predicate).ToListAsync();
            return qurey;
        }

        public async Task<List<string>> FindAll(Expression<Func<T, bool>> predicate, Expression<Func<T, string>> Object)
        {
            var qurey = await _dbSet.Where(predicate).Select(Object).ToListAsync();
            return qurey;
        }

        

        public async Task<object> Mapping(Expression<Func<T, object>> Object)
        {
            var qurey =  _dbSet.Select(Object);
            return qurey;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return entity;
        }
    }
}
