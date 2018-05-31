using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;

namespace AfterSchool.Models
{
    public class ImportField
    {
        public string Value { get; set; }

        private SystemMessages _Messages;
        public SystemMessages Messages { get { return _Messages; } }

        public ImportField()
        {
            _Messages = new SystemMessages();
        }
    }
}