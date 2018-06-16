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
	[Serializable]
    public class PasswordResetToken : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? WebUserId { get { return (int?)MDV["WebUserId"].Value; } set { MDV["WebUserId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public DateTime? ExpireDate { get { return (DateTime?)MDV["ExpireDate"].Value; } set { MDV["ExpireDate"].Value = value; } }
		public bool Used { get { return (bool)MDV["Used"].Value; } set { MDV["Used"].Value = value; } }
		

        public PasswordResetToken()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(PasswordResetToken).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.Used = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(PasswordResetToken).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "WebUserId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "ExpireDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "Used", System.Data.SqlDbType.Bit);
				
                return _dp;
            }
        }

        protected static PasswordResetToken LoadObject(System.Data.DataRow row)
        {
            PasswordResetToken obj = new PasswordResetToken();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(PasswordResetToken).Name);
				
                return dq;
            }
        }

        public static PasswordResetToken Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<PasswordResetToken> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return PasswordResetToken.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return PasswordResetToken.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.WebUserId.IsEmpty())
				msgs.Add("WebUserId is required on PasswordResetToken object", MessageType.Error);
			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on PasswordResetToken object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(PasswordResetToken).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(PasswordResetToken).Name);
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
			dps.Add(ObjectMap["WebUserId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ExpireDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Used"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public PasswordResetToken Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return PasswordResetToken.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static PasswordResetToken LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public static PasswordResetToken LoadValidByUid(Guid uid, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["uid"], uid);
            dq.Criterias.Add(ObjectMap["ExpireDate"], DateTime.UtcNow, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);
            dq.Criterias.Add(ObjectMap["Used"], false);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static bool ExistsAndIsValidByUid(Guid uid, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["uid"], uid);
            dq.Criterias.Add(ObjectMap["ExpireDate"], DateTime.UtcNow, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);
            dq.Criterias.Add(ObjectMap["Used"], false);
            return dq.GetCount() > 0;
        }

        public static bool ExistsAndIsValidByWebUserId(int WebUserId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
            dq.Criterias.Add(ObjectMap["ExpireDate"], DateTime.UtcNow, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);
            dq.Criterias.Add(ObjectMap["Used"], false);
            return dq.GetCount() > 0;
        }

        public static PasswordResetToken LoadValidByWebUserId(int WebUserId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
            dq.Criterias.Add(ObjectMap["ExpireDate"], DateTime.UtcNow, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);
            dq.Criterias.Add(ObjectMap["Used"], false);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static PasswordResetToken AddOrUpdateToken(WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            PasswordResetToken token = null;

            if (msgs.IsEmpty()) msgs = new SystemMessages();

            if (!PasswordResetToken.ExistsAndIsValidByWebUserId(user.ID.Value, dc: dc))
            {
                token = new PasswordResetToken
                {
                    ExpireDate = DateTime.UtcNow.AddDays(3),
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = user.ID,
                    WebUserId = user.ID
                };
            }
            else
            {
                token = PasswordResetToken.LoadValidByWebUserId(user.ID.Value, dc: dc);
                token.ExpireDate = DateTime.UtcNow.AddDays(3);
            }

            token.LastModifiedDate = DateTime.UtcNow;
            token.LastModifiedBy = user.ID.Value;
            msgs.AddRange(token.Save(dc: dc));

            return token;
        }

        public static SystemMessages UseToken(Guid uid, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var token = PasswordResetToken.LoadValidByUid(uid, dc: adc);
                    token.Used = true;
                    token.LastModifiedBy = token.WebUserId;
                    token.LastModifiedDate = DateTime.UtcNow;
                    amsgs.AddRange(token.Save(adc));

                    amsgs.AddRange(token.GetWebUser(adc).MarkAsVerified(adc));
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            return msgs;
        }

        public SystemMessages Use(DatabaseConnection dc = null)
        {
            return PasswordResetToken.UseToken(this.Uid, dc: dc);
        }

        public WebUser GetWebUser(DatabaseConnection dc = null)
        {
            return WebUserId.HasValue ? WebUser.Load(WebUserId.Value, dc: dc) : null;
        }

        #endregion
    }
}
