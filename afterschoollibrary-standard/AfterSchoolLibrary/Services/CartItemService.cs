using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class CartItemService
    {
        public static ActivityGroup GetActivityGroup(this CartItem cartItem, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.Load(cartItem.ParticipationObjectId.Value, msgs, dc);
        }
        
        public static AccountActivityGroup GetAccountActivityGroup(this CartItem cartItem, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.Load(cartItem.ParticipationObjectId.Value, msgs, dc);
        }

        public static ParticipationObject GetParticipationObject(this CartItem cartItem, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (cartItem.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                return cartItem.GetActivity(msgs, dc).ToParticipationObject(msgs, dc);
            else if (cartItem.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
                return cartItem.GetActivityGroup(msgs, dc).ToParticipationObject(msgs, dc);
            else if (cartItem.ParticipationObjectTypeId == ParticipationObjectType.AccountActivityGroup)
                return cartItem.GetAccountActivityGroup(msgs, dc).ToParticipationObject(msgs, dc);

            return null;
        }
    }
}
