using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Interop;

namespace AfterSchool.Models
{
    public class MultiFactorViewModel
    {
        public SystemMessages Messages { get; set; }
        public string UID { get; set; }
        public string ReturnURL { get; set; }
        public string Invite { get; set; }
        public string Remember { get; set; }
        public DateTime ExpirationDate { get; set; }

        public MultiFactorViewModel(SystemMessages messages, string uid, string returnurl, string invite, string remember, DateTime expirationDate)
        {
            Messages = messages;
            UID = uid;
            ReturnURL = returnurl;
            Invite = invite;
            Remember = remember;
            ExpirationDate = expirationDate;
        }
    }
}