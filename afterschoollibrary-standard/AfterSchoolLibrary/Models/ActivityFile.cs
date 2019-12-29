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
    /// Keeps files with one specific activity. Not currently used.
    /// </summary>
	
    public class ActivityFile : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? FileId { get { return (int?)MDV["FileId"].Value; } set { MDV["FileId"].Value = value; } }
		public string FileFilename { get { return (string)MDV["FileFilename"].Value; } }
		public string FileFileLocation { get { return (string)MDV["FileFileLocation"].Value; } }
		public int? FileFileSizeInBytes { get { return (int?)MDV["FileFileSizeInBytes"].Value; } }
		public string FileLabel { get { return (string)MDV["FileLabel"].Value; } }
		public string FileDescription { get { return (string)MDV["FileDescription"].Value; } }
		public FileType FileFileTypeId { get { return (FileType)MDV["FileFileTypeId"].Value; } }
		

        public ActivityFile()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityFile).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityFile).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "FileId", System.Data.SqlDbType.Int);
				_dp.Add(className, "FileFilename", typeof(File).Name, "Filename", System.Data.SqlDbType.NVarChar, "File");
				_dp.Add(className, "FileFileLocation", typeof(File).Name, "FileLocation", System.Data.SqlDbType.NVarChar, "File");
				_dp.Add(className, "FileFileSizeInBytes", typeof(File).Name, "FileSizeInBytes", System.Data.SqlDbType.Int, "File");
				_dp.Add(className, "FileLabel", typeof(File).Name, "Label", System.Data.SqlDbType.NVarChar, "File");
				_dp.Add(className, "FileDescription", typeof(File).Name, "Description", System.Data.SqlDbType.NVarChar, "File");
				_dp.Add(className, "FileFileTypeId", typeof(File).Name, "FileTypeId", System.Data.SqlDbType.Int, "File");
				
                return _dp;
            }
        }

        protected static ActivityFile LoadObject(System.Data.DataRow row)
        {
            ActivityFile obj = new ActivityFile();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityFile).Name);
				dq.Joins.Add(File.ObjectMap["ID"], ActivityFile.ObjectMap["FileId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "File");
				
                return dq;
            }
        }

        public static ActivityFile Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityFile> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityFile.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityFile.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ActivityId.IsEmpty())
				msgs.Add("ActivityId is required on ActivityFile object", MessageType.Error);
			if (this.FileId.IsEmpty())
				msgs.Add("FileId is required on ActivityFile object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityFile).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityFile).Name);
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
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FileId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityFile Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityFile.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityFile> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityFile> LoadByFileId(int FileId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FileId"], FileId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityFile LoadByActivityIdByFileId(int ActivityId, int FileId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["FileId"], FileId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"
        
        public File ToFile()
        {
            return new File()
            {
                ID = this.FileId,
                Filename = this.FileFilename,
                FileSizeInBytes = this.FileFileSizeInBytes,
                FileLocation = this.FileFileLocation,
                FileTypeId = this.FileFileTypeId,
                Label = this.FileLabel,
                Description = this.FileDescription
            };
        }

        #endregion
    }
}
