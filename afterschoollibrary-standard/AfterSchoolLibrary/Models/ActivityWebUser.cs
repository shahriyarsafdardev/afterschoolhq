using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary
{
	/// <summary>
    /// 
    /// </summary>
	
    public class ActivityWebUser : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? WebUserId { get { return (int?)MDV["WebUserId"].Value; } set { MDV["WebUserId"].Value = value; } }
		public bool IsLeader { get { return (bool)MDV["IsLeader"].Value; } set { MDV["IsLeader"].Value = value; } }
		public string WebUserEmail { get { return (string)MDV["WebUserEmail"].Value; } }
		public string WebUserFirstName { get { return (string)MDV["WebUserFirstName"].Value; } }
		public string WebUserLastName { get { return (string)MDV["WebUserLastName"].Value; } }
		public string WebUserPhone { get { return (string)MDV["WebUserPhone"].Value; } }
		public Guid WebUserUid { get { return (Guid)MDV["WebUserUid"].Value; } }
		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
		public Guid ActivityUid { get { return (Guid)MDV["ActivityUid"].Value; } }
		

        public ActivityWebUser()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityWebUser).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsLeader = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityWebUser).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "WebUserId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsLeader", System.Data.SqlDbType.Bit);
				_dp.Add(className, "WebUserEmail", typeof(WebUser).Name, "Email", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserFirstName", typeof(WebUser).Name, "FirstName", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserLastName", typeof(WebUser).Name, "LastName", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserPhone", typeof(WebUser).Name, "Phone", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserUid", typeof(WebUser).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "WebUser");
				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityUid", typeof(Activity).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Activity");
				
                return _dp;
            }
        }

        protected static ActivityWebUser LoadObject(System.Data.DataRow row)
        {
            ActivityWebUser obj = new ActivityWebUser();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityWebUser).Name);
				dq.Joins.Add(Activity.ObjectMap["ID"], ActivityWebUser.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Activity");
				dq.Joins.Add(WebUser.ObjectMap["ID"], ActivityWebUser.ObjectMap["WebUserId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "WebUser");
				
                return dq;
            }
        }

        public static ActivityWebUser Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityWebUser> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityWebUser.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityWebUser.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ActivityId.IsEmpty())
				msgs.Add("ActivityId is required on ActivityWebUser object", MessageType.Error);
			if (this.WebUserId.IsEmpty())
				msgs.Add("WebUserId is required on ActivityWebUser object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityWebUser).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityWebUser).Name);
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
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WebUserId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsLeader"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityWebUser Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityWebUser.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityWebUser> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityWebUser> LoadByWebUserId(int WebUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityWebUser> LoadByWebUserIdByIsLeader(int WebUserId, bool IsLeader, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["IsLeader"], IsLeader);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityWebUser> LoadByActivityIdByIsLeader(int ActivityId, bool IsLeader, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["IsLeader"], IsLeader);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityWebUser LoadByActivityIdByWebUserId(int ActivityId, int WebUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public ActivityWebUser Copy(int newActivityId, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var awu = new ActivityWebUser()
            {
                ActivityId = newActivityId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId,
                IsLeader = this.IsLeader,
                WebUserId = this.WebUserId
            };

            msgs.AddRange(awu.Save(dc: dc));

            return awu;
        }

        public string WebUserDisplayName
        {
            get
            {
                return BaseWebUser.BuildDisplayName(this.WebUserFirstName, this.WebUserLastName);
            }
        }

        #endregion
    }
}
