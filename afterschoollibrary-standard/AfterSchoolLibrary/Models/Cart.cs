using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary.Models
{
	/// <summary>
    /// Way to keep track of a user cart. A cart contains one or more activities.
    /// </summary>
	[Serializable]
    public class Cart : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public int? RegistrationId { get { return (int?)MDV["RegistrationId"].Value; } set { MDV["RegistrationId"].Value = value; } }
		

        public Cart()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Cart).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Cart).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "RegistrationId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        public static Cart LoadObject(System.Data.DataRow row)
        {
            Cart obj = new Cart();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Cart).Name);
				
                return dq;
            }
        }

        public static Cart Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Cart> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Cart.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Cart.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Cart).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Cart).Name);
        }

        public static SystemMessages Create(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            BasicLibrary.DataAccess.DatabaseParameters dps;

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Cart Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Cart.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static Cart LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public int? GradeLevel { get; set; }
        public GenderType? Gender { get; set; }
        public int? Age { get; set; }
        public Dictionary<int, string> QuestionAnswers { get; set; }

        public void ResetCartItems()
        {
            this._CartItems = null;
        }

        public static Cart GetNewCart(int? registrationId = null, WebUser loggedInUser = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var cart = new Cart
            {
                RegistrationId = registrationId,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0)
            };
            msgs.AddRange(cart.Save());

            return cart;
        }

        public CartItem AddCartItem(RegistrantActivity ra, SystemMessages msgs, DatabaseConnection dc = null)
        {

            var purchaseableObject = PurchaseableObject.FromParticipationObjectIdParticipationObjectTypeId(ra.ParticipationObjectId.Value, ParticipationObjectType.Activity);
            return AddCartItem(purchaseableObject, ra.ParticipationObjectId.Value, ParticipationObjectType.Activity, ra.ID.Value, null, msgs, dc);
        }

        public CartItem AddCartItem(Activity activity, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var purchaseableObject = PurchaseableObject.FromActivity(activity);
            return AddCartItem(
                purchaseableObject,
                activity.ID.Value,
                ParticipationObjectType.Activity,
                null,
                null,
                msgs,
                dc
                );
        }

        public CartItem AddCartItem(ParticipationObject participationObject, List<DateTime> selectedDates, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var purchaseableObject = PurchaseableObject.FromParticipationObject(participationObject, msgs);
            return AddCartItem(
                purchaseableObject,
                participationObject.ObjectID.Value,
                participationObject.ObjectType,
                null,
                selectedDates,
                msgs,
                dc
                );
        }

        public CartItem AddCartItem(PurchaseableObject purchaseableObject, int participationObjectId, ParticipationObjectType participationObjectType, int? registrantActivityId, List<DateTime> selectedDates, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            var alreadyExistsInCart = this.GetCartItems(msgs, dc)
                .Where(x => 
                    x.ParticipationObjectId == participationObjectId && 
                    x.ParticipationObjectTypeId == participationObjectType
                )
                .Any();

            if (!alreadyExistsInCart)
            {
                var item = new CartItem
                {
                    CartId = this.ID,
                    PurchaseableObjectId = purchaseableObject.ObjectID.Value,
                    PurchaseableObjectTypeId = purchaseableObject.ObjectType,
                    ParticipationObjectId = participationObjectId,
                    ParticipationObjectTypeId = participationObjectType,
                    RegistrantActivityId = registrantActivityId,
                    SelectedDates = selectedDates?.Select(x => x.ToShortDateTimeString()).ToStringList(","),
                    CreateDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    CreatedBy = 0,
                    LastModifiedBy = 0
                };
                msgs.AddRange(item.Save(dc));

                //force reload
                this.ResetCartItems();
                return item;
            }

            return this.GetCartItems(msgs, dc).Where(x =>
                    x.ParticipationObjectId == participationObjectId &&
                    x.ParticipationObjectTypeId == participationObjectType
                ).First();

        }

        public void MoveItemTo(int index, CartItem item)
        {
            this.GetCartItems().Remove(item);
            this.GetCartItems().Insert(index, item);
        }

        public void RemoveCartItem(Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            RemoveCartItem(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc);
        }

        public void RemoveCartItem(ParticipationObject po, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            RemoveCartItem(po.ObjectID.Value, po.ObjectType, msgs, dc);
        }

        public void RemoveCartItem(int participationObjectId, ParticipationObjectType participationObjectType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var cartItem = GetCartItems(msgs, dc)
                            .Where(item => item.ParticipationObjectId == participationObjectId && item.ParticipationObjectTypeId == participationObjectType)
                            .FirstOrDefault();

            if (cartItem.IsNotEmpty())
                msgs.AddRange(cartItem.Delete(dc));

            ResetCartItems();
        }

        private List<CartItem> _CartItems = null;
        public List<CartItem> GetCartItems(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_CartItems.IsEmpty())
                _CartItems = CartItem.LoadByCartId(this.ID.Value, msgs, dc);

            return _CartItems;
        }

        #endregion
    }
}
