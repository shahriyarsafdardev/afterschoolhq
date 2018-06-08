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
	
    public class GradeLevel : BasicLookUpObject
    {
		#region "ClassGenerator"

		public string Abbreviation { get { return (string)MDV["Abbreviation"].Value; } set { MDV["Abbreviation"].Value = value; } }
		

        public GradeLevel()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(GradeLevel).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(GradeLevel).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "Abbreviation", System.Data.SqlDbType.NVarChar, 50);
				
                return _dp;
            }
        }

        protected static GradeLevel LoadObject(System.Data.DataRow row)
        {
            GradeLevel obj = new GradeLevel();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(GradeLevel).Name);
				
                return dq;
            }
        }

        public static GradeLevel Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<GradeLevel> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return GradeLevel.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return GradeLevel.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(GradeLevel).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(GradeLevel).Name);
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
			dps.Add(ObjectMap["Abbreviation"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public GradeLevel Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return GradeLevel.Load(this.ID.Value, dc);
		}
		
		

        #endregion

		#region "UserDefined"

        #endregion
    }
}
