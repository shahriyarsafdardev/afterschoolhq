using AfterSchoolLibrary;
using BasicLibrary;
using System.Collections.Specialized;

namespace AfterSchool.Models
{
    public class FormQuestionViewModel
    {
        public FormQuestion Question { get; set; }
        public NameValueCollection Values { get; set; }

        public FormQuestionViewModel(FormQuestion question, NameValueCollection values)
        {
            Question = question;
            Values = values;
        }

        public FormQuestionViewModel(FormQuestion question)
        {
            Question = question;
            Values = new NameValueCollection();
        }

        public string RequiredProperty
        {
            get
            {
                return Question.IsRequired ? " data-required " : "";
            }
        }

        public string Id
        {
            get
            {
                return $"question-{((int)Question.ID).ToString()}";
            }
        }

        public string Name
        {
            get
            {
                return Id;
            }
        }

        public string ToolTip
        {
            get
            {
                return Question.IsRequired ? " data-toggle='popover' data-trigger='hover' data-content='required' data-placement='right' " : "";
            }
        }

        public string ToolTipId
        {
            get
            {
                return Question.IsRequired ? $" data-tooltip-id='{Id}' " : "";
            }
        }

        public string RequiredStar
        {
            get
            {
                return Question.IsRequired ? "<span class='text-danger'>*</span>" : " (optional)";
            }
        }

        public string CheckedProperty
        {
            get
            {
                return Value == "1" ? " checked=\"checked\" " : "";
            }
        }

        public string DefaultValue
        {
            get
            {
                if (Question.QuestionTypeId == QuestionType.Checkbox)
                    return "1";

                return "";
            }
        }

        public string Value
        {
            get
            {
                return Values[this.Name];
            }
        }
    }
}