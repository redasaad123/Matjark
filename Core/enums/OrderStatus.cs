using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.enums
{
    public enum OrderStatus
    {
        Pending = 0,           // الطلب قيد الانتظار
        Completed = 1,         // تم التعبئة
        MissingItems = 2,      // فيه حاجة ناقصة
        DeliveredToSupplier = 3 // تم التسليم للمورد
    }
}
