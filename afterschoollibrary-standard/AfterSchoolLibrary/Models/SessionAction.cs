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
    /// An action taken during a particular user session. Not a logged in session, but a session where a user hits the site and takes action, such as registering, updating a record, etc.
    /// </summary>
	
    public class SessionAction : BasicObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? SessionId { get { return (int?)MDV["SessionId"].Value; } set { MDV["SessionId"].Value = value; } }
		public SessionActionType SessionActionTypeId { get { return (SessionActionType)MDV["SessionActionTypeId"].Value; } set { MDV["SessionActionTypeId"].Value = value; } }
		public int? WebUserId { get { return (int?)MDV["WebUserId"].Value; } set { MDV["WebUserId"].Value = value; } }
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		public int? RegistrationId { get { return (int?)MDV["RegistrationId"].Value; } set { MDV["RegistrationId"].Value = value; } }
		public int? RegistrationPeriodId { get { return (int?)MDV["RegistrationPeriodId"].Value; } set { MDV["RegistrationPeriodId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public string RegistrantFirstName { get { return (string)MDV["RegistrantFirstName"].Value; } }
		public string RegistrantLastName { get { return (string)MDV["RegistrantLastName"].Value; } }
		public string RegistrantEmail { get { return (string)MDV["RegistrantEmail"].Value; } }
		public string RegistrantPhone { get { return (string)MDV["RegistrantPhone"].Value; } }
		public string RegistrantCompany { get { return (string)MDV["RegistrantCompany"].Value; } }
		public Guid RegistrantUid { get { return (Guid)MDV["RegistrantUid"].Value; } }
		public RegistrantType RegistrantRegistrantTypeId { get { return (RegistrantType)MDV["RegistrantRegistrantTypeId"].Value; } }
		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
		public string WebUserUsername { get { return (string)MDV["WebUserUsername"].Value; } }
		public string WebUserEmail { get { return (string)MDV["WebUserEmail"].Value; } }
		public string WebUserFirstName { get { return (string)MDV["WebUserFirstName"].Value; } }
		public string WebUserLastName { get { return (string)MDV["WebUserLastName"].Value; } }
		public string CreatedByUsername { get { return (string)MDV["CreatedByUsername"].Value; } }
		public string CreatedByEmail { get { return (string)MDV["CreatedByEmail"].Value; } }
		public string CreatedByFirstName { get { return (string)MDV["CreatedByFirstName"].Value; } }
		public string CreatedByLastName { get { return (string)MDV["CreatedByLastName"].Value; } }
		public string ActivityGroupName { get { return (string)MDV["ActivityGroupName"].Value; } }
		

        public SessionAction()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(SessionAction).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.SessionActionTypeId = SessionActionType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(SessionAction).Name;
                _dp = BasicObject.get_BaseObjectMap(className);
				_dp.Add(className, "SessionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "SessionActionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "WebUserId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationPeriodId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantFirstName", typeof(Registrant).Name, "FirstName", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantLastName", typeof(Registrant).Name, "LastName", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantEmail", typeof(Registrant).Name, "Email", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantPhone", typeof(Registrant).Name, "Phone", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantCompany", typeof(Registrant).Name, "Company", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantUid", typeof(Registrant).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Registrant");
				_dp.Add(className, "RegistrantRegistrantTypeId", typeof(Registrant).Name, "RegistrantTypeId", System.Data.SqlDbType.Int, "Registrant");
				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "WebUserUsername", typeof(WebUser).Name, "Username", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserEmail", typeof(WebUser).Name, "Email", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserFirstName", typeof(WebUser).Name, "FirstName", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserLastName", typeof(WebUser).Name, "LastName", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "CreatedByUsername", typeof(WebUser).Name, "Username", System.Data.SqlDbType.NVarChar, "CreatedBy");
				_dp.Add(className, "CreatedByEmail", typeof(WebUser).Name, "Email", System.Data.SqlDbType.NVarChar, "CreatedBy");
				_dp.Add(className, "CreatedByFirstName", typeof(WebUser).Name, "FirstName", System.Data.SqlDbType.NVarChar, "CreatedBy");
				_dp.Add(className, "CreatedByLastName", typeof(WebUser).Name, "LastName", System.Data.SqlDbType.NVarChar, "CreatedBy");
				_dp.Add(className, "ActivityGroupName", typeof(ActivityGroup).Name, "Name", System.Data.SqlDbType.NVarChar, "ActivityGroup");
				
                return _dp;
            }
        }

        public static SessionAction LoadObject(System.Data.DataRow row)
        {
            SessionAction obj = new SessionAction();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(SessionAction).Name);
				dq.Joins.Add(Registrant.ObjectMap["ID"], SessionAction.ObjectMap["RegistrantId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Registrant");
				dq.Joins.Add(Activity.ObjectMap["ID"], SessionAction.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Activity");
				dq.Joins.Add(ActivityGroup.ObjectMap["ID"], SessionAction.ObjectMap["ActivityGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "ActivityGroup");
				dq.Joins.Add(WebUser.ObjectMap["ID"], SessionAction.ObjectMap["WebUserId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "WebUser");
				dq.Joins.Add(WebUser.ObjectMap["ID"], SessionAction.ObjectMap["CreatedBy"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "CreatedBy");
				
                return dq;
            }
        }

        public static SessionAction Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<SessionAction> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return SessionAction.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return SessionAction.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.SessionId.IsEmpty())
				msgs.Add("SessionId is required on SessionAction object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(SessionAction).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(SessionAction).Name);
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
			dps.Add(ObjectMap["SessionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SessionActionTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WebUserId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationPeriodId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public SessionAction Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return SessionAction.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<SessionAction> LoadBySessionId(int SessionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["SessionId"], SessionId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByWebUserId(int WebUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadBySessionActionTypeId(SessionActionType SessionActionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["SessionActionTypeId"], SessionActionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByRegistrationId(int RegistrationId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByRegistrationPeriodId(int RegistrationPeriodId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByActivityGroupIdBySessionActionTypeId(int ActivityGroupId, SessionActionType SessionActionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			dq.Criterias.Add(ObjectMap["SessionActionTypeId"], SessionActionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByActivityIdBySessionActionTypeId(int ActivityId, SessionActionType SessionActionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["SessionActionTypeId"], SessionActionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByRegistrantIdBySessionActionTypeId(int RegistrantId, SessionActionType SessionActionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			dq.Criterias.Add(ObjectMap["SessionActionTypeId"], SessionActionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByRegistrationIdBySessionActionTypeId(int RegistrationId, SessionActionType SessionActionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			dq.Criterias.Add(ObjectMap["SessionActionTypeId"], SessionActionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SessionAction> LoadByRegistrationPeriodIdBySessionActionTypeId(int RegistrationPeriodId, SessionActionType SessionActionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
			dq.Criterias.Add(ObjectMap["SessionActionTypeId"], SessionActionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static List<SessionAction> LoadByActivityId(int ActivityId, List<SessionActionType> actionTypes, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.DebugMode = false;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);

            if (actionTypes != null && actionTypes.Any())
            {
                dq.Criterias.Add(ObjectMap["SessionActionTypeId"], actionTypes.Select(x => (int)x).ToList(), BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            }

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<SessionAction> LoadByAccountId(int AccountId, List<SessionActionType> actionTypes, int? start, int? length, SystemMessages msgs = null,  BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);

            if (actionTypes != null && actionTypes.Any())
            {
                dq.Criterias.Add(ObjectMap["SessionActionTypeId"], actionTypes.Select(x => (int)x).ToList(), BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            }

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<SessionAction> LoadByAccountIdByActivityId(int ActivityId, int AccountId, List<SessionActionType> actionTypes, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.DebugMode = false;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);

            if (actionTypes != null && actionTypes.Any())
                dq.Criterias.Add(ObjectMap["SessionActionTypeId"], actionTypes.Select(x => (int)x).ToList(), BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<SessionAction> LoadByRegistrationPeriodId(int RegistrationPeriodId, List<SessionActionType> actionTypes, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseDistinct = true;
            dq.UseParameterizedQuery = false; 
            dq.SelectParameters.Clear();

            dq.SelectParameters.Add(ObjectMap["ID"]);
            dq.SelectParameters.Add(ObjectMap["RegistrationId"]);
            dq.SelectParameters.Add(ObjectMap["AccountId"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantId"]);
            dq.SelectParameters.Add(ObjectMap["RegistrationPeriodId"]);
            dq.SelectParameters.Add(ObjectMap["CreateDate"]);
            dq.SelectParameters.Add(ObjectMap["CreatedBy"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantFirstName"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantLastName"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantEmail"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantPhone"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantCompany"]); 
            dq.SelectParameters.Add(ObjectMap["RegistrantUid"]); 
            dq.SelectParameters.Add(ObjectMap["SessionActionTypeId"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantRegistrantTypeId"]);

            dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);

            if (actionTypes != null && actionTypes.Any())
            {
                dq.Criterias.Add(ObjectMap["SessionActionTypeId"], actionTypes.Select(x => (int)x).ToList(), BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            }

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<SessionAction> LoadByActivityGroupId(int ActivityGroupId, List<SessionActionType> actionTypes, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseDistinct = true;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            dq.SelectParameters.Add(ObjectMap["ID"]);
            dq.SelectParameters.Add(ObjectMap["RegistrationId"]);
            dq.SelectParameters.Add(ObjectMap["AccountId"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantId"]);
            dq.SelectParameters.Add(ObjectMap["RegistrationPeriodId"]);
            dq.SelectParameters.Add(ObjectMap["CreateDate"]);
            dq.SelectParameters.Add(ObjectMap["CreatedBy"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantFirstName"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantLastName"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantEmail"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantPhone"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantCompany"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantUid"]);
            dq.SelectParameters.Add(ObjectMap["SessionActionTypeId"]);
            dq.SelectParameters.Add(ObjectMap["RegistrantRegistrantTypeId"]);
            dq.SelectParameters.Add(ObjectMap["ActivityGroupId"]);
            dq.SelectParameters.Add(ObjectMap["ActivityGroupName"]);
            dq.SelectParameters.Add(ObjectMap["CreatedByFirstName"]);
            dq.SelectParameters.Add(ObjectMap["CreatedByLastName"]);

            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);

            if (actionTypes != null && actionTypes.Any())
                dq.Criterias.Add(ObjectMap["SessionActionTypeId"], actionTypes.Select(x => (int)x).ToList(), BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public Registrant ToRegistrant()
        {
            return new Registrant()
            {
                ID = this.RegistrantId,
                FirstName = this.RegistrantFirstName,
                LastName = this.RegistrantLastName,
                Email = this.RegistrantEmail,
                Uid = this.RegistrantUid,
                Phone = this.RegistrantPhone,
                Company = this.RegistrantCompany,
                RegistrantTypeId = RegistrantType.Individual
            };
        }

        public string RegistrantName
        {
            get
            {
                string rtn = "";
                if (this.RegistrantRegistrantTypeId == RegistrantType.Individual)
                {
                    rtn = BaseWebUser.BuildDisplayName(this.RegistrantFirstName, this.RegistrantLastName);
                }
                else if (this.RegistrantRegistrantTypeId == RegistrantType.Organization)
                {
                    rtn = this.RegistrantCompany.ToProperCase();
                }

                return rtn;
            }
        }

        public string WebUserName
        {
            get
            {
                return BaseWebUser.BuildDisplayName(WebUserFirstName, WebUserLastName);
            }
        }

        public string CreatedByName
        {
            get
            {
                return BaseWebUser.BuildDisplayName(CreatedByFirstName, CreatedByLastName);
            }
        }

        public override string ToString()
        {
            // Temporarily commenting out code
            //switch (this.SessionActionTypeId)
            //{
            //    case SessionActionType.RegistrationCompletedView:
            //        return string.Format("{0} has registered for {1}", this.RegistrantName, this.ActivityName.IfEmpty("multiple activities"));
            //    case SessionActionType.ActivityCreate:
            //        return string.Format("{0} was created by {1}", this.ActivityName, this.CreatedByName);
            //    case SessionActionType.ActivityPublish:
            //        return string.Format("{0} was published by {1}", this.ActivityName, this.CreatedByName);
            //    case SessionActionType.ActivityUnpublish:
            //        return string.Format("{0} was unpublished by {1}", this.ActivityName, this.CreatedByName);
            //    case SessionActionType.ActivityRegistrationCompleted:
            //        return string.Format("{0} has registered for {1}", this.RegistrantName, this.ActivityName);
            //    case SessionActionType.ActivityRegistrationUpdated:
            //        return string.Format("The registration for {0} has been updated", this.RegistrantName);
            //    case SessionActionType.RegistrationUpdated:
            //        return string.Format("The registration for {0} has been updated", this.RegistrantName);
            //    case SessionActionType.RegistrationAddActivity:
            //        return string.Format("{0} was added to the registration for {1}", this.ActivityName, this.RegistrantName);
            //    case SessionActionType.RegistrationRemoveActivity:
            //        return string.Format("{0} was removed from the registration for {1}", this.ActivityName, this.RegistrantName);
            //    case SessionActionType.ActivityMoveToApplication:
            //        return string.Format("{0} was moved to an application seat for {1}", this.RegistrantName, this.ActivityName);
            //    case SessionActionType.ActivityMoveToReserved:
            //        return string.Format("{0} was moved to an reserved seat for {1}", this.RegistrantName, this.ActivityName);
            //    case SessionActionType.ActivityMoveToWaitingList:
            //        return string.Format("{0} was moved to an waiting list seat for {1}", this.RegistrantName, this.ActivityName);
            //    case SessionActionType.ActivityGroupPublish:
            //        return string.Format("{0} was published by {1}", this.ActivityGroupName, this.CreatedByName);
            //    case SessionActionType.ActivityGroupUnpublish:
            //        return string.Format("{0} was unpublished by {1}", this.ActivityGroupName, this.CreatedByName);
            //    case SessionActionType.ActivityGroupCreate:
            //        return string.Format("{0} was created by {1}", this.ActivityGroupName, this.CreatedByName);
            //    default:
            //        return "";
            //}

            switch (this.SessionActionTypeId)
            {
                case SessionActionType.RegistrationCompletedView:
                    return string.Format("{0} has registered for {1}", this.RegistrantName, this.ActivityName.IfEmpty("multiple activities"));
                case SessionActionType.ActivityCreate:
                    return string.Format("{0} was created", this.ActivityName, this.CreatedByName);
                case SessionActionType.ActivityPublish:
                    return string.Format("{0} was published", this.ActivityName, this.CreatedByName);
                case SessionActionType.ActivityUnpublish:
                    return string.Format("{0} was unpublished", this.ActivityName, this.CreatedByName);
                case SessionActionType.ActivityRegistrationCompleted:
                    return string.Format("{0} has registered for {1}", this.RegistrantName, this.ActivityName);
                case SessionActionType.ActivityRegistrationUpdated:
                    return string.Format("The registration for {0} has been updated", this.RegistrantName);
                case SessionActionType.RegistrationUpdated:
                    return string.Format("The registration for {0} has been updated", this.RegistrantName);
                case SessionActionType.RegistrationAddActivity:
                    return string.Format("{0} was added to the registration for {1}", this.ActivityName, this.RegistrantName);
                case SessionActionType.RegistrationRemoveActivity:
                    return string.Format("{0} was removed from the registration for {1}", this.ActivityName, this.RegistrantName);
                case SessionActionType.ActivityMoveToApplication:
                    return string.Format("{0} was moved to an application seat for {1}", this.RegistrantName, this.ActivityName);
                case SessionActionType.ActivityMoveToReserved:
                    return string.Format("{0} was moved to an reserved seat for {1}", this.RegistrantName, this.ActivityName);
                case SessionActionType.ActivityMoveToWaitingList:
                    return string.Format("{0} was moved to an waiting list seat for {1}", this.RegistrantName, this.ActivityName);
                case SessionActionType.ActivityGroupPublish:
                    return string.Format("{0} was published", this.ActivityGroupName, this.CreatedByName);
                case SessionActionType.ActivityGroupUnpublish:
                    return string.Format("{0} was unpublished", this.ActivityGroupName, this.CreatedByName);
                case SessionActionType.ActivityGroupCreate:
                    return string.Format("{0} was created", this.ActivityGroupName, this.CreatedByName);
                default:
                    return "";
            }
        }

        #endregion
    }
}
