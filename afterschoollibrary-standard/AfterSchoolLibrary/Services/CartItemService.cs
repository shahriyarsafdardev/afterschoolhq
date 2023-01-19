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
            return cartItem.ParticipationObjectTypeId switch
            {
                ParticipationObjectType.Activity => cartItem.GetActivity(msgs, dc).GetParticipationObject(msgs, dc),
                ParticipationObjectType.ActivityGroup => cartItem.GetActivityGroup(msgs, dc).GetParticipationObject(msgs, dc),
                ParticipationObjectType.AccountActivityGroup => cartItem.GetAccountActivityGroup(msgs, dc).GetParticipationObject(msgs, dc),
                _ => null,
            };
        }
    }
}
