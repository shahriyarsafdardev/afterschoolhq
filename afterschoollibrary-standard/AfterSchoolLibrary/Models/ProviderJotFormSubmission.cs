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
    /// Jotform submissions for providers
    /// </summary>
	[Serializable]
    public class ProviderJotFormSubmission : BasicIDObject
    {
        #region "ClassGenerator"

        public DateTime? SubmissionDate { get => (DateTime?) MDV["SubmissionDate"].Value; set => MDV["SubmissionDate"].Value = value; }
        public string TeamMembers { get => (string) MDV["TeamMembers"].Value; set => MDV["TeamMembers"].Value = value; }
        public string ForProfit { get => (string) MDV["ForProfit"].Value; set => MDV["ForProfit"].Value = value; }
        public string CurrentlyUsing { get => (string) MDV["CurrentlyUsing"].Value; set => MDV["CurrentlyUsing"].Value = value; }
        public string AnalyticsDashboard { get => (string) MDV["AnalyticsDashboard"].Value; set => MDV["AnalyticsDashboard"].Value = value; }
        public string ProgramLocation { get => (string) MDV["ProgramLocation"].Value; set => MDV["ProgramLocation"].Value = value; }
        public string ImplementationTimeline { get => (string) MDV["ImplementationTimeline"].Value; set => MDV["ImplementationTimeline"].Value = value; }
        public string WithinBudget { get => (string) MDV["WithinBudget"].Value; set => MDV["WithinBudget"].Value = value; }
        public string PainPoints { get => (string) MDV["PainPoints"].Value; set => MDV["PainPoints"].Value = value; }
        public string PhoneNumber { get => (string) MDV["PhoneNumber"].Value; set => MDV["PhoneNumber"].Value = value; }
        public string WantProfile { get => (string) MDV["WantProfile"].Value; set => MDV["WantProfile"].Value = value; }
        public string WantDemo { get => (string) MDV["WantDemo"].Value; set => MDV["WantDemo"].Value = value; }
        public string BestEmail { get => (string) MDV["BestEmail"].Value; set => MDV["BestEmail"].Value = value; }
        public string OtherDecisionMakers { get => (string) MDV["OtherDecisionMakers"].Value; set => MDV["OtherDecisionMakers"].Value = value; }
        public string NextRegistrationPeriod { get => (string) MDV["NextRegistrationPeriod"].Value; set => MDV["NextRegistrationPeriod"].Value = value; }
        public string Have15Minutes { get => (string) MDV["Have15Minutes"].Value; set => MDV["Have15Minutes"].Value = value; }
        public string WhatsNext { get => (string) MDV["WhatsNext"].Value; set => MDV["WhatsNext"].Value = value; }
        public string BestWayToReach { get => (string) MDV["BestWayToReach"].Value; set => MDV["BestWayToReach"].Value = value; }
        public string HowToLearnMore { get => (string) MDV["HowToLearnMore"].Value; set => MDV["HowToLearnMore"].Value = value; }
        public string SubmissionId { get => (string) MDV["SubmissionId"].Value; set => MDV["SubmissionId"].Value = value; }


        public ProviderJotFormSubmission()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ProviderJotFormSubmission).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ProviderJotFormSubmission).Name;
                _dp = BasicIDObject.get_BaseObjectMap(className);
                _dp.Add(className, "SubmissionDate", System.Data.SqlDbType.DateTime);
                _dp.Add(className, "TeamMembers", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "ForProfit", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "CurrentlyUsing", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "AnalyticsDashboard", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "ProgramLocation", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "ImplementationTimeline", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "WithinBudget", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "PainPoints", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "PhoneNumber", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "WantProfile", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "WantDemo", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "BestEmail", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "OtherDecisionMakers", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "NextRegistrationPeriod", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "Have15Minutes", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "WhatsNext", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "BestWayToReach", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "HowToLearnMore", System.Data.SqlDbType.NVarChar, "100");
                _dp.Add(className, "SubmissionId", System.Data.SqlDbType.NVarChar, "100");

                return _dp;
            }
        }

        public static ProviderJotFormSubmission LoadObject(System.Data.DataRow row)
        {
            ProviderJotFormSubmission obj = new ProviderJotFormSubmission();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ProviderJotFormSubmission).Name);
				
                return dq;
            }
        }

        public static ProviderJotFormSubmission Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static ProviderJotFormSubmission Load(string submissionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["submissionId"], submissionId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static bool Exists(string submissionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["submissionId"], submissionId);
            return dq.GetCount(msgs) > 0;
        }

        public static List<ProviderJotFormSubmission> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ProviderJotFormSubmission.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ProviderJotFormSubmission.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ProviderJotFormSubmission).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ProviderJotFormSubmission).Name);
        }

        public static SystemMessages Create(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            BasicLibrary.DataAccess.DatabaseParameters dps;

			//dps = new BasicLibrary.DataAccess.DatabaseParameters();
			//dps.Add(ObjectMap["SubmissionDate"]);
			//msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ProviderJotFormSubmission Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ProviderJotFormSubmission.Load(this.ID.Value, msgs, dc: dc);
		}

		

        #endregion

		#region "UserDefined"


        #endregion
    }
}
