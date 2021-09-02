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
    /// Saves Survey responses.
    /// </summary>
	[Serializable]
    public class SurveyResponse : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? SurveyRequestId { get { return (int?)MDV["SurveyRequestId"].Value; } set { MDV["SurveyRequestId"].Value = value; } }
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		public SurveyFeedbackStatus SurveyFeedbackStatusTypeId { get { return (SurveyFeedbackStatus)MDV["SurveyFeedbackStatusTypeId"].Value; } set { MDV["SurveyFeedbackStatusTypeId"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public Guid LastAttendedFormQuestionAnswerUid { get { return (Guid)MDV["LastAttendedFormQuestionAnswerUid"].Value; } set { MDV["LastAttendedFormQuestionAnswerUid"].Value = value; } }
		

        public SurveyResponse()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(SurveyResponse).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.NotSet;
			this.IsDeleted = false;
			this.Uid = Guid.NewGuid();
			this.LastAttendedFormQuestionAnswerUid = Guid.NewGuid();
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(SurveyResponse).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "SurveyRequestId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "SurveyFeedbackStatusTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "LastAttendedFormQuestionAnswerUid", System.Data.SqlDbType.UniqueIdentifier);
				
                return _dp;
            }
        }

        public static SurveyResponse LoadObject(System.Data.DataRow row)
        {
            SurveyResponse obj = new SurveyResponse();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(SurveyResponse).Name);
				dq.Joins.Add(Registrant.ObjectMap["ID"], SurveyResponse.ObjectMap["RegistrantId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Registrant");
				dq.Joins.Add(SurveyRequest.ObjectMap["ID"], SurveyResponse.ObjectMap["SurveyRequestId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "SurveyRequest");
				
                return dq;
            }
        }

        public static SurveyResponse Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<SurveyResponse> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return SurveyResponse.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return SurveyResponse.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on SurveyResponse object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(SurveyResponse).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(SurveyResponse).Name);
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
			dps.Add(ObjectMap["SurveyRequestId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SurveyFeedbackStatusTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LastAttendedFormQuestionAnswerUid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public SurveyResponse Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return SurveyResponse.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<SurveyResponse> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SurveyResponse> LoadBySurveyRequestId(int SurveyRequestId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["SurveyRequestId"], SurveyRequestId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static SurveyResponse LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static SurveyResponse LoadByUidBySurveyRequestId(Guid Uid, int SurveyRequestId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["SurveyRequestId"], SurveyRequestId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static SurveyResponse LoadByUidByRegistrantId(Guid Uid, int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}
