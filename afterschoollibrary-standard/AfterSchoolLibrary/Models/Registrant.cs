using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;
using static BasicLibrary.BaseWebUser;

namespace AfterSchoolLibrary.Models
{
	/// <summary>
    /// A single registrant for an activity, this is the main person who registered
    /// </summary>
	[Serializable]
    public class Registrant : BaseContact
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public int? WebUserId { get { return (int?)MDV["WebUserId"].Value; } set { MDV["WebUserId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public RegistrantType RegistrantTypeId { get { return (RegistrantType)MDV["RegistrantTypeId"].Value; } set { MDV["RegistrantTypeId"].Value = value; } }
		public int? ParentRegistrantId { get { return (int?)MDV["ParentRegistrantId"].Value; } set { MDV["ParentRegistrantId"].Value = value; } }
		public bool IsMaster { get { return (bool)MDV["IsMaster"].Value; } set { MDV["IsMaster"].Value = value; } }
		public bool IsContactOnly { get { return (bool)MDV["IsContactOnly"].Value; } set { MDV["IsContactOnly"].Value = value; } }
		public int? FirstRegistrantId { get { return (int?)MDV["FirstRegistrantId"].Value; } set { MDV["FirstRegistrantId"].Value = value; } }
		public bool IsParentOrGuardian { get { return (bool)MDV["IsParentOrGuardian"].Value; } set { MDV["IsParentOrGuardian"].Value = value; } }
		public bool AllowTextMessages { get { return (bool)MDV["AllowTextMessages"].Value; } set { MDV["AllowTextMessages"].Value = value; } }
		/// <summary>
		/// The location id of the school the registrant is attending
		/// </summary>
		public int? SchoolLocationId { get { return (int?)MDV["SchoolLocationId"].Value; } set { MDV["SchoolLocationId"].Value = value; } }
		/// <summary>
		/// The status of the profile - completed
		/// </summary>
		public RegistrantProfileStatus ProfileStatusId { get { return (RegistrantProfileStatus)MDV["ProfileStatusId"].Value; } set { MDV["ProfileStatusId"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		

        public Registrant()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Registrant).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.RegistrantTypeId = RegistrantType.NotSet;
			this.IsMaster = false;
			this.IsContactOnly = false;
			this.IsParentOrGuardian = false;
			this.AllowTextMessages = false;
			this.ProfileStatusId = RegistrantProfileStatus.NotSet;
			this.IsDeleted = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Registrant).Name;
                _dp = BaseContact.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "WebUserId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "RegistrantTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ParentRegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsMaster", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsContactOnly", System.Data.SqlDbType.Bit);
				_dp.Add(className, "FirstRegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsParentOrGuardian", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AllowTextMessages", System.Data.SqlDbType.Bit);
				_dp.Add(className, "SchoolLocationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ProfileStatusId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				
                return _dp;
            }
        }

        public static Registrant LoadObject(System.Data.DataRow row)
        {
            Registrant obj = new Registrant();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Registrant).Name);
				
                return dq;
            }
        }

