using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;
using AfterSchoolLibrary.Services;
using System.Numerics;
using AfterSchoolLibrary.Helpers;

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

        private bool IsValidCartItem(PurchaseableObjectType purchaseableObjectType, int participationObjectId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (purchaseableObjectType is PurchaseableObjectType.Activity)
            {
                var cartItems = GetCartItems(msgs, dc);
                if (cartItems.Count == 0)
                    return true;
                var newActivity = Activity.Load(participationObjectId, msgs, dc);
                foreach (var item in cartItems)
                {
                    var cartActivity = Activity.Load(item.ParticipationObjectId??0, msgs, dc);
                    if(!ValidateActivityRestriction(newActivity, cartActivity, msgs))
                        return false;
                    if(!ValidateActivityBySchedule(newActivity, cartActivity, msgs, dc))
                        return false;
                }
            }
            return true;
        }

        private bool ValidateActivityRestriction(Activity activity1, Activity activity2, SystemMessages msgs)
        {
            var isRestrictedByGrade = activity1.IsRestrictedByGrade() || activity2.IsRestrictedByGrade();
            var isRestrictedByAge = activity1.IsRestrictedByAge() || activity2.IsRestrictedByAge();

            var isValidAgeRestriction = activity1.IsRestrictedByAge() && activity2.IsRestrictedByAge();
            var isValidGradeRestriction = activity1.IsRestrictedByGrade() && activity2.IsRestrictedByGrade();

            var gradeValidated = false;
            var ageValidated = false;

            if (isValidAgeRestriction || isValidGradeRestriction)
            {
                if (isRestrictedByGrade)
                    gradeValidated = OverlappingUtilities.HasOverlappingVecotorRanges(
                        new Vector2 { X = activity1.GradeLevelStart ?? 0, Y = activity1.GradeLevelEnd ?? 0 },
                        new Vector2 { X = activity2.GradeLevelStart ?? 0, Y = activity2.GradeLevelEnd ?? 0 }
                        );
                else if (isRestrictedByAge)
                    ageValidated = OverlappingUtilities.HasOverlappingVecotorRanges(
                        new Vector2 { X = activity1.AgeStart ?? 0, Y = activity1.AgeEnd ?? 0 },
                        new Vector2 { X = activity2.AgeStart ?? 0, Y = activity2.AgeEnd ?? 0 }
                        );
                else
                    ageValidated = gradeValidated = false;
            }

            bool genderValidated = ValidateGender(activity1, activity2);
            var errorMsg = "";
            if (!gradeValidated && isRestrictedByGrade)
                errorMsg += "Grade";
            if (!ageValidated && isRestrictedByAge)
                errorMsg += errorMsg.IsNotEmpty() ? ", Age" : "Age";
            if (!genderValidated)
                errorMsg += errorMsg.IsNotEmpty() ? " & Gender" : "Gender";
            if (errorMsg.IsNotEmpty())
                msgs.Add(new SystemMessage($"Conflicting Activity {errorMsg} with cart items!", MessageType.UserError));
            return genderValidated && (ageValidated || gradeValidated);
        }

        private bool ValidateActivityBySchedule(Activity activity1, Activity activity2, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var hasError = false;
            if (activity1.HasSchedule || activity2.HasSchedule)
            {
                if (activity1.HasSchedule && activity2.HasSchedule)
                {
                    var activity1Schedule = ActivitySchedule.Load(activity1.ActivityScheduleId.Value,msgs,dc);
                    var activity2Schedule = ActivitySchedule.Load(activity2.ActivityScheduleId.Value, msgs, dc);
                    if (activity1Schedule.RepeatsWeekly && activity2Schedule.RepeatsWeekly)
                    {
                        var schedule1Range = new DateRange()
                        {
                            StartDate = activity1Schedule.StartDate ?? DateTime.MinValue,
                            EndDate = activity1Schedule.EndDate ?? DateTime.MaxValue
                        };
                        var schedule2Range = new DateRange()
                        {
                            StartDate = activity2Schedule.StartDate ?? DateTime.MinValue,
                            EndDate = activity2Schedule.EndDate ?? DateTime.MaxValue
                        };
                        if (OverlappingUtilities.HasOverlappingDateRanges(schedule1Range, schedule2Range))
                        {
                            var activity1Days = activity1Schedule.GetDaysOfTheWeek(msgs, dc);
                            var activity2Days = activity2Schedule.GetDaysOfTheWeek(msgs, dc);
                            if (OverlappingUtilities.HasOverlappingLists(activity1Days, activity2Days))
                            {
                                var timeRange1 = new TimeRange()
                                {
                                    StartTime = (activity1Schedule.DailyStartTime ?? DateTime.MinValue).TimeOfDay,
                                    EndTime = (activity1Schedule.DailyEndTime ?? DateTime.MaxValue).TimeOfDay
                                };
                                var timeRange2 = new TimeRange()
                                {
                                    StartTime = (activity2Schedule.DailyStartTime ?? DateTime.MinValue).TimeOfDay,
                                    EndTime = (activity2Schedule.DailyEndTime ?? DateTime.MaxValue).TimeOfDay
                                };
                                if (OverlappingUtilities.HasOverlappingTimeRanges(timeRange1, timeRange2))
                                    hasError = true;
                            }
                        }
                    }
                    else if (!activity1Schedule.RepeatsWeekly && !activity2Schedule.RepeatsWeekly)
                    {
                        var sessionInstance1 = activity1Schedule.GetUpcomingInstance(msgs, dc);
                        var sessionInstance2 = activity2Schedule.GetUpcomingInstance(msgs, dc);
                        var overlappingInstance = OverlappingUtilities.GetOverlappingInstanceDates(sessionInstance1, sessionInstance2);
                        if (OverlappingUtilities.HasOverlapingInstanceTimes(overlappingInstance))
                            hasError = true;
                    }
                    else if (!activity1Schedule.RepeatsWeekly || !activity2Schedule.RepeatsWeekly)
                    {
                        var sessionInstance = (!activity1Schedule.RepeatsWeekly ? activity1Schedule : activity2Schedule).GetUpcomingInstance(msgs, dc);
                        var dateRangeActivity = activity1Schedule.RepeatsWeekly ? activity1Schedule : activity2Schedule;
                        var dateRange = new DateRange()
                        {
                            StartDate = dateRangeActivity.StartDate ?? DateTime.MinValue,
                            EndDate = dateRangeActivity.EndDate ?? DateTime.MaxValue
                        };
                        var matchingDates = sessionInstance.Where(ins => (ins.StartDateTime?.Date ?? DateTime.MinValue).IsInRange(dateRange));
                        hasError = matchingDates.Any(dates => OverlappingUtilities.InstanceScheduleTimeOverlaps(dates, dateRangeActivity));
                    }
                    else
                        hasError = true;
                }
            }
            if (hasError)
                msgs.Add(new SystemMessage("Conflicting Activity Schedule with cart items!", MessageType.UserError));
            return !hasError;
        }

        private bool ValidateGender(Activity activity1, Activity activity2)
        {
            if(activity1.IsGenderEnforced && activity2.IsGenderEnforced)
            {
                var activity1AllowedGenders = activity1.GetAllowedGenders();
                return activity2.GetAllowedGenders().All(gender => activity1AllowedGenders.Contains(gender));
            }
            else if (activity1.IsGenderEnforced || activity2.IsGenderEnforced)
                return false;
            return true;
        }

        public CartItem AddCartItem(PurchaseableObject purchaseableObject, int participationObjectId, ParticipationObjectType participationObjectType, int? registrantActivityId, List<DateTime> selectedDates, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            if (!IsValidCartItem(purchaseableObject.ObjectType, participationObjectId, msgs, dc))
                return null;

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
