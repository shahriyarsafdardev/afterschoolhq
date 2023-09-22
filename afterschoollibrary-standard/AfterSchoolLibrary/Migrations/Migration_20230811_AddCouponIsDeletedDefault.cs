using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230811_AddCouponIsDeletedDefault
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Coupon.Update(
                        new Hashtable() { { "IsDeleted", 0 } },
                        new Hashtable() { { "IsDeleted", DBNull.Value } },
                        Coupon.ObjectMap,
                        adc
                    ));
                },
                uniqueIdenfier: "Migration_20230811_AddCouponIsDeletedDefault",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
