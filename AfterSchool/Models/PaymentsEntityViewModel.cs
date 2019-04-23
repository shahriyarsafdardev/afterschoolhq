using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Models
{
    public class PaymentsEntityViewModel
    {
        public LegalEntityType? BusinessType { get; set; }
        public string BusinessName { get; set; }
        public string BusinessUrl { get; set; }
        public string BusinessPhone { get; set; }
        public string EIN { get; set; }

        public string ProductDescription { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string SupportPhone { get; set; }
        public string SupportEmail { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public string PersonalEmail { get; set; }
        public string PeronsalPhone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string SSNLast4 { get; set; }
        public string SSN { get; set; }

        public bool TOSAccept { get; set; }
        public DateTime? TOSAcceptanceDate { get; set; }
        public string TOSAcceptanceIP { get; set; }
        public string TOSAcceptanceUserAgent { get; set; }
    }
}