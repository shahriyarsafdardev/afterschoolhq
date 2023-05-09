using System;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.District.Models
{
    public class VolunteerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int WebUserId { get; set; }
        public int AccountId { get; set; }
        public int? VolunteerApprovalId { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
    public class VolunteerAssembler
    {
        public static VolunteerViewModel VolunteerAssemblerResult(Registrant registrant, VolunteerApproval volunteerApproval = null)
        {
            return new VolunteerViewModel()
            {
                WebUserId = (int)registrant.WebUserId,
                AccountId = (int)registrant.AccountId,
                VolunteerApprovalId = volunteerApproval?.ID,
                FirstName = registrant.FirstName,
                LastName = registrant.LastName,
                Phone = registrant.Phone,
                Email = registrant.Email,
                ApprovalStatus = volunteerApproval.IsNotEmpty() ? volunteerApproval.ApprovalStatusId : ApprovalStatus.NotSet,
                ExpirationDate = volunteerApproval?.ExpirationDate,
                CreatedDate = volunteerApproval?.CreateDate
            };
        }
    }
}