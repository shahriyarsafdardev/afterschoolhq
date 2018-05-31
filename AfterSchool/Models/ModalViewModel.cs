using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Models
{
    public class ModalViewModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string HtmlBody { get; set; }
        public string ActionButtonId { get; set; }
        public string ActionButtonHtml { get; set; }
    }
}