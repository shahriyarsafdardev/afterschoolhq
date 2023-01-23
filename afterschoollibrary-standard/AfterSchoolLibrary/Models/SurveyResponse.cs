using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;
using AfterSchoolLibrary.Enums;

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
		public Guid? LastAttendedFormQuestionAnswerUid { get { return (Guid?)MDV["LastAttendedFormQuestionAnswerUid"].Value; } set { MDV["LastAttendedFormQuestionAnswerUid"].Value = value; } }
        public string RegistrantFirstName => (string) MDV["RegistrantFirstName"].Value;
        public string RegistrantLastName => (string) MDV["RegistrantLastName"].Value;
        public string RegistrantEmail => (string) MDV["RegistrantEmail"].Value;


        public SurveyResponse()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(SurveyResponse).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.NotSet;
			this.IsDeleted = false;
			this.Uid = Guid.NewGuid();
			
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
                _dp.Add(className, "RegistrantFirstName", typeof(Registrant).Name, "FirstName", SqlDbType.NVarChar, "Registrant");
                _dp.Add(className, "RegistrantLastName", typeof(Registrant).Name, "LastName", SqlDbType.NVarChar, "Registrant");
                _dp.Add(className, "RegistrantEmail", typeof(Registrant).Name, "Email", SqlDbType.NVarChar, "Registrant");

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


        public static int GetCountByAnswered(int accountId, int formId, List<int> activityIds, string search, DateTime? startDate, DateTime? endDate,
            List<int> searchQuestionIds, List<QuestionDataType> searchQuestionTypeIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            DatabaseCriteria searchCriteria = null;

            if (search.IsNotBlank())
            {
                searchCriteria = new DatabaseCriteria(FormQuestionAnswer.ObjectMap["Text"], search.Trim(), DatabaseCriteriaOperatorEnum.Like);
                dq.Criterias.Add(searchCriteria);
            }

            var sql = LoadByAnsweredQuery(accountId, formId, activityIds, searchCriteria, startDate, endDate, true, searchQuestionIds, searchQuestionTypeIds);

            return dq.ExecuteSelect(sql, msgs).Rows[0][0].ToString().ToInteger();
        }

        public static List<SurveyResponse> LoadByAnswered(int accountId, int formId, List<int> activityIds, string search, DateTime? startDate, DateTime? endDate, int? start, int? length,
            List<int> searchQuestionIds, List<QuestionDataType> searchQuestionTypeIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            DatabaseCriteria searchCriteria = null;

            if (search.IsNotBlank())
            {
                searchCriteria = new DatabaseCriteria(FormQuestionAnswer.ObjectMap["Text"], search.Trim(), DatabaseCriteriaOperatorEnum.Like);
                dq.Criterias.Add(searchCriteria);
            }

            var sql = LoadByAnsweredQuery(accountId, formId, activityIds, searchCriteria, startDate, endDate, false, searchQuestionIds, searchQuestionTypeIds);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        private static string LoadByAnsweredQuery(int accountId, int formId, List<int> activityIds, DatabaseCriteria search, DateTime? startDate, DateTime? endDate, 
            bool getCountOnly, List<int> searchQuestionIds, List<QuestionDataType> searchQuestionTypeIds)
        {
            string sql = @$"
                select {(getCountOnly ? "count(distinct SurveyResponse.ID)" : "distinct SurveyResponse.*, Registrant.FirstName as RegistrantFirstName, Registrant.LastName as RegistrantLastName, Registrant.Email as RegistrantEmail")} 
                from 
                    SurveyRequest inner join
	                SurveyResponse on SurveyRequest.ID = SurveyResponse.SurveyRequestId inner join
                    FormQuestionAnswer on SurveyResponse.ID = FormQuestionAnswer.SurveyResponseId inner join
                    Registrant on SurveyResponse.RegistrantId = Registrant.ID";

            if (activityIds.IsNotEmpty() && activityIds.Any())
                sql += @"
                    left outer join 
                    RegistrantActivity on Registrant.ID = RegistrantActivity.RegistrantId";

            sql += @$"
                where 
                    SurveyRequest.AccountId = {accountId} and 
                    SurveyRequest.FormId = {formId} and
                    SurveyResponse.SurveyFeedbackStatusTypeId = 3";

            if (search.IsNotEmpty())
            {
                sql += $" and FormQuestionAnswer.Text like @{search.UniqueName}";

                if (searchQuestionIds.IsNotEmpty() && searchQuestionIds.Any())
                {
                    sql += @$" and (
                                FormQuestionAnswer.FormQuestionId in ({searchQuestionIds.ToStrings().ToStringList()})";

                    var excludingNotSet = searchQuestionTypeIds.Where(x => x != QuestionDataType.NotSet).ToList();
                    if (excludingNotSet.IsNotEmpty() && excludingNotSet.Any())
                        sql += @$" or    
                                FormQuestionAnswer.QuestionDataTypeId in ({excludingNotSet.Select(x => (int) x).ToStrings().ToStringList()})";

                    sql += ")";
                }
            }

            if (startDate.HasValue)
                sql += $" and SurveyResponse.LastModifiedDate >= '{startDate.ToShortDateTimeString()}'";

            if (endDate.HasValue)
                sql += $" and SurveyResponse.LastModifiedDate <= '{endDate.ToShortDateTimeString()}'";
            
            if (activityIds.IsNotEmpty() && activityIds.Any())
                sql += @$"
                    and RegistrantActivity.ParticipationObjectId in ({activityIds.ToStrings().ToStringList()})
                    and RegistrantActivity.ParticipationObjectTypeId = {(int) ParticipationObjectType.Activity}";

            if (!getCountOnly)
                sql += @"
                    order by SurveyResponse.LastModifiedDate";

            return sql;
        }
        #endregion
    }
}
