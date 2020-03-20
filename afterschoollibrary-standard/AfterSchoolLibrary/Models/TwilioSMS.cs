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
    /// Store information about messages sent via text
    /// </summary>
	
    public class TwilioSMS : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// We can make sure we don't send the same message to the same person even if they have 2 kids in the same class
		/// </summary>
		public int? ActivityMessageId { get { return (int?)MDV["ActivityMessageId"].Value; } set { MDV["ActivityMessageId"].Value = value; } }
		/// <summary>
		/// The master registrant that receives the message
		/// </summary>
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		public int? SendingAccountId { get { return (int?)MDV["SendingAccountId"].Value; } set { MDV["SendingAccountId"].Value = value; } }
		public int? SenderId { get { return (int?)MDV["SenderId"].Value; } set { MDV["SenderId"].Value = value; } }
		public string TwilioResourceId { get { return (string)MDV["TwilioResourceId"].Value; } set { MDV["TwilioResourceId"].Value = value; } }
		public string TwilioStatus { get { return (string)MDV["TwilioStatus"].Value; } set { MDV["TwilioStatus"].Value = value; } }
		

        public TwilioSMS()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(TwilioSMS).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(TwilioSMS).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityMessageId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "SendingAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "SenderId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TwilioResourceId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "TwilioStatus", System.Data.SqlDbType.NVarChar , "20");
				
                return _dp;
            }
        }

        public static TwilioSMS LoadObject(System.Data.DataRow row)
        {
            TwilioSMS obj = new TwilioSMS();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(TwilioSMS).Name);
				
                return dq;
            }
        }

        public static TwilioSMS Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<TwilioSMS> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return TwilioSMS.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return TwilioSMS.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(TwilioSMS).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(TwilioSMS).Name);
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
			dps.Add(ObjectMap["ActivityMessageId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SendingAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SenderId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TwilioResourceId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TwilioStatus"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public TwilioSMS Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return TwilioSMS.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static TwilioSMS LoadByTwilioResourceId(string TwilioResourceId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["TwilioResourceId"], TwilioResourceId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static TwilioSMS LoadByRegistrantIdByActivityMessageId(int RegistrantId, int ActivityMessageId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			dq.Criterias.Add(ObjectMap["ActivityMessageId"], ActivityMessageId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<TwilioSMS> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static bool ExistsByRegistrantIdByActivityMessageId(int RegistrantId, int ActivityMessageId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            dq.Criterias.Add(ObjectMap["ActivityMessageId"], ActivityMessageId);
            return dq.GetCount(msgs) > 0;
        }

        #endregion
    }
}
