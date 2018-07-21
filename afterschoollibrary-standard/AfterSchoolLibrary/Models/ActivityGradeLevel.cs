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
    /// Which grades are accepted for a particular activity
    /// </summary>
	
    public class ActivityGradeLevel : BasicObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? GradeLevelId { get { return (int?)MDV["GradeLevelId"].Value; } set { MDV["GradeLevelId"].Value = value; } }
		public string GradeLevelName { get { return (string)MDV["GradeLevelName"].Value; } }
		public string GradeLevelAbbreviation { get { return (string)MDV["GradeLevelAbbreviation"].Value; } }
		

        public ActivityGradeLevel()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityGradeLevel).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityGradeLevel).Name;
                _dp = BasicObject.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "GradeLevelId", System.Data.SqlDbType.Int);
				_dp.Add(className, "GradeLevelName", typeof(GradeLevel).Name, "Name", System.Data.SqlDbType.NVarChar, "GradeLevel");
				_dp.Add(className, "GradeLevelAbbreviation", typeof(GradeLevel).Name, "Abbreviation", System.Data.SqlDbType.NVarChar, "GradeLevel");
				
                return _dp;
            }
        }

        protected static ActivityGradeLevel LoadObject(System.Data.DataRow row)
        {
            ActivityGradeLevel obj = new ActivityGradeLevel();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityGradeLevel).Name);
				dq.Joins.Add(GradeLevel.ObjectMap["ID"], ActivityGradeLevel.ObjectMap["GradeLevelId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "GradeLevel");
				
                return dq;
            }
        }

        public static ActivityGradeLevel Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityGradeLevel> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityGradeLevel.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityGradeLevel.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ActivityId.IsEmpty())
				msgs.Add("ActivityId is required on ActivityGradeLevel object", MessageType.Error);
			if (this.GradeLevelId.IsEmpty())
				msgs.Add("GradeLevelId is required on ActivityGradeLevel object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityGradeLevel).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityGradeLevel).Name);
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
			dps.Add(ObjectMap["GradeLevelId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityGradeLevel Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityGradeLevel.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityGradeLevel> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityGradeLevel LoadByActivityIdByGradeLevelId(int ActivityId, int GradeLevelId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["GradeLevelId"], GradeLevelId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<ActivityGradeLevel> LoadByGradeLevelId(int GradeLevelId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["GradeLevelId"], GradeLevelId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public GradeLevel ToGradeLevel()
        {
            return new GradeLevel()
            {
                ID = this.GradeLevelId,
                Name = this.GradeLevelName,
                Abbreviation = this.GradeLevelAbbreviation
            };
        }

        #endregion
    }
}
