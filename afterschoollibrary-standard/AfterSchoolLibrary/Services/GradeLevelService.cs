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
    }
}
