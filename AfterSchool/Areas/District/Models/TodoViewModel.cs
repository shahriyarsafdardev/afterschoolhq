using System;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.District.Models
{
    public class TodoViewModel
    {
        public int WebUserId { get; set; }
        public int AccountId { get; set; }
        public int TaskId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
        public TaskStatus TaskStatus { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime? DateCompleted { get; set; }
    }

    public class TodoAssembler
    {
        public static TodoViewModel TodoAssemblerResult(WebUser webUser, VolunteerTask task)
        {
            return new TodoViewModel()
            {
                WebUserId = (int)webUser.ID,
                AccountId = (int)task.AccountId,
                TaskId = (int)task.ID,
                FirstName = webUser.FirstName,
                LastName = webUser.LastName,
                Phone = webUser.Phone,
                ApplicationStatus = task.ApplicationStatus,
                TaskStatus = task.Task,
                DateSubmitted = task.DateSubmitted,
                DateCompleted = task.DateCompleted
            };
        }
    }
    
}