using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Web;
using System.Web.WebPages;

namespace AfterSchool.Areas.Api.Models
{
    public class SelectRegistrantsDto
    {
        public int? ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsParentOrGuardian { get; set; }    
        public bool IsAdult { get; set; }
        public Guid Uid { get; set; }    
        
        public bool IsGradeRestricted { get; set; }

        public bool IsAgeRestricted { get; set; }

        public bool IsGenderRestricted { get; set; }
        public bool CanStillRegister { get; set; }   

        public bool HasSubmittedForActivity { get; set; }

        public bool HasWithDrawnFromActivity { get; set; }

        public bool HasCancelledSubscription { get; set; }  

        public bool HasConsentFor { get; set; }

        public bool IsOnWaitingListFor { get; set; }

        public bool IsReservedAndPaidFor { get ; set; }

        public bool IsAppliedFor { get;set; }

        public bool MasterRegistrantHasPlan { get; set; }   

        public Guid? MakePaymentAcc { get; set; }

        public bool IsProfileCompleted { get; set; }

        public string? IneligibleText { get; set; }

        public string? Grade { get; set; }

        public static SelectRegistrantsDto MapToDto(SelectRegistrantsDto registrant)
        {
            return new SelectRegistrantsDto()
            {
                ID = registrant.ID,
                FirstName = registrant.FirstName,
                LastName = registrant.LastName,
                IsParentOrGuardian = registrant.IsParentOrGuardian,
                IsAdult=registrant.IsAdult,
                Uid= registrant.Uid,
                IsGradeRestricted=registrant.IsGradeRestricted,
                IsAgeRestricted=registrant.IsAgeRestricted,
                IsGenderRestricted=registrant.IsGenderRestricted,
                CanStillRegister=registrant.CanStillRegister,
                HasSubmittedForActivity=registrant.HasSubmittedForActivity,
                HasWithDrawnFromActivity = registrant.HasWithDrawnFromActivity,
                HasCancelledSubscription = registrant.HasCancelledSubscription,
                HasConsentFor = registrant.HasConsentFor,
                IsOnWaitingListFor = registrant.IsOnWaitingListFor,
                IsReservedAndPaidFor = registrant.IsReservedAndPaidFor,
                IsAppliedFor = registrant.IsAppliedFor,
                MasterRegistrantHasPlan = registrant.MasterRegistrantHasPlan,
                MakePaymentAcc = registrant.MakePaymentAcc,
                IsProfileCompleted = registrant.IsProfileCompleted,
                IneligibleText = registrant.IneligibleText,
                Grade = registrant.Grade,
            };
        }

        public static List<SelectRegistrantsDto> MapToDtoList(List<SelectRegistrantsDto> Registrants)
        {
            return Registrants
                .Select(question =>
                MapToDto(question))
                .ToList();
        }


        public static string GetGradeHelper(Registrant student)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(student.GetAnswer(QuestionDataType.RegistrantGrade).Text);
            JsonElement rootElement = jsonDocument.RootElement;
            string grade = rootElement[0].GetProperty("Value").GetString();
            return grade;
        }

    }
}