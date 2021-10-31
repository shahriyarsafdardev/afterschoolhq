using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Models
{
	/// <summary>
    /// A simple list of grades, such as Kindergarten, 1st Grade, etc.
    /// </summary>
	
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
				_dp.Add(className, "Abbreviation", System.Data.SqlDbType.NVarChar , "50");
				
                return _dp;
            }
        }

        public static GradeLevel LoadObject(System.Data.DataRow row)
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

        public static GradeLevel Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<GradeLevel> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return GradeLevel.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return GradeLevel.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Name.IsEmpty())
				msgs.Add("Name is required on GradeLevel object", MessageType.Error);
			
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

		public GradeLevel Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return GradeLevel.Load(this.ID.Value, msgs, dc: dc);
		}
		
		

        #endregion

		#region "UserDefined"

        public static SystemMessages CreateDefaultGradeLevels(DatabaseConnection dc  = null)
        {
            var msgs = new SystemMessages();
            var gradeLevelCount = GradeLevel.GetCount(msgs: msgs, dc: dc);
            if (gradeLevelCount == 0)
            {
                msgs.AddRange(new GradeLevel() { Name = "Pre-K", Abbreviation = "P" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "Kindergarten", Abbreviation = "K" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "1st", Abbreviation = "1" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "2nd", Abbreviation = "2" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "3rd", Abbreviation = "3" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "4th", Abbreviation = "4" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "5th", Abbreviation = "5" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "6th", Abbreviation = "6" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "7th", Abbreviation = "7" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "8th", Abbreviation = "8" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "9th", Abbreviation = "9" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "10th", Abbreviation = "10" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "11th", Abbreviation = "11" }.Save(dc: dc));
                msgs.AddRange(new GradeLevel() { Name = "12th", Abbreviation = "12" }.Save(dc: dc));
            }
            return msgs;
        }

        public static bool VerifyGradeLevel(FormQuestionAnswer formQuestionAnswer, GradeLevel gradeLevel)
        {
            bool sameGradeLevel = false;
            if (Helpers.Utilities.IsAlreadyJsonString(formQuestionAnswer.Text)) 
            {
                var data = Helpers.Utilities.GetKeyValuePairs(formQuestionAnswer.Text);
                var registrantGradeLevelAnswerText = data.First().value.ToLower().Trim();
                sameGradeLevel = gradeLevel.Abbreviation.IsSameAs(registrantGradeLevelAnswerText) || (registrantGradeLevelAnswerText.MaxChars(gradeLevel.Abbreviation.Length) == gradeLevel.Abbreviation);
            }
            else if (formQuestionAnswer.IsNotEmpty())
            {
                sameGradeLevel = gradeLevel.Abbreviation.IsSameAs(formQuestionAnswer.Text) || (formQuestionAnswer.Text.MaxChars(gradeLevel.Abbreviation.Length) == gradeLevel.Abbreviation);
            }

            return sameGradeLevel;
        }

        #endregion
    }
}