        public static Registrant Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Registrant> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Registrant.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Registrant.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Registrant object", MessageType.Error);
			if (this.RegistrantTypeId.IsEmpty())
				msgs.Add("RegistrantTypeId is required on Registrant object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Registrant).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Registrant).Name);
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
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WebUserId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParentRegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsMaster"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsContactOnly"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FirstRegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsParentOrGuardian"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AllowTextMessages"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SchoolLocationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ProfileStatusId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Registrant Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Registrant.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Registrant> LoadByAccountIdByIsDeleted(int AccountId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByParentRegistrantId(int ParentRegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParentRegistrantId"], ParentRegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByAccountIdByRegistrantTypeIdByIsDeleted(int AccountId, RegistrantType RegistrantTypeId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["RegistrantTypeId"], RegistrantTypeId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByAccountIdByIsContactOnlyByIsDeleted(int AccountId, bool IsContactOnly, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsContactOnly"], IsContactOnly);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Registrant LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Registrant LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Registrant> LoadByWebUserIdByIsDeleted(int WebUserId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByWebUserIdByIsMasterByIsDeleted(int WebUserId, bool IsMaster, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["IsMaster"], IsMaster);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByFirstRegistrantId(int FirstRegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FirstRegistrantId"], FirstRegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByParentRegistrantIdByAccountId(int ParentRegistrantId, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParentRegistrantId"], ParentRegistrantId);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"        

        public bool IsProfileCompleted
        {
            get
            {
                return this.ProfileStatusId == RegistrantProfileStatus.Completed;
            }
        }

        public string Name => BuildDisplayName(FirstName, LastName);

        public List<FormQuestionAnswer> GetAnswers(int activityId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByRegistrantIdByActivityId(this.ID.Value, activityId, dc: dc)
                .OrderBy(x => x.FormQuestionDisplayOrder)
                .ToList();
        }

        public List<FormQuestionAnswer> GetAnswers(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByRegistrantId(this.ID.Value, msgs, dc)
                .OrderBy(x => x.FormQuestionDisplayOrder)
                .ToList();
        }

        public FormQuestionAnswer GetAnswer(QuestionDataType questionDataType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.Load(
                        criterias: new Hashtable() { { "QuestionDataTypeId", (int)questionDataType }, { "RegistrantId", this.ID.Value } }, 
                        msgs: msgs,
                        dc: dc)
                    .FirstOrDefault();
        }

        public string GetValue(QuestionDataType questionDataType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            string s = GetAnswer(questionDataType, msgs, dc)?.Text.IfEmpty("-");
            if (s.IsNotEmpty() && AfterSchoolLibrary.Helpers.Utilities.IsAlreadyJsonString(s))
                return AfterSchoolLibrary.Helpers.Utilities.GetKeyValuePairs(s).FirstOrDefault()?.value;

            return s;
        }

        public static List<Registrant> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public List<RegistrantActivity> GetRegistrantActivities(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByRegistrantId(this.ID.Value, msgs, dc).ToList();
        }

        public List<RegistrantActivity> GetRegistrantActivities(List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByRegistrantId(this.ID.Value, assignmentTypes, msgs, dc).ToList();
        }

        private static DatabaseQuery SearchRegistrantsQuery(string searchText, int? contactListId, int accountId, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);

            if (contactListId.HasValue)
            {
                var contactListQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ContactListRegistrant).Name);
                contactListQuery.UseParameterizedQuery = false;
                contactListQuery.SelectParameters.Clear();
                contactListQuery.SelectParameters.Add(ContactListRegistrant.ObjectMap["RegistrantId"]);
                contactListQuery.Criterias.Add(ContactListRegistrant.ObjectMap["contactListId"], contactListId.Value);

                dq.Criterias.Add(ObjectMap["Id"], contactListQuery, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            }

            if (searchText.IsNotEmpty())
            {
                var itemList = new List<string>();

                // split list
                itemList.AddRange(searchText.Split(' ').Select(x => x.Trim()).Where(x => x.IsNotBlank()));

                var dcsAnd = new DatabaseCriterias(LogicalOperatorType.And);

                foreach (var item in itemList)
                {
                    var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                    dcs.Add(ObjectMap["FirstName"], item, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.Like);
                    dcs.Add(ObjectMap["LastName"], item, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.Like);
                    dcs.Add(ObjectMap["Email"], item, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.Like);
                    dcsAnd.Add(dcs);
                }

                dq.Criterias.Add(dcsAnd);
            }

            dq.OrderBys.Add(ObjectMap["FirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["LastName"], OrderByDirectionEnum.Ascending);

            return dq;
        }

        public static int SearchRegistrantCount(string searchText, int? contactListId, int accountId, DatabaseConnection dc = null)
        {
            return SearchRegistrantsQuery(searchText, contactListId, accountId, dc: dc).GetCount();
        }
        public static List<Registrant> SearchRegistrants(string searchText, int? contactListId, int accountId, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = SearchRegistrantsQuery(searchText, contactListId, accountId, dc: dc);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByAccountIdByIsDeleted(int AccountId, bool isDeleted, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.GetCount();
        }                       

        public static Registrant LoadByNameByEmail(int accountId, string firstName, string lastName, string email, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["accountId"], accountId);
            dq.Criterias.Add(ObjectMap["firstName"], firstName);
            dq.Criterias.Add(ObjectMap["lastName"], lastName);
            dq.Criterias.Add(ObjectMap["email"], email);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Registrant LoadByEmailOnly(int accountId, string email, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            dq.Criterias.Add(ObjectMap["accountId"], accountId);
            dq.Criterias.Add(ObjectMap["email"], email);

            var dcs1 = new DatabaseCriterias(LogicalOperatorType.Or);
            dcs1.Add(ObjectMap["firstname"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dcs1.Add(ObjectMap["firstname"], "");
            dq.Criterias.Add(dcs1);

            var dcs2 = new DatabaseCriterias(LogicalOperatorType.Or);
            dcs2.Add(ObjectMap["lastname"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dcs2.Add(ObjectMap["lastname"], "");
            dq.Criterias.Add(dcs2);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }


        //public List<Activity> GetActivitiesByDay(DayOfWeekASL day, DatabaseConnection dc = null)
        //{
        //    return Activity.GetActivitiesByRegistrantByDay(this.ID.Value, day, DateTime.UtcNow, dc: dc);
        //}

        public static List<Registrant> LoadFirstRegistrants(int accountId, int? start, int? length, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            dq.Criterias.Add(ObjectMap["FirstRegistrantId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.OrderBys.Add(ObjectMap["FirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["LastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public SystemMessages FindAndSetFirstRegistrant(WebUser loggedInUser, DatabaseConnection dc = null)
        {
            return FindAndSetFirstRegistrant(this.FirstName, this.LastName, this.Email, loggedInUser, dc);
        }

        public SystemMessages FindAndSetFirstRegistrant(string firstName, string lastName, string email, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], this.AccountId);
            dq.Criterias.Add(ObjectMap["FirstRegistrantId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.Criterias.Add(ObjectMap["FirstName"], firstName);
            dq.Criterias.Add(ObjectMap["LastName"], lastName);
            dq.Criterias.Add(ObjectMap["Email"], email);
            dq.Criterias.Add(ObjectMap["ID"], this.ID.Value, DatabaseCriteriaOperatorEnum.NotEqualTo);
            dq.OrderBys.Add(ObjectMap["Id"], OrderByDirectionEnum.Ascending);

            var firstRegistrant = dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();

            if (firstRegistrant.IsNotEmpty())
            {
                this.FirstRegistrantId = firstRegistrant.ID;
                this.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
                this.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(this.Save(dc));
            }

            return msgs;
        }

        public static List<Registrant> FindRelatedRegistrants(int AccountId, string firstName, string lastName, string email, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["FirstName"], firstName);
            dq.Criterias.Add(ObjectMap["LastName"], lastName);
            dq.Criterias.Add(ObjectMap["Email"], email);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadMastersNotRecentlyUpdated(DateTime lastUpdatedDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsMaster"], true);
            dq.Criterias.Add(ObjectMap["LastModifiedDate"], lastUpdatedDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public Registrant Copy(WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var copy = new Registrant();
            copy.CopyDataValues(this.OriginalDataValues, this.ModifiedDataValues);
            copy.CreateDate = copy.LastModifiedDate = DateTime.UtcNow;
            copy.CreatedBy = copy.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            copy.Uid = Guid.NewGuid();
            copy.ResetID();
            msgs.AddRange(copy.Save(dc: dc));

            return copy;
        }

        public string GetFirstNameLastInitial()
        {
            if (this.FirstName.Length < 3 )
                return $"{this.FirstName} {this.LastName.MaskString(1)}.";
            
            return $"{this.FirstName.MaskString(Math.Max(this.FirstName.Length - 3, 3))} {this.LastName.MaskString(1)}.";
        }

        #endregion
    }
}
