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
    /// Where legal details and other info is stored for the stripe account holder
    /// </summary>
	
    public class StripeAccount : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public string LegalEntityBusinessName { get { return (string)MDV["LegalEntityBusinessName"].Value; } set { MDV["LegalEntityBusinessName"].Value = value; } }
		public string LegalEntityEIN { get { return (string)MDV["LegalEntityEIN"].Value; } set { MDV["LegalEntityEIN"].Value = value; } }
		public string LegalEntitySSNLast4 { get { return (string)MDV["LegalEntitySSNLast4"].Value; } set { MDV["LegalEntitySSNLast4"].Value = value; } }
		public string LegalEntityFirstName { get { return (string)MDV["LegalEntityFirstName"].Value; } set { MDV["LegalEntityFirstName"].Value = value; } }
		public string LegalEntityLastName { get { return (string)MDV["LegalEntityLastName"].Value; } set { MDV["LegalEntityLastName"].Value = value; } }
		public DateTime? LegalEntityDOB { get { return (DateTime?)MDV["LegalEntityDOB"].Value; } set { MDV["LegalEntityDOB"].Value = value; } }
		public string LegalEntityAddress1 { get { return (string)MDV["LegalEntityAddress1"].Value; } set { MDV["LegalEntityAddress1"].Value = value; } }
		public string LegalEntityAddress2 { get { return (string)MDV["LegalEntityAddress2"].Value; } set { MDV["LegalEntityAddress2"].Value = value; } }
		public string LegalEntityCity { get { return (string)MDV["LegalEntityCity"].Value; } set { MDV["LegalEntityCity"].Value = value; } }
		public string LegalEntityState { get { return (string)MDV["LegalEntityState"].Value; } set { MDV["LegalEntityState"].Value = value; } }
		public string LegalEntityPostalCode { get { return (string)MDV["LegalEntityPostalCode"].Value; } set { MDV["LegalEntityPostalCode"].Value = value; } }
		public DateTime? TOSAcceptanceDate { get { return (DateTime?)MDV["TOSAcceptanceDate"].Value; } set { MDV["TOSAcceptanceDate"].Value = value; } }
		public string TOSAcceptanceIP { get { return (string)MDV["TOSAcceptanceIP"].Value; } set { MDV["TOSAcceptanceIP"].Value = value; } }
		public string TOSAcceptanceUserAgent { get { return (string)MDV["TOSAcceptanceUserAgent"].Value; } set { MDV["TOSAcceptanceUserAgent"].Value = value; } }
		public LegalEntityType LegalEntityTypeId { get { return (LegalEntityType)MDV["LegalEntityTypeId"].Value; } set { MDV["LegalEntityTypeId"].Value = value; } }
		public string BusinessPhone { get { return (string)MDV["BusinessPhone"].Value; } set { MDV["BusinessPhone"].Value = value; } }
		public string SupportPhone { get { return (string)MDV["SupportPhone"].Value; } set { MDV["SupportPhone"].Value = value; } }
		public string SupportEmail { get { return (string)MDV["SupportEmail"].Value; } set { MDV["SupportEmail"].Value = value; } }
		public string BusinessUrl { get { return (string)MDV["BusinessUrl"].Value; } set { MDV["BusinessUrl"].Value = value; } }
		public bool RequiresFullSSN { get { return (bool)MDV["RequiresFullSSN"].Value; } set { MDV["RequiresFullSSN"].Value = value; } }
		public StripeVerificationStatus VerificationStatusId { get { return (StripeVerificationStatus)MDV["VerificationStatusId"].Value; } set { MDV["VerificationStatusId"].Value = value; } }
		public bool ChargesEnabled { get { return (bool)MDV["ChargesEnabled"].Value; } set { MDV["ChargesEnabled"].Value = value; } }
		public string ChargesDisabledReason { get { return (string)MDV["ChargesDisabledReason"].Value; } set { MDV["ChargesDisabledReason"].Value = value; } }
		public bool PayoutsEnabled { get { return (bool)MDV["PayoutsEnabled"].Value; } set { MDV["PayoutsEnabled"].Value = value; } }
		public string BankAccountId { get { return (string)MDV["BankAccountId"].Value; } set { MDV["BankAccountId"].Value = value; } }
		public string BankName { get { return (string)MDV["BankName"].Value; } set { MDV["BankName"].Value = value; } }
		public string BankLast4 { get { return (string)MDV["BankLast4"].Value; } set { MDV["BankLast4"].Value = value; } }
		public DateTime? VerificationDueBy { get { return (DateTime?)MDV["VerificationDueBy"].Value; } set { MDV["VerificationDueBy"].Value = value; } }
		public bool RequiresDocument { get { return (bool)MDV["RequiresDocument"].Value; } set { MDV["RequiresDocument"].Value = value; } }
		public bool RequiresAdditionalOwners { get { return (bool)MDV["RequiresAdditionalOwners"].Value; } set { MDV["RequiresAdditionalOwners"].Value = value; } }
		public string DocumentFileId { get { return (string)MDV["DocumentFileId"].Value; } set { MDV["DocumentFileId"].Value = value; } }
		public StripeVerificationFailedCode FailedDetailsCode { get { return (StripeVerificationFailedCode)MDV["FailedDetailsCode"].Value; } set { MDV["FailedDetailsCode"].Value = value; } }
		

        public StripeAccount()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(StripeAccount).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.LegalEntityTypeId = LegalEntityType.NotSet;
			this.RequiresFullSSN = false;
			this.VerificationStatusId = StripeVerificationStatus.NotSet;
			this.ChargesEnabled = false;
			this.PayoutsEnabled = false;
			this.RequiresDocument = false;
			this.RequiresAdditionalOwners = false;
			this.FailedDetailsCode = StripeVerificationFailedCode.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(StripeAccount).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "LegalEntityBusinessName", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "LegalEntityEIN", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "LegalEntitySSNLast4", System.Data.SqlDbType.NVarChar , "4");
				_dp.Add(className, "LegalEntityFirstName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "LegalEntityLastName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "LegalEntityDOB", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "LegalEntityAddress1", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "LegalEntityAddress2", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "LegalEntityCity", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "LegalEntityState", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "LegalEntityPostalCode", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "TOSAcceptanceDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "TOSAcceptanceIP", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "TOSAcceptanceUserAgent", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "LegalEntityTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "BusinessPhone", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "SupportPhone", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "SupportEmail", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "BusinessUrl", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "RequiresFullSSN", System.Data.SqlDbType.Bit);
				_dp.Add(className, "VerificationStatusId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ChargesEnabled", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ChargesDisabledReason", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "PayoutsEnabled", System.Data.SqlDbType.Bit);
				_dp.Add(className, "BankAccountId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "BankName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "BankLast4", System.Data.SqlDbType.NVarChar , "4");
				_dp.Add(className, "VerificationDueBy", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "RequiresDocument", System.Data.SqlDbType.Bit);
				_dp.Add(className, "RequiresAdditionalOwners", System.Data.SqlDbType.Bit);
				_dp.Add(className, "DocumentFileId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "FailedDetailsCode", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static StripeAccount LoadObject(System.Data.DataRow row)
        {
            StripeAccount obj = new StripeAccount();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(StripeAccount).Name);
				
                return dq;
            }
        }

        public static StripeAccount Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<StripeAccount> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return StripeAccount.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return StripeAccount.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(StripeAccount).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(StripeAccount).Name);
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
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityBusinessName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityEIN"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntitySSNLast4"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityFirstName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityLastName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityDOB"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityAddress1"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityAddress2"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityCity"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityState"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityPostalCode"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TOSAcceptanceDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TOSAcceptanceIP"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TOSAcceptanceUserAgent"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LegalEntityTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BusinessPhone"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SupportPhone"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SupportEmail"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BusinessUrl"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RequiresFullSSN"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["VerificationStatusId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ChargesEnabled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ChargesDisabledReason"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PayoutsEnabled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BankAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BankName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BankLast4"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["VerificationDueBy"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RequiresDocument"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RequiresAdditionalOwners"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DocumentFileId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FailedDetailsCode"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public StripeAccount Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return StripeAccount.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static StripeAccount LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public bool IsVerified
        {
            get
            {
                return VerificationStatusId == StripeVerificationStatus.Verified;
            }
        }

        #endregion
    }
}
