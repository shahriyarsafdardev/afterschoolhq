using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
