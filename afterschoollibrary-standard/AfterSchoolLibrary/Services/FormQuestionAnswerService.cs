using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Helpers;
using System.Linq;

namespace AfterSchoolLibrary.Services
{
    public static class FormQuestionAnswerService
    {
        public static bool IsMultiOptionOrEmoji(QuestionType formQuestionQuestionTypeId)
        {
            if (formQuestionQuestionTypeId == QuestionType.DropDownList || formQuestionQuestionTypeId == QuestionType.RadioList || formQuestionQuestionTypeId == QuestionType.Rating || formQuestionQuestionTypeId == QuestionType.Emoji)
                return true;
            else
                return false;
        }

        public static string GetAnswerText(this FormQuestionAnswer answer)
        {
            if (answer.Text.IsJson())
                return Utilities.GetKeyValuePairs(answer.Text).FirstOrDefault()?.value;

            return answer.Text;
        }
    }
}
