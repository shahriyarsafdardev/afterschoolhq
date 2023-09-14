using System;
using System.Collections.Generic;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Linq;

namespace AfterSchoolLibrary.Models
{
    public class RegistrantNoteVersion : BasicObject
    {
        #region "ClassGenerator"

        public string Title
        {
            get => (string) MDV["Title"].Value;
            set => MDV["Title"].Value = value;
        }

        public string Text
        {
            get => (string) MDV["Text"].Value;
            set => MDV["Text"].Value = value;
        }

        public int RegistrantNoteId
        {
            get => (int) MDV["RegistrantNoteId"].Value;
            set => MDV["RegistrantNoteId"].Value = value;
        }

        public bool IsDeleted
        {
            get => (bool) MDV["IsDeleted"].Value;
            set => MDV["IsDeleted"].Value = value;
        }

        public RegistrantNoteVersion()
        {
            Config(ObjectMap, DatabaseQuery, typeof(RegistrantNoteVersion).Name, "ID");
            LoadObjectValues(ObjectMap);
            IsDeleted = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(RegistrantNoteVersion).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "Title", SqlDbType.NVarChar, "400");
                _dp.Add(className, "Text", SqlDbType.NVarChar, "4000");
                _dp.Add(className, "RegistrantNoteId", SqlDbType.Int);
                _dp.Add(className, "IsDeleted", SqlDbType.Bit);

                return _dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantNoteVersion).Name);

                return dq;
            }
        }

        public static RegistrantNoteVersion LoadObject(DataRow row)
        {
            var obj = new RegistrantNoteVersion();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(RegistrantNoteVersion)));
            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantNoteVersion).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["Title"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["Text"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["RegistrantNoteId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["IsDeleted"]}));

            return msgs;
        }

        public static List<RegistrantNoteVersion> LoadByNoteIdByIsDeleted(int noteId, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantNoteId"], noteId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static RegistrantNoteVersion SearchByNoteIdByIsDeleted(string searchQuery, bool isDeleted, int noteVersionId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Id"], noteVersionId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);

            var searchDcs = new DatabaseCriterias(LogicalOperatorType.Or);
            searchDcs.Add(ObjectMap["Title"], searchQuery, DatabaseCriteriaOperatorEnum.Like);
            searchDcs.Add(ObjectMap["Text"], searchQuery, DatabaseCriteriaOperatorEnum.Like);
            dq.Criterias.Add(searchDcs);

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion
    }
}