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
            var registrantGradeLevel = formQuestionAnswer.Text;
            if (Helpers.Utilities.IsAlreadyJsonString(formQuestionAnswer.Text))
            {
                var data = Helpers.Utilities.GetKeyValuePairs(formQuestionAnswer.Text);
                registrantGradeLevel = data.First().value.ToLower().Trim(); 
            }

            return gradeLevel.Abbreviation.IsSameAs(registrantGradeLevel) || (registrantGradeLevel.MaxChars(gradeLevel.Abbreviation.Length) == gradeLevel.Abbreviation);
        }
    }
}
