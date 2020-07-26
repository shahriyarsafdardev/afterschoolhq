using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class SubscriberToPolymorphicRelationship_20200726
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var subscribers = Subscriber.Load(msgs: amsgs, dc: adc);

                    subscribers.ForEach(subscriber => {
                        if (subscriber.AccountId.HasValue)
                        {
                            subscriber.ObjectId = subscriber.AccountId;
                            subscriber.ObjectTypeId = AHQObjectType.Account;
                            amsgs.AddRange(subscriber.Save(adc));
                        }
                    });
                },
                uniqueIdenfier: "SubscriberToPolymorphicRelationship_20200726",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
