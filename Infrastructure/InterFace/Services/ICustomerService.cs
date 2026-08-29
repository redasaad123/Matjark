using Infrastructure.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.InterFace.Services
{
    public interface ICustomerService
    {
        Task<string> GetCustomerInfo(CustomerViewModel model);


    }
}
