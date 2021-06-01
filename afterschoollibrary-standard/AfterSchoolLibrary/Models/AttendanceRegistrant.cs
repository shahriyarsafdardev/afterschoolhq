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
    /// This keeps track of the attendance marking for one particular student
    /// </summary>
	
    public class AttendanceRegistrant : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AttendanceId { get { return (int?)MDV["AttendanceId"].Value; } set { MDV["AttendanceId"].Value = value; } }
		public int? TransactionId { get { return (int?)MDV["TransactionId"].Value; } set { MDV["TransactionId"].Value = value; } }
		public bool IsPresent { get { return (bool)MDV["IsPresent"].Value; } set { MDV["IsPresent"].Value = value; } }
		public bool IsLate { get { return (bool)MDV["IsLate"].Value; } set { MDV["IsLate"].Value = value; } }
		public int? NumberPresent { get { return (int?)MDV["NumberPresent"].Value; } set { MDV["NumberPresent"].Value = value; } }
		public int? NumberLate { get { return (int?)MDV["NumberLate"].Value; } set { MDV["NumberLate"].Value = value; } }
		public int? NumberPossible { get { return (int?)MDV["NumberPossible"].Value; } set { MDV["NumberPossible"].Value = value; } }
		public string RegistrantEmail { get { return (string)MDV["RegistrantEmail"].Value; } }
		public string RegistrantFirstName { get { return (string)MDV["RegistrantFirstName"].Value; } }
		public string RegistrantLastName { get { return (string)MDV["RegistrantLastName"].Value; } }
		public string RegistrantPhone { get { return (string)MDV["RegistrantPhone"].Value; } }
		public string RegistrantCompany { get { return (string)MDV["RegistrantCompany"].Value; } }
		public RegistrantType RegistrantRegistrantTypeId { get { return (RegistrantType)MDV["RegistrantRegistrantTypeId"].Value; } }
		public int? RegistrantAccountId { get { return (int?)MDV["RegistrantAccountId"].Value; } }
		public int? AttendanceActivityId { get { return (int?)MDV["AttendanceActivityId"].Value; } }
		public bool AttendanceIsCompleted { get { return (bool)MDV["AttendanceIsCompleted"].Value; } }
		public DateTime? AttendanceDate { get { return (DateTime?)MDV["AttendanceDate"].Value; } }
		

        public AttendanceRegistrant()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AttendanceRegistrant).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsPresent = false;
			this.IsLate = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AttendanceRegistrant).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AttendanceId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TransactionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsPresent", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsLate", System.Data.SqlDbType.Bit);
				_dp.Add(className, "NumberPresent", System.Data.SqlDbType.Int);
				_dp.Add(className, "NumberLate", System.Data.SqlDbType.Int);
				_dp.Add(className, "NumberPossible", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantEmail", typeof(Registrant).Name, "Email", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantFirstName", typeof(Registrant).Name, "FirstName", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantLastName", typeof(Registrant).Name, "LastName", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantPhone", typeof(Registrant).Name, "Phone", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantCompany", typeof(Registrant).Name, "Company", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantRegistrantTypeId", typeof(Registrant).Name, "RegistrantTypeId", System.Data.SqlDbType.Int, "Registrant");
				_dp.Add(className, "RegistrantAccountId", typeof(Registrant).Name, "AccountId", System.Data.SqlDbType.Int, "Registrant");
				_dp.Add(className, "AttendanceActivityId", typeof(Attendance).Name, "ActivityId", System.Data.SqlDbType.Int, "Attendance");
				_dp.Add(className, "AttendanceIsCompleted", typeof(Attendance).Name, "IsCompleted", System.Data.SqlDbType.Bit, "Attendance");
				_dp.Add(className, "AttendanceDate", typeof(Attendance).Name, "Date", System.Data.SqlDbType.DateTime, "Attendance");
				
                return _dp;
            }
        }

        public static AttendanceRegistrant LoadObject(System.Data.DataRow row)
        {
            AttendanceRegistrant obj = new AttendanceRegistrant();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AttendanceRegistrant).Name);
				dq.Joins.Add(Registrant.ObjectMap["ID"], AttendanceRegistrant.ObjectMap["RegistrantId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Registrant");
				dq.Joins.Add(Attendance.ObjectMap["ID"], AttendanceRegistrant.ObjectMap["AttendanceId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Attendance");
				
                return dq;
            }
        }

        public static AttendanceRegistrant Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AttendanceRegistrant> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AttendanceRegistrant.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AttendanceRegistrant.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.RegistrantId.IsEmpty())
				msgs.Add("RegistrantId is required on AttendanceRegistrant object", MessageType.Error);
			if (this.AttendanceId.IsEmpty())
				msgs.Add("AttendanceId is required on AttendanceRegistrant object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AttendanceRegistrant).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AttendanceRegistrant).Name);
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
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AttendanceId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TransactionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsPresent"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsLate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberPresent"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberLate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberPossible"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AttendanceRegistrant Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AttendanceRegistrant.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<AttendanceRegistrant> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AttendanceRegistrant> LoadByAttendanceId(int AttendanceId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AttendanceId"], AttendanceId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AttendanceRegistrant> LoadByAttendanceIdByRegistrantId(int AttendanceId, int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AttendanceId"], AttendanceId);
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

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

        public bool IsAlreadyBilled
        {
            get
            {
                return this.TransactionId.HasValue;
            }
        }

        public static int GetPresentCount(int attendanceId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            BasicLibrary.DataAccess.SelectParameter sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberPresent"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["attendanceId"], attendanceId);
            dq.Criterias.Add(ObjectMap["AttendanceIsCompleted"], true);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int GetPossibleCount(int attendanceId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            BasicLibrary.DataAccess.SelectParameter sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberPossible"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["attendanceId"], attendanceId);
            dq.Criterias.Add(ObjectMap["AttendanceIsCompleted"], true);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int GetPresentCountByActivityId(int activityId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            BasicLibrary.DataAccess.SelectParameter sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberPresent"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["AttendanceActivityId"], activityId);
            dq.Criterias.Add(ObjectMap["AttendanceIsCompleted"], true);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int GetPossibleCountByActivityId(int activityId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            BasicLibrary.DataAccess.SelectParameter sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberPossible"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["AttendanceActivityId"], activityId);
            dq.Criterias.Add(ObjectMap["AttendanceIsCompleted"], true);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int GetPresentCountByAccountId(int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            BasicLibrary.DataAccess.SelectParameter sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberPresent"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["RegistrantAccountId"], accountId);
            dq.Criterias.Add(ObjectMap["AttendanceIsCompleted"], true);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int GetPossibleCountByAccountId(int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            BasicLibrary.DataAccess.SelectParameter sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberPossible"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["RegistrantAccountId"], accountId);
            dq.Criterias.Add(ObjectMap["AttendanceIsCompleted"], true);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static List<AttendanceRegistrant> LoadByAttendanceIdByRegistrantIdByDateRange(int AttendanceId, int RegistrantId, DateRange dateRange, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AttendanceId"], AttendanceId);
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            dq.Criterias.Add(ObjectMap["AttendanceDate"], dateRange.StartDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            dq.Criterias.Add(ObjectMap["AttendanceDate"], dateRange.EndDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AttendanceRegistrant> LoadByActivityIdByDateRange(int ActivityId, DateRange dateRange, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AttendanceActivityId"], ActivityId);
            dq.Criterias.Add(ObjectMap["AttendanceDate"], dateRange.StartDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            dq.Criterias.Add(ObjectMap["AttendanceDate"], dateRange.EndDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AttendanceRegistrant> LoadByActivityIdRegistrantIdByDateRange(int ActivityId, int RegistrantId, DateRange dateRange, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AttendanceActivityId"], ActivityId);
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            dq.Criterias.Add(ObjectMap["AttendanceDate"], dateRange.StartDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            dq.Criterias.Add(ObjectMap["AttendanceDate"], dateRange.EndDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }



        #endregion
    }
}
