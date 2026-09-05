using Infrastructure.InterFace;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : class
    {
        private readonly AppDBContext context;

        private IGenericRepository<T> _entity;

        public UnitOfWork(AppDBContext context)
        {
            this.context = context;
        }
        public IGenericRepository<T> Entity => _entity ??= new GenericRepository<T>(context);


        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
