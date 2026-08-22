using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.InterFace
{
    public interface IUnitOfWork <T> where T : class
    {
        IGenericRepository<T> Entity { get; }

        void SaveChanges();
    }
}
