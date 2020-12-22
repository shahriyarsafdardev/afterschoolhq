using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class PurchaseableObjectService
    {
        public static ParticipationObject GetParticipationObject(this PurchaseableObject purchaseableObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (purchaseableObject.ParticipationObject.IsNotEmpty())
                return purchaseableObject.ParticipationObject;

            if (purchaseableObject.ObjectID.HasValue && purchaseableObject.ObjectType.IsNotEmpty())
            {
                switch (purchaseableObject.ObjectType)
                {
                    case PurchaseableObjectType.AccountActivityGroup:
                        return AccountActivityGroup.Load(purchaseableObject.ObjectID.Value, msgs, dc).GetParticipationObject(msgs, dc);
                    case PurchaseableObjectType.ActivityGroup:
                        return ActivityGroup.Load(purchaseableObject.ObjectID.Value, msgs, dc).GetParticipationObject(msgs, dc);
                    case PurchaseableObjectType.Activity:
                        return Activity.Load(purchaseableObject.ObjectID.Value, msgs, dc).GetParticipationObject(msgs, dc);

                    default:
                        return null;

                }
            }

            return null;
        }
    }
}
