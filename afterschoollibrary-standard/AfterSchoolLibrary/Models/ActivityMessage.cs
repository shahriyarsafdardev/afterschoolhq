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
    /// Used to store a message that is sent in regards to an session/league, activity/team, or even on the whole account
    /// </summary>
	
    public class ActivityMessage : BasicObject
    {
		#region "ClassGenerator"

		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public DateTime? DateSent { get { return (DateTime?)MDV["DateSent"].Value; } set { MDV["DateSent"].Value = value; } }
		public string Subject { get { return (string)MDV["Subject"].Value; } set { MDV["Subject"].Value = value; } }
		public string Body { get { return (string)MDV["Body"].Value; } set { MDV["Body"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public bool SendToStaff { get { return (bool)MDV["SendToStaff"].Value; } set { MDV["SendToStaff"].Value = value; } }
		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
		public string ActivityGroupName { get { return (string)MDV["ActivityGroupName"].Value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		

        public ActivityMessage()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityMessage).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.SendToStaff = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityMessage).Name;
                _dp = BasicObject.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DateSent", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "Subject", System.Data.SqlDbType.NVarChar , "150");
				_dp.Add(className, "Body", System.Data.SqlDbType.Text);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "SendToStaff", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityGroupName", typeof(ActivityGroup).Name, "Name", System.Data.SqlDbType.NVarChar, "ActivityGroup");
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				
                return _dp;
            }
        }

        public static ActivityMessage LoadObject(System.Data.DataRow row)
        {
            ActivityMessage obj = new ActivityMessage();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityMessage).Name);
				dq.Joins.Add(Activity.ObjectMap["ID"], ActivityMessage.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Activity");
				dq.Joins.Add(ActivityGroup.ObjectMap["ID"], ActivityMessage.ObjectMap["ActivityGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "ActivityGroup");
				dq.Joins.Add(Account.ObjectMap["ID"], ActivityMessage.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static ActivityMessage Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityMessage> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityMessage.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityMessage.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on ActivityMessage object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityMessage).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityMessage).Name);
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
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DateSent"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Subject"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Body"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SendToStaff"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityMessage Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityMessage.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityMessage> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityMessage> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityMessage LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityMessage LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<ActivityMessage> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        private List<ActivityMessageRegistrant> _ActivityMessageRegistrants = null;
        public List<ActivityMessageRegistrant> GetActivityMessageRegistrants(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_ActivityMessageRegistrants.IsEmpty())
            {
                _ActivityMessageRegistrants = ActivityMessageRegistrant.LoadByActivityMessageId(this.ID.Value, msgs, dc);
            }
            return _ActivityMessageRegistrants;
        }

        public int GetRegistrantCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessageRegistrant.GetCount(
                new Hashtable()
                {
                    { "ActivityMessageId", this.ID.Value }
                },
                msgs
                );
        }

        public int GetUnsentCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessageRegistrant.GetCount(
                   new Hashtable()
                   {
                    { "ActivityMessageId", this.ID.Value },
                    { "DateSent", DBNull.Value }
                   },
                   msgs
               );
        }

        public int GetSentCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.GetRegistrantCount(msgs, dc) - this.GetUnsentCount(msgs, dc);
        }

        public static List<ActivityMessage> LoadByActivityId(int ActivityId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<ActivityMessage> LoadByAccountId(int AccountId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.GetCount(msgs);
        }

        #endregion
    }
}
