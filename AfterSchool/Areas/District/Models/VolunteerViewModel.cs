using System;
using AfterSchoolLibrary.Models;

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
        public int VolunteerApprovalId { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
    public class VolunteerAssembler
    {
        public static VolunteerViewModel VolunteerAssemblerResult(Registrant registrant, VolunteerApproval volunteerApproval)
        {
            return new VolunteerViewModel()
            {
                WebUserId = (int)registrant.WebUserId,
                AccountId = (int)registrant.AccountId,
                VolunteerApprovalId = (int)volunteerApproval.ID,
                FirstName = registrant.FirstName,
                LastName = registrant.LastName,
                Phone = registrant.Phone,
                Email = registrant.Email,
                ApprovalStatus = volunteerApproval.ApprovalStatusId,
                ExpirationDate = (DateTime)volunteerApproval.ExpirationDate
            };
        }
    }
}