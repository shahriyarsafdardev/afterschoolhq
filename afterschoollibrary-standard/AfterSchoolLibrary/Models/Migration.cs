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
    /// Store migration history
    /// </summary>
	
    public class Migration : BasicObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public string UniqueIdentifier { get { return (string)MDV["UniqueIdentifier"].Value; } set { MDV["UniqueIdentifier"].Value = value; } }
		

        public Migration()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Migration).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Migration).Name;
                _dp = BasicObject.get_BaseObjectMap(className);
				_dp.Add(className, "UniqueIdentifier", System.Data.SqlDbType.NVarChar , "100");
				
                return _dp;
            }
        }

        protected static Migration LoadObject(System.Data.DataRow row)
        {
            Migration obj = new Migration();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Migration).Name);
				
                return dq;
            }
        }

        public static Migration Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Migration> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Migration.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Migration.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.UniqueIdentifier.IsEmpty())
				msgs.Add("UniqueIdentifier is required on Migration object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Migration).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Migration).Name);
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
			dps.Add(ObjectMap["UniqueIdentifier"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Migration Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Migration.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static Migration LoadByUniqueIdentifier(string UniqueIdentifier, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["UniqueIdentifier"], UniqueIdentifier);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public static bool ExistsByUniqueIdentifier(string UniqueIdentifier, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["UniqueIdentifier"], UniqueIdentifier);
            return dq.GetCount(msgs) > 0;
        }

        public static Migration CreateNew(string uniqueIdentifier, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var migration = new Migration()
            {
                UniqueIdentifier = uniqueIdentifier,
                CreateDate = DateTime.UtcNow,
                CreatedBy = 0
            };

            msgs.AddRange(migration.Save(dc));

            return migration;
        }

        public delegate void MigrationAction(SystemMessages msgs, DatabaseConnection dc);

        public static void Run(MigrationAction action, string uniqueIdenfier, SystemMessages msgs, DatabaseConnection dc)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            if (!Migration.ExistsByUniqueIdentifier(uniqueIdenfier, msgs, dc))
            {
                action(msgs, dc);

                if (msgs.HasNoErrors())
                    Migration.CreateNew(uniqueIdenfier, msgs, dc);
            }
        }

        #endregion
    }
}
