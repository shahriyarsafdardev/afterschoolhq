//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BasicLibrary;
//using BasicLibrary.DataAccess;
//using System.Data;
//using System.Collections;

//namespace AfterSchoolLibrary
//{
	
//    public class Archive : BasicLookUpObject
//    {
//		#region "ClassGenerator"

//		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		

//        public Archive()
//        {
//            this.Config(ObjectMap, DatabaseQuery, typeof(Archive).Name, "ID");
//            this.LoadObjectValues(ObjectMap);
			
//        }

//        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
//        {
//            get
//            {
//                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
//                string className = typeof(Archive).Name;
//                _dp = BasicLookUpObject.get_BaseObjectMap(className);
//				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				
//                return _dp;
//            }
//        }

//        protected static Archive LoadObject(System.Data.DataRow row)
//        {
//            Archive obj = new Archive();
//            obj.LoadObjectValues(ObjectMap, row);
//            return obj;
//        }

//        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
//        {
//            get
//            {
//                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Archive).Name);
				
//                return dq;
//            }
//        }

//        public static Archive Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//            dq.Connection = dc;
//            dq.Criterias.Add(ObjectMap["ID"], id);
//            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
//        }

//        public static List<Archive> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//            dq.Connection = dc;
//			dq.UseParameterizedQuery = false;
//			dq.SelectRange.Start = start;
//			dq.SelectRange.Length = length;
//            return Archive.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
//        }

//        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//            dq.Connection = dc;
//            return Archive.BaseGetCount(criterias, ObjectMap, dq);
//        }

//		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            return base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Archive).Name);
//        }

//        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Archive).Name);
//        }

//        public static SystemMessages Create(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
//        }

//        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//        {
//            SystemMessages msgs = new SystemMessages();
//            BasicLibrary.DataAccess.DatabaseParameters dps;

//			dps = new BasicLibrary.DataAccess.DatabaseParameters();
//			dps.Add(ObjectMap["AccountId"]);
//			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

//            return msgs;
//        }

//		public Archive Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
//		{
//			return Archive.Load(this.ID.Value, dc);
//		}
		
//		public static List<Archive> LoadByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
//		{
//			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
//			dq.Connection = dc;
//			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
//			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
//		}

		

//        #endregion

//		#region "UserDefined"

//        #endregion
//    }
//}
