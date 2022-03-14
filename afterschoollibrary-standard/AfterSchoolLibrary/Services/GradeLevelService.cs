using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class GradeLevelService
    {
        public static bool VerifyGradeLevel(this GradeLevel gradeLevel, FormQuestionAnswer formQuestionAnswer)
        {
            return VerifyGradeLevel(gradeLevel, formQuestionAnswer.Text); 
        }

        public static bool VerifyGradeLevel(this GradeLevel gradeLevel, string answer)
        {
            var registrantGradeLevel = answer;
            if (Helpers.Utilities.IsAlreadyJsonString(registrantGradeLevel))
            {
                var data = Helpers.Utilities.GetKeyValuePairs(registrantGradeLevel);
                registrantGradeLevel = data.First().value;
            }

            return gradeLevel.Name.ToLower() == registrantGradeLevel.ToLower().Trim() || gradeLevel.Abbreviation.ToLower() == registrantGradeLevel.ToLower().Trim();
        }
    }
}
