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
	
    public class AccountInvoice : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? AccountPackageId { get { return (int?)MDV["AccountPackageId"].Value; } set { MDV["AccountPackageId"].Value = value; } }
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public int? AccountPackagePackageId { get { return (int?)MDV["AccountPackagePackageId"].Value; } }
		public DateTime? AccountPackageStartDate { get { return (DateTime?)MDV["AccountPackageStartDate"].Value; } }
		public DateTime? AccountPackageEndDate { get { return (DateTime?)MDV["AccountPackageEndDate"].Value; } }
		public DateTime? AccountPackageTrialStartDate { get { return (DateTime?)MDV["AccountPackageTrialStartDate"].Value; } }
		public DateTime? AccountPackageTrialEndDate { get { return (DateTime?)MDV["AccountPackageTrialEndDate"].Value; } }
		public string PackageName { get { return (string)MDV["PackageName"].Value; } }
		public string PackageMaxActivities { get { return (string)MDV["PackageMaxActivities"].Value; } }
		public string PackageMaxRegistrants { get { return (string)MDV["PackageMaxRegistrants"].Value; } }
		public string PackageMaxUploadSpace { get { return (string)MDV["PackageMaxUploadSpace"].Value; } }
		

        public AccountInvoice()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountInvoice).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountInvoice).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountPackageId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "AccountPackagePackageId", typeof(AccountPackage).Name, "PackageId", System.Data.SqlDbType.Int, "AccountPackage");
				_dp.Add(className, "AccountPackageStartDate", typeof(AccountPackage).Name, "StartDate", System.Data.SqlDbType.DateTime, "AccountPackage");
				_dp.Add(className, "AccountPackageEndDate", typeof(AccountPackage).Name, "EndDate", System.Data.SqlDbType.DateTime, "AccountPackage");
				_dp.Add(className, "AccountPackageTrialStartDate", typeof(AccountPackage).Name, "TrialStartDate", System.Data.SqlDbType.DateTime, "AccountPackage");
				_dp.Add(className, "AccountPackageTrialEndDate", typeof(AccountPackage).Name, "TrialEndDate", System.Data.SqlDbType.DateTime, "AccountPackage");
				_dp.Add(className, "PackageName", typeof(Package).Name, "Name", System.Data.SqlDbType.NVarChar, "Package");
				_dp.Add(className, "PackageMaxActivities", typeof(Package).Name, "MaxActivities", System.Data.SqlDbType.NVarChar, "Package");
				_dp.Add(className, "PackageMaxRegistrants", typeof(Package).Name, "MaxRegistrants", System.Data.SqlDbType.NVarChar, "Package");
				_dp.Add(className, "PackageMaxUploadSpace", typeof(Package).Name, "MaxUploadSpace", System.Data.SqlDbType.NVarChar, "Package");
				
                return _dp;
            }
        }

        protected static AccountInvoice LoadObject(System.Data.DataRow row)
        {
            AccountInvoice obj = new AccountInvoice();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AccountInvoice).Name);
				dq.Joins.Add(AccountPackage.ObjectMap["PackageId"], AccountInvoice.ObjectMap["PackageId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "AccountPackage");
				dq.Joins.Add(Package.ObjectMap["ID"], AccountPackage.ObjectMap["PackageId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Package");
				
                return dq;
            }
        }

        public static AccountInvoice Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountInvoice> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AccountInvoice.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AccountInvoice.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountInvoice).Name);
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountInvoice).Name);
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
			dps.Add(ObjectMap["AccountPackageId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AccountInvoice Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AccountInvoice.Load(this.ID.Value, dc);
		}
		
		public static List<AccountInvoice> LoadByAccountPackageId(int AccountPackageId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountPackageId"], AccountPackageId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountInvoice> LoadByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static AccountInvoice LoadByUid(Guid Uid, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public List<AccountPayment> GetPayments(DatabaseConnection dc = null)
        {
            return AccountPayment.LoadByInvoiceId(this.ID.Value, dc);
        }

        #endregion
    }
}
