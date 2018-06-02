//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BasicLibrary;
//using BasicLibrary.DataAccess;
//using System.Data;
//using System.Collections;

//namespace AfterSchoolLibrary
//{
	
//    public class ActivityLeader : BasicObjectWithModified
//    {
//		#region "ClassGenerator"

//		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
//		public int? LeaderId { get { return (int?)MDV["LeaderId"].Value; } set { MDV["LeaderId"].Value = value; } }
//		public string LeaderEmail { get { return (string)MDV["LeaderEmail"].Value; } }
//		public string LeaderFirstName { get { return (string)MDV["LeaderFirstName"].Value; } }
//		public string LeaderLastName { get { return (string)MDV["LeaderLastName"].Value; } }
//		public string LeaderPhone { get { return (string)MDV["LeaderPhone"].Value; } }
//		public Guid LeaderUid { get { return (Guid)MDV["LeaderUid"].Value; } }
//		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
//		public Guid ActivityUid { get { return (Guid)MDV["ActivityUid"].Value; } }
		

//        public ActivityLeader()
//        {
//            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityLeader).Name, "ID");
//            this.LoadObjectValues(ObjectMap);
			
//        }

//        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
//        {
//            get
//            {
//                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
//                string className = typeof(ActivityLeader).Name;
//                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
//				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
//				_dp.Add(className, "LeaderId", System.Data.SqlDbType.Int);
//				_dp.Add(className, "LeaderEmail", typeof(Leader).Name, "Email", System.Data.SqlDbType.NVarChar, "Leader");
//				_dp.Add(className, "LeaderFirstName", typeof(Leader).Name, "FirstName", System.Data.SqlDbType.NVarChar, "Leader");
//				_dp.Add(className, "LeaderLastName", typeof(Leader).Name, "LastName", System.Data.SqlDbType.NVarChar, "Leader");
//				_dp.Add(className, "LeaderPhone", typeof(Leader).Name, "Phone", System.Data.SqlDbType.NVarChar, "Leader");
//				_dp.Add(className, "LeaderUid", typeof(Leader).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Leader");
//				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
//				_dp.Add(className, "ActivityUid", typeof(Activity).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Activity");
				
//                return _dp;
//            }
//        }

//        protected static ActivityLeader LoadObject(System.Data.DataRow row)
//        {
//            ActivityLeader obj = new ActivityLeader();
//            obj.LoadObjectValues(ObjectMap, row);
//            return obj;
//        }

//        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
//        {
//            get
//            {
//                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityLeader).Name);
//				dq.Joins.Add(Activity.ObjectMap["ID"], ActivityLeader.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Activity");
//				dq.Joins.Add(Leader.ObjectMap["ID"], ActivityLeader.ObjectMap["LeaderId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Leader");
				
//                return dq;
//            }
//        }

//        public static ActivityLeader Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//            dq.Connection = dc;
//            dq.Criterias.Add(ObjectMap["ID"], id);
//            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
//        }

//        public static List<ActivityLeader> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//            dq.Connection = dc;
//			dq.UseParameterizedQuery = false;
//			dq.SelectRange.Start = start;
//			dq.SelectRange.Length = length;
//            return ActivityLeader.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
//        }

//        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//            dq.Connection = dc;
//            return ActivityLeader.BaseGetCount(criterias, ObjectMap, dq);
//        }

//		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            return base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityLeader).Name);
//        }

//        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityLeader).Name);
//        }

//        public static SystemMessages Create(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
//        }

//        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            SystemMessages msgs = new SystemMessages();
//            BasicLibrary.DataAccess.DatabaseParameters dps;

//			dps = new BasicLibrary.DataAccess.DatabaseParameters();
//			dps.Add(ObjectMap["ActivityId"]);
//			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

//			dps = new BasicLibrary.DataAccess.DatabaseParameters();
//			dps.Add(ObjectMap["LeaderId"]);
//			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

//            return msgs;
//        }

//		public ActivityLeader Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//		{
//			return ActivityLeader.Load(this.ID.Value, dc);
//		}
		
//		public static List<ActivityLeader> LoadByActivityId(int ActivityId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//		{
//			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//			dq.Connection = dc;
//			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
//			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
//		}

//		public static List<ActivityLeader> LoadByLeaderId(int LeaderId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//		{
//			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//			dq.Connection = dc;
//			dq.Criterias.Add(ObjectMap["LeaderId"], LeaderId);
//			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
//		}

//		public static ActivityLeader LoadByActivityIdByLeaderId(int ActivityId, int LeaderId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//		{
//			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//			dq.Connection = dc;
//			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
//			dq.Criterias.Add(ObjectMap["LeaderId"], LeaderId);
//			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
//		}

		

//        #endregion

//		#region "UserDefined"

//        public Leader ToLeader()
//        {
//            return new Leader()
//            {
//                ID = this.LeaderId,
//                FirstName = this.LeaderFirstName,
//                LastName = this.LeaderLastName,
//                Email = this.LeaderEmail,
//                Uid = this.LeaderUid,
//                Phone = this.LeaderPhone
//            };
//        }

//        public Activity ToActivity()
//        {
//            return new Activity()
//            {
//                ID = this.ActivityId,
//                Name = this.ActivityName,
//                Uid = this.ActivityUid
//            };
//        }

//        #endregion
//    }
//}
