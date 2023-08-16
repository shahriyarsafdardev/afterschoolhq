using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Helpers;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Enums;
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
            try
            {
                var pageNumber = 1;
                var recordsPerPage = 10;
                
                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                var currentRegistrant = registration.GetRegistrant(msgs);
                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");
                
                if (pageNo.IsNotEmpty())
                    pageNumber = pageNo;
                
                var firstRegistrantId = currentRegistrant.FirstRegistrantId.IsEmpty() ? currentRegistrant.ID : currentRegistrant.FirstRegistrantId;
                var registrantNotes = RegistrantNote.LoadByFirstRegistrantIdByIsDeleted(firstRegistrantId.Value,false,msgs);
                var start = recordsPerPage * (pageNumber - 1);
                var length = recordsPerPage;
                var registrantNotesDto = new RegistrantNotesDto();
                var notesDto = new List<NotesDto>();
                var totalPages = 0; //Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();
                var count = 0;
                
                registrantNotes.ForEach(registrantNote =>
                {
                    var registrantNoteVersion = RegistrantNoteVersions.LoadByNoteIdByIsDeleted(registrantNote.ID.Value, false, msgs)
                        .OrderByDescending(rnv => rnv.CreateDate).FirstOrDefault();
                    
                    if (searchQuery.IsNotEmpty())
                    {
                        // registrantNoteVersion = RegistrantNoteVersions.SearchByNoteIdByIsDeleted(searchQuery, false, registrantNoteVersion.ID, msgs);
                    }

                    if (registrantNoteVersion.IsNotEmpty())
                    {
                        var currentUser = WebUser.Load(registrantNoteVersion.CreatedBy.Value, msgs);

                        notesDto.Add(new NotesDto()
                        {
                            ID = registrantNote.ID.Value,
                            Title = registrantNoteVersion.Title,
                            Text = registrantNoteVersion.Text,
                            CreatedByName = currentUser.DisplayName,
                            PostDate = registrantNote.PostDate,
                        });
                        count += 1;
                    }
                });
                
                registrantNotesDto.RegistrantName = currentRegistrant.Name;
                registrantNotesDto.Notes = notesDto;
                
                dynamic response = new
                {
                    RegistrantNotes = registrantNotesDto,
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
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");

                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                var currentRegistrant = registration.GetRegistrant(msgs);
                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");
                
                if(title.IsEmpty())
                    throw new Exception("Invalid Note Title");
                
                if(text.IsEmpty())
                    throw new Exception("Invalid Note Text");
                
                if(postDate.IsEmpty())
                    throw new Exception("Invalid Note Date");
                
                var firstRegistrantId = currentRegistrant.FirstRegistrantId.IsEmpty() ? currentRegistrant.ID : currentRegistrant.FirstRegistrantId;
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
                                Title = title,
                                Text = text,
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
                {
                    noteDto = new NotesDto
                    {
                        ID = registrantNote.ID.Value,
                        Title = registrantNoteVersion.Title,
                        Text = registrantNoteVersion.Text,
                        CreatedByName = currentUser.DisplayName,
                        PostDate = registrantNote.PostDate,
                    };    
                }
                
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
    }
}