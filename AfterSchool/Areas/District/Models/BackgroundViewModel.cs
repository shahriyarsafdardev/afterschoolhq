using System;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.District.Models
{
    public class BackgroundViewModel
    {
        public int Id { get; set; }
        public int WebUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public BackgroundCheckResult BackgroundCheckStatuResult { get; set; } 
        public DateTime DateSubmitted { get; set; }
    }
    
    public class BackgroundAssembler
    {
        public static BackgroundViewModel BackgroundAssemblerResult(Registrant registrant, BackgroundCheck background)
        {
            return new BackgroundViewModel()
            {
                Id = (int)background.ID,
                BackgroundCheckStatuResult = background.BackgroundCheckResultId,
                WebUserId = (int)registrant.WebUserId,
                FirstName = registrant.FirstName,
                LastName = registrant.LastName,
                DateSubmitted = (DateTime)background.SterlingEmailSentDate
            };
        }
    }
}