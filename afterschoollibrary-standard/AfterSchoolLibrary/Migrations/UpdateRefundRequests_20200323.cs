using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateRefundRequests_20200323
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var requests = RefundRequest.Load(msgs: amsgs, dc: adc);

                    requests.ForEach(request => {
                        if (request.RequestFulfilled)
                            request.UpdateStatus(RefundRequestStatus.Completed, new WebUser() { ID = 1 }, amsgs, adc);
                        else
                            request.UpdateStatus(RefundRequestStatus.RequestedByProvider, new WebUser() { ID = request.CreatedBy }, amsgs, adc);
                    });
                },
                uniqueIdenfier: "UpdateRefundRequests_20200323",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
