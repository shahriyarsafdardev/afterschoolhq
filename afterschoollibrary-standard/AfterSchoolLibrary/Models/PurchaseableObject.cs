using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Models
{
    public class PurchaseableObject
    {
        public object Object { get; set; }
        public int? ObjectID { get; set; }
        public PurchaseableObjectType ObjectType { get; set; }
        public string Name { get; set; }

        public bool IsActivity()
        {
            return ObjectType == PurchaseableObjectType.Activity;
        }

        public bool IsActivityGroup()
        {
            return ObjectType == PurchaseableObjectType.ActivityGroup;
        }

        public bool IsAccountActivityGroup()
        {
            return ObjectType == PurchaseableObjectType.AccountActivityGroup;
        }

        public static PurchaseableObject FromPurchaseableObjectCoupon(PurchaseableObjectCoupon purchaseableObjectCoupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var po =  new PurchaseableObject()
            {
                ObjectID = purchaseableObjectCoupon.PurchaseableObjectId,
                ObjectType = purchaseableObjectCoupon.PurchaseableObjectTypeId,
            };
            po.LoadObject(msgs, dc);
            return po;
        }

        private void LoadObject(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            switch(ObjectType)
            {
                case PurchaseableObjectType.Activity:
                    this.Object = Activity.Load(this.ObjectID.Value, msgs, dc);
                    this.Name = ((Activity)this.Object).Name;
                    break;
                case PurchaseableObjectType.ActivityGroup:
                    this.Object = ActivityGroup.Load(this.ObjectID.Value, msgs, dc);
                    this.Name = ((ActivityGroup)this.Object).Name;
                    break;
                case PurchaseableObjectType.AccountActivityGroup:
                    this.Object = AccountActivityGroup.Load(this.ObjectID.Value, msgs, dc);
                    this.Name = ((AccountActivityGroup)this.Object).GetActivityGroup(msgs, dc).Name;
                    break;
            }
        }

        public static PurchaseableObject FromActivity(Activity activity)
        {
            return new PurchaseableObject()
            {
                Object = activity,
                ObjectID = activity.ID,
                ObjectType = PurchaseableObjectType.Activity,
                Name = activity.Name,
            };
        }

        public static PurchaseableObject FromActivityGroup(ActivityGroup activityGroup)
        {
            return new PurchaseableObject()
            {
                Object = activityGroup,
                ObjectID = activityGroup.ID,
                ObjectType = PurchaseableObjectType.ActivityGroup,
                Name = activityGroup.Name,
            };
        }

        public static PurchaseableObject FromAccountActivityGroup(AccountActivityGroup accountActivityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new PurchaseableObject()
            {
                Object = accountActivityGroup,
                ObjectID = accountActivityGroup.ID,
                ObjectType = PurchaseableObjectType.AccountActivityGroup,
                Name = accountActivityGroup.GetActivityGroup(msgs, dc).Name,
            };
        }
    }
}
