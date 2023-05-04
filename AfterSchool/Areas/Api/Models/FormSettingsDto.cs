using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Areas.Api.Models
{
    public class FormSettingsDto
    {
        public int formId { get; set; }
        public FormType? formType { get; set; }
        public string? formName { get; set; }
        public string? formDescription { get; set; }

        public static FormSettingsDto fromForm(Form form)
        {
            return new FormSettingsDto()
            {
                formId = form.ID ?? 0,
                formType = form.FormTypeId,
                formName = form.Name,
                formDescription = form.Description
            };
        }

        public void UpdateFormIfNotEmpty(Form form)
        {
            form.ID = this.formId;
            form.FormTypeId = this.formType??form.FormTypeId;
            form.Name = this.formName??form.Name;
            form.Description = this.formDescription??form.Description;
        }

    }
}