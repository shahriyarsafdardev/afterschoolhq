using System.Drawing;
using System.Net;
using System.Text;
using AfterSchool.Controllers;
using BasicLibrary.DataAccess;
using TinyCsvParser;
using AfterSchoolAdHoc.Mapper;
using AfterSchoolAdHoc.DTOs;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Twilio.Http;
using HttpClient = System.Net.Http.HttpClient;
using Utilities = BasicLibrary.Utilities;

namespace AfterSchoolAdHoc;

public class ImportVolunteer
{
    public static void Run(string filePath, DatabaseConnection dc)
    {
        try
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvVolunteerMapper csvMapper = new CsvVolunteerMapper();
            CsvParser<VolunteerDto> csvParser = new CsvParser<VolunteerDto>(csvParserOptions, csvMapper);
            
            var volunteers = csvParser.ReadFromFile(filePath, Encoding.ASCII).ToList();
            
            ParticipationObject participationObject = new ParticipationObject(ActivityType.VolunteerOpportunity);
            participationObject.OwnerAccount = new Account();
            participationObject.OwnerAccount.ID = 5;
            
            var msgs = new SystemMessages();
            
            foreach (var volunteer in volunteers)
            {
                if(volunteer.Result.status.Contains("Approved") && volunteer.Result.email.IsEmail() && !WebUser.ExistsByEmail(volunteer.Result.email,msgs,dc) )
                {
                    
                    WebUser user = null;
                    Account account = null;
                    Registration registration = null;

                    Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        Registrant registrant = null;
                        var user = new WebUser()
                        {
                            FirstName = volunteer.Result.legalFirstName.IsEmpty() || volunteer.Result.legalFirstName.Length > 50 ? volunteer.Result.preferedName : volunteer.Result.legalFirstName ,
                            LastName = volunteer.Result.legalSecondName,
                            Email = volunteer.Result.email,
                            Phone = null,
                            IsActive = true,
                            IsRegistered = true,
                            VerifiedAccount = false,
                            AllowTextMessages = false,
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = null,
                            LastModifiedDate = DateTime.UtcNow,
                            LastModifiedBy = null,
                            Username = WebUserService.GenerateUsername(volunteer.Result.legalFirstName.IsEmpty() || volunteer.Result.legalFirstName.Length > 50 ? volunteer.Result.preferedName : volunteer.Result.legalFirstName, volunteer.Result.legalSecondName),
                        };

                        msgs.AddRange(user.Save(adc));

                        if (user.IsEmpty())
                            amsgs.Add("user empty", MessageType.UserError);

                        if (amsgs.HasNoErrors())
                        {
                            user.AllowTextMessages = true;
                            user.LastModifiedBy = user.ID;
                            user.LastModifiedDate = DateTime.UtcNow;
                            user.CreatedBy = user.ID;
                            amsgs.AddRange(user.Save(adc));

                            account = RegistrationCommon.GetOrCreateVolunteerAccount(user, participationObject ,amsgs, adc);

                            registrant = user.GetMasterVolunteerRegistrant(amsgs, adc);
                            registrant.Phone = volunteer.Result.phoneNumber.IsPhone() ? volunteer.Result.phoneNumber : null;

                            amsgs.AddRange(registrant.Save(adc));
                        }

                        if (amsgs.HasNoErrors())
                        {
                            Registrant submittedRecord = registrant.SubmitRecordToAccount(
                                account,
                                user,
                                msgs,
                                dc);
                            registration = submittedRecord.AddRegistrationWithESignature(
                                participationObject.OwnerAccount.ID.Value,
                                participationObject?.GetAnyRelatedActivityGroupId(),
                                participationObject?.GetAccountActivityGroupId(),
                                null,
                                "Volunteer",
                                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36","::1",
                                user.ID.Value,
                                msgs,
                                dc);
                            
                            VolunteerApprovalService.AddApproval(Convert.ToInt32(user.ID),
                                4,
                                ApprovalStatus.Approved,
                                "Approved",
                                DateTime.UtcNow.AddYears(1), 
                                user,
                                amsgs,adc
                            );
                            
                            VolunteerTaskService.AddTask(user.ID, 
                                TaskStatus.ReviewApplicant, 
                                ApplicationStatus.New, 
                                4 , 
                                DateTime.UtcNow, 
                                amsgs , adc);
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => { msgs.AddRange(smsgs); },
                    onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
                    onException: (SystemMessages emsgs) => { msgs.AddRange(emsgs); },
                    dc: dc
                    );
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}