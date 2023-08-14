using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;

namespace AfterSchoolLibrary.Models
{
    public class RegistrantNote : BasicObject
    {
        #region "ClassGenerator"

        public DateTime PostDate
        {
            get => (DateTime) MDV["PostDate"].Value;
            set => MDV["PostDate"].Value = value;
        }
        
        public int FirstRegistrantId
        {
            get => (int) MDV["FirstRegistrantId"].Value;
            set => MDV["FirstRegistrantId"].Value = value;
        }
        
        public bool IsPinned
        {
            get => (bool) MDV["IsPinned"].Value;
            set => MDV["IsPinned"].Value = value;
        }
        
        public bool IsDeleted
        {
            get => (bool) MDV["IsDeleted"].Value;
            set => MDV["IsDeleted"].Value = value;
        }
        
        public RegistrantNote()
        {
            Config(ObjectMap, DatabaseQuery, typeof(RegistrantNote).Name, "ID");
            LoadObjectValues(ObjectMap);
            IsPinned = false;
			IsDeleted = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                DatabaseParameters _dp = new DatabaseParameters();
                string className = typeof(RegistrantNote).Name;
                _dp = get_BaseObjectMap(className);
				_dp.Add(className, "PostDate", SqlDbType.Date);
				_dp.Add(className, "FirstRegistrantId", SqlDbType.Int);
                _dp.Add(className, "IsPinned", SqlDbType.Bit);
				_dp.Add(className, "IsDeleted", SqlDbType.Bit);
				
                return _dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantNote).Name);
				
                return dq;
            }
        }
        
        public static RegistrantNote LoadObject(DataRow row)
        {
            var obj = new RegistrantNote();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

		public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(RegistrantNote)));
            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantNote).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["PostDate"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["FirstRegistrantId"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["IsPinned"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["IsDeleted"] }));

            return msgs;
        }
        #endregion
    }
}