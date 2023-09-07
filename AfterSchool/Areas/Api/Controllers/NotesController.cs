using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Api.Controllers
{
    public class NotesController : BaseController
    {
        [HttpGet]
        public ActionResult GetRegistrantNotes(string registrationUid, int pageNo, string searchQuery)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            var isAdmin = false;
            var pageNumber = 1;
            var recordsPerPage = 3;
            var count = 0;
            try
            {
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");
                
                if (!currentUser.HasAccessTo(currentAccount))
                    throw new Exception("You are not authorized user to access this page.");

                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                var registration = Registration.LoadByUidByAccountId(registrationUid.ToGuid(),currentAccount.ID.Value, msgs);
                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                var currentRegistrant = registration.GetRegistrant(msgs);
                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                if (pageNo.IsNotEmpty())
                    pageNumber = pageNo;

                if (SessionVariables.AccountUserType.In(FormConstants.AdminUserTypes))
                    isAdmin = true;

                var start = recordsPerPage * (pageNumber - 1);
                var length = recordsPerPage;
                var registrantNotes = currentRegistrant.GetNotes(msgs);
                var registrantNotesDto = new RegistrantNotesDto();
                var notesDto = new List<NotesDto>();

                registrantNotes.ForEach(registrantNote =>
                {
                    var registrantNoteVersion = registrantNote.GetLatestVersion(msgs);
                    
                    if (searchQuery.IsNotEmpty() && registrantNoteVersion.IsNotEmpty())
                        registrantNoteVersion = RegistrantNoteVersions.SearchByNoteIdByIsDeleted(searchQuery, false, registrantNoteVersion.ID.Value, msgs);

                    if (registrantNoteVersion.IsNotEmpty())
                    {
                        var registrantNoteWebUser = registrantNote.GetCreator(msgs);
                        notesDto.Add(new NotesDto()
                        {
                            ID = registrantNote.ID.Value,
                            Title = registrantNoteVersion.Title,
                            Text = registrantNoteVersion.Text,
                            CreatedByName = registrantNoteWebUser.DisplayName,
                            PostDate = registrantNote.PostDate.ToShortDateTimeString(),
                            IsEditable = isAdmin || currentUser.ID == registrantNote.CreatedBy,
                        });
                        count += 1;
                    }
                });

                notesDto = notesDto.Skip(start).Take(length).ToList();
                var totalPages = Math.Ceiling(count.ToDouble() / length).ToInteger();
                registrantNotesDto.RegistrantName = currentRegistrant.Name;
                registrantNotesDto.Notes = notesDto;

                dynamic response = new
                {
                    RegistrantNotes = registrantNotesDto,
                    TotalPages = totalPages,
                    PageNo = pageNumber,
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateRegistrantNote(string registrationUid, string title, string text, DateTime postDate)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");
                
                if (!currentUser.HasAccessTo(currentAccount))
                    throw new Exception("You are not authorized user to access this page.");

                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                var currentRegistrant = registration.GetRegistrant(msgs);
                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                if (title.IsEmpty())
                    throw new Exception("Invalid Note Title");
                
                if (title.Trim().Length > 400)
                    throw new Exception("Title should be 1 to 400 characters.");
                
                if (text.IsEmpty())
                    throw new Exception("Invalid Note Text");
                
                if (text.Trim().Length > 4000)
                    throw new Exception("Text should be 1 to 4000 characters.");

                if (postDate.IsEmpty())
                    throw new Exception("Invalid Note Date");

                var firstRegistrantId = currentRegistrant.GetFirstRegistrantId();
                RegistrantNote registrantNote = null;
                RegistrantNoteVersions registrantNoteVersion = null;
                NotesDto noteDto = null;

                Database.ExecuteTransaction
                (
                    (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        registrantNote = new RegistrantNote
                        {
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = currentUser.ID,
                            LastModifiedDate = DateTime.UtcNow,
                            LastModifiedBy = currentUser.ID,
                            PostDate = postDate,
                            FirstRegistrantId = firstRegistrantId.Value,
                            IsPinned = false,
                            IsDeleted = false,
                        };
                        amsgs.AddRange(registrantNote.Save(adc));

                        if (amsgs.HasNoErrors())
                        {
                            registrantNoteVersion = new RegistrantNoteVersions
                            {
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = currentUser.ID,
                                Title = title.Trim(),
                                Text = text.Trim(),
                                RegistrantNoteId = registrantNote.ID.Value,
                                IsDeleted = false,
                            };
                            amsgs.AddRange(registrantNoteVersion.Save(adc));
                        }
                    },
                    (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );

                if (registrantNote.IsNotEmpty() && registrantNoteVersion.IsNotEmpty())
                    noteDto = new NotesDto
                    {
                        ID = registrantNote.ID.Value,
                        Title = registrantNoteVersion.Title,
                        Text = registrantNoteVersion.Text,
                        CreatedByName = currentUser.DisplayName,
                        PostDate = registrantNote.PostDate.ToShortDateTimeString(),
                        IsEditable = true,
                    };

                dynamic response = new
                {
                    Note = noteDto,
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public ActionResult UpdateRegistrantNote(string title, string text, DateTime postDate, int noteId, bool createNewVersion)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                NotesDto noteDto = null;
                RegistrantNoteVersions registrantNoteVersion = null;

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");
                
                if (!currentUser.HasAccessTo(currentAccount))
                    throw new Exception("You are not authorized user to access this page.");
                
                if (title.IsEmpty())
                    throw new Exception("Invalid Note Title");
                
                if (title.Trim().Length > 400)
                    throw new Exception("Title should be 1 to 400 characters.");
                
                if (text.IsEmpty())
                    throw new Exception("Invalid Note Text");
                
                if (text.Trim().Length > 4000)
                    throw new Exception("Text should be 1 to 4000 characters.");

                if (postDate.IsEmpty())
                    throw new Exception("Invalid Note Date");

                var registrantNote = RegistrantNote.LoadByIDByIsDeleted(noteId, false, msgs);

                if (registrantNote.IsNotEmpty())
                    Database.ExecuteTransaction
                    (
                        (SystemMessages amsgs, DatabaseConnection adc) =>
                        { 
                            registrantNote.PostDate = postDate;
                            registrantNote.LastModifiedBy = currentUser.ID;
                            registrantNote.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(registrantNote.Save(adc));
                            
                            if (createNewVersion)
                            {
                                registrantNoteVersion = new RegistrantNoteVersions
                                {
                                    CreateDate = DateTime.UtcNow,
                                    CreatedBy = currentUser.ID,
                                    Title = title.Trim(),
                                    Text = text.Trim(),
                                    RegistrantNoteId = registrantNote.ID.Value,
                                    IsDeleted = false,    
                                };
                                amsgs.AddRange(registrantNoteVersion.Save(adc));
                            }   
                        },
                        (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                    );

                registrantNoteVersion = registrantNote.GetLatestVersion(msgs);

                if(registrantNote.IsNotEmpty() && registrantNoteVersion.IsNotEmpty())
                    noteDto = new NotesDto
                    {
                        ID = registrantNote.ID.Value,
                        Title = registrantNoteVersion.Title,
                        Text = registrantNoteVersion.Text,
                        CreatedByName = currentUser.DisplayName,
                        PostDate = registrantNote.PostDate.ToShortDateTimeString(),
                        IsEditable = true,
                    };

                dynamic response = new
                {
                    Note = noteDto,
                };
                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult DeleteRegistrantNote(int noteId)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");
                
                if (!currentUser.HasAccessTo(currentAccount))
                    throw new Exception("You are not authorized user to access this page.");
                
                var registrantNote = RegistrantNote.LoadByIDByIsDeleted(noteId, false, msgs);
                
                if (registrantNote.IsNotEmpty())
                {
                    registrantNote.IsDeleted = true;
                    registrantNote.LastModifiedBy = currentUser.ID;
                    registrantNote.LastModifiedDate = DateTime.UtcNow;
                    registrantNote.Save(); 
                }
                
                return Json(
                   new ApiResponse(msgs, null),
                   JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}