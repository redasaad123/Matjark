using Infrastructure.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    internal class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDBContext context;

        public GenericRepository(AppDBContext context)
        {
            this.context = context;
        }
    }
}
