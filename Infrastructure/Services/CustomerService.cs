using Core.Models;
using Infrastructure.InterFace;
using Infrastructure.InterFace.Services;
using Infrastructure.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork<Customer> customerUnitOfWork;

        public CustomerService(IUnitOfWork<Customer> CustomerUnitOfWork)
        {
            customerUnitOfWork = CustomerUnitOfWork;
        }

        public Task<string> GetCustomerInfo(CustomerViewModel model)
        {
            if (model == null)
                return Task.FromResult("");

            var info = $"الاسم: {model.Name}, الهاتف: {model.PhoneNumber}, العنوان: {model.Address}";
            return Task.FromResult(info);
        }
    }
}
