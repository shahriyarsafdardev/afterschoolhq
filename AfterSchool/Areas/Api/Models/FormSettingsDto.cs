using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Linq;
using System.Web.WebPages;

namespace AfterSchool.Areas.Api.Models
{
    public class FormSettingsDto
    {
        public int? FormId { get; set; }
        public FormType FormType { get; set; }
        public string? FormName { get; set; }
        public string? FormDescription { get; set; }
        public bool? IsPublished { get; set; }
        public string PublishedURL { get; set; }    

        public bool IsNull()
        {
            return FormId == null;
        }

        public bool IsForcedNull()
        {
            return IsNull() || FormName.IsEmpty();
        }

        public static FormSettingsDto MapToDto(Form Form)
        {
            return new FormSettingsDto()
            {
                FormId = Form.ID,
                FormType = Form.FormTypeId,
                FormName = Form.Name,
                FormDescription = Form.Description,
                IsPublished = Form.IsPublished ?? false,
                PublishedURL = FormService.BuildShortHomePageUrlForm(Form.Uid)
            };
        }

        public void UpdateIfNotNull(Form Form)
        {
            if (!this.IsForcedNull())
            {
                Form.FormTypeId = this.FormType;
                Form.Name = this.FormName;
                Form.Description = this.FormDescription;
            }
        }

    }
}