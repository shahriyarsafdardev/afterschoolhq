using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Areas.Api.Models
{
    public class FormSettingsDto
    {
        public int FormId { get; set; }
        public FormType? FormType { get; set; }
        public string? FormName { get; set; }
        public string? FormDescription { get; set; }

        public static FormSettingsDto MapToDto(Form Form)
        {
            return new FormSettingsDto()
            {
                FormId = Form.ID ?? 0,
                FormType = Form.FormTypeId,
                FormName = Form.Name,
                FormDescription = Form.Description
            };
        }

        public void UpdateIfNotNull(Form Form)
        {
            Form.ID = this.FormId;
            Form.FormTypeId = this.FormType??Form.FormTypeId;
            Form.Name = this.FormName??Form.Name;
            Form.Description = this.FormDescription??Form.Description;
        }

    }
}