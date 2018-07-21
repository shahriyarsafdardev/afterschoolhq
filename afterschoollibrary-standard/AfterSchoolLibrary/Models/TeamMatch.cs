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
    /// A competitive match played between two teams in the same league
    /// </summary>
	
    public class TeamMatch : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? HomeTeamId { get { return (int?)MDV["HomeTeamId"].Value; } set { MDV["HomeTeamId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AwayTeamId { get { return (int?)MDV["AwayTeamId"].Value; } set { MDV["AwayTeamId"].Value = value; } }
		public int? LocationId { get { return (int?)MDV["LocationId"].Value; } set { MDV["LocationId"].Value = value; } }
		public int? HomeTeamScore { get { return (int?)MDV["HomeTeamScore"].Value; } set { MDV["HomeTeamScore"].Value = value; } }
		public int? AwayTeamScore { get { return (int?)MDV["AwayTeamScore"].Value; } set { MDV["AwayTeamScore"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? ActivityInstanceId { get { return (int?)MDV["ActivityInstanceId"].Value; } set { MDV["ActivityInstanceId"].Value = value; } }
		

        public TeamMatch()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(TeamMatch).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(TeamMatch).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "HomeTeamId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AwayTeamId", System.Data.SqlDbType.Int);
				_dp.Add(className, "LocationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "HomeTeamScore", System.Data.SqlDbType.Int);
				_dp.Add(className, "AwayTeamScore", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityInstanceId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static TeamMatch LoadObject(System.Data.DataRow row)
        {
            TeamMatch obj = new TeamMatch();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(TeamMatch).Name);
				
                return dq;
            }
        }

        public static TeamMatch Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<TeamMatch> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return TeamMatch.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return TeamMatch.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.HomeTeamId.IsEmpty())
				msgs.Add("HomeTeamId is required on TeamMatch object", MessageType.Error);
			if (this.AwayTeamId.IsEmpty())
				msgs.Add("AwayTeamId is required on TeamMatch object", MessageType.Error);
			if (this.ActivityInstanceId.IsEmpty())
				msgs.Add("ActivityInstanceId is required on TeamMatch object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(TeamMatch).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(TeamMatch).Name);
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
			dps.Add(ObjectMap["HomeTeamId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AwayTeamId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LocationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["HomeTeamScore"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AwayTeamScore"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityInstanceId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public TeamMatch Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return TeamMatch.Load(this.ID.Value, msgs, dc: dc);
		}
		
		

        #endregion

		#region "UserDefined"

        #endregion
    }
}
