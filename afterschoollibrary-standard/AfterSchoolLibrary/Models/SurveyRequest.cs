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
    /// table to save details of all suvrye request created by providers.
    /// </summary>
	[Serializable]
    public class SurveyRequest : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public string Name { get { return (string)MDV["Name"].Value; } set { MDV["Name"].Value = value; } }
		public string Description { get { return (string)MDV["Description"].Value; } set { MDV["Description"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int AccountId { get { return (int)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int ActivityId { get { return (int)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int FormId { get { return (int)MDV["FormId"].Value; } set { MDV["FormId"].Value = value; } }
		public DateTime? DueDate { get { return (DateTime?)MDV["DueDate"].Value; } set { MDV["DueDate"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public bool MultipleResponsesEnabled { get { return (bool)MDV["MultipleResponsesEnabled"].Value; } set { MDV["MultipleResponsesEnabled"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		

        public SurveyRequest()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(SurveyRequest).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.MultipleResponsesEnabled = false;
			this.IsDeleted = false;
			this.Uid = Guid.NewGuid();
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(SurveyRequest).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "Name", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "Description", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "FormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DueDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "MultipleResponsesEnabled", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				
                return _dp;
            }
        }

        public static SurveyRequest LoadObject(System.Data.DataRow row)
        {
            SurveyRequest obj = new SurveyRequest();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(SurveyRequest).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], SurveyRequest.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Account");
				dq.Joins.Add(Form.ObjectMap["ID"], SurveyRequest.ObjectMap["FormId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Form");
				dq.Joins.Add(Activity.ObjectMap["ID"], SurveyRequest.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Activity");
				
                return dq;
            }
        }

        public static SurveyRequest Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<SurveyRequest> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return SurveyRequest.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return SurveyRequest.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on SurveyRequest object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(SurveyRequest).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(SurveyRequest).Name);
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
			dps.Add(ObjectMap["Name"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Description"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FormId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DueDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["MultipleResponsesEnabled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public SurveyRequest Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return SurveyRequest.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<SurveyRequest> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SurveyRequest> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SurveyRequest> LoadByFormId(int FormId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static SurveyRequest LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static SurveyRequest LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static SurveyRequest LoadByUidByActivityId(Guid Uid, int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static SurveyRequest LoadByUidByFormId(Guid Uid, int FormId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}
