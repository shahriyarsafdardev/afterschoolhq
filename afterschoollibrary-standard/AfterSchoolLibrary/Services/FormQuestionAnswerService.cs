using AfterSchoolLibrary.Models;
using System.Linq;
using BasicLibrary;

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
            {
                // apparently ratings have the value as the text and sometimes the key
                // to be consistent need to always return the key instead
                if (answer.FormQuestionQuestionTypeId == QuestionType.Rating)
                    return Helpers.Utilities.GetKeyValuePairs(answer.Text).FirstOrDefault()?.key;

                return Helpers.Utilities.GetKeyValuePairs(answer.Text).FirstOrDefault()?.value;
            }

            if (answer.FormQuestionQuestionTypeId == QuestionType.Checkbox)
                return answer.Text.IsBoolean() ? answer.Text.ToBoolean().ToYesNo() : "";

            return answer.Text;
        }
    }
}
