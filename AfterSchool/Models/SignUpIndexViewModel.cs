using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Models
{
    public class SignUpIndexViewModel
    {
        public string OrgName { get; set; }
        public string OrgPhone { get; set; }
        public AccountType? OrgType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password1 { get; set; }
        public string Password2 { get; set; }
        public string TimeZone { get; set; }
        public string InviteCodeToShare { get; set; }
        public string HowHearAboutUs { get; set; }
        public bool IsClaimProcess { get; set; }
        public Account Account { get; set; }
        public bool IsRequestOnlyAccess { get; set; }
        public SignUpType SignupType { get; set; }
        public Account InvitingAccount { get; set; }
        
        public string PackageName { get; set; }
        public string PackageUid { get; set; }
        
        public decimal TrialDays { get; set; }
        public string SubscriptionPeriod { get; set; }
    }
}