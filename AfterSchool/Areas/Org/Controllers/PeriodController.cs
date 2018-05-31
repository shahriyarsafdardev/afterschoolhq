using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchool.Models;
using System.Web.Script.Serialization;
using Hangfire;

namespace AfterSchool.Areas.Org.Controllers
{
    public class PeriodController : BaseLoggedInController
    {
        [HttpGet]
        public ActionResult Index(string pn, string rpp)
        {
            var periods = new List<RegistrationPeriod>();
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
            {
                pageNumber = pn.ToInteger();
            }
            if (rpp.IsNumber() && rpp.ToInteger() > 0)
            {
                recordsPerPage = rpp.ToInteger();
            }

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;

            totalCount = SessionVariables.CurrentAccount.GetRegistrationPeriodCount();
            periods = SessionVariables.CurrentAccount.GetRegistrationPeriods(start, length);

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(periods);
        }

        [HttpGet]
        public ActionResult Summary(string id)
        {
            var period = RegistrationPeriod.Load(id.ToInteger());
            var registrations = period.GetRegistrations();
            var actions = SessionAction.LoadByRegistrationPeriodId(
                period.ID.Value,
                new List<SessionActionType>() {
                    SessionActionType.ActivityCreate,
                    SessionActionType.ActivityPublish,
                    SessionActionType.ActivityUnpublish,
                    SessionActionType.ActivityRegistrationCompleted,
                    SessionActionType.ActivityRegistrationUpdated
                },
                0,
                15);
            ViewBag.Messages = new SystemMessages();
            ViewBag.Actions = actions;
            ViewBag.Registrations = registrations;
            return View(period);
        }

        [HttpGet]
        public ActionResult Edit(string id, string qg)
        {
            var period = new RegistrationPeriod();
            if (id.ToInteger() > 0)
            {
                period = RegistrationPeriod.Load(id.ToInteger());
            }
            else
            {
                period.ID = new int?();
                period.RegistrationModeId = RegistrationMode.Register;
                period.StartDateTime = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).Date.AddHours(8);
                period.EndDateTime = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow.AddWeeks(1)).Date.AddHours(16);
            }
            ViewBag.Messages = new SystemMessages();
            return View(period);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(string id, string name, string description, string opendate, string opentime, string closedate, string closetime, string regmode)
        {
            var msgs = new SystemMessages();

            if (name.IsEmpty())
            {
                msgs.Add("Label is required", MessageType.UserError);
            }
            else if (RegistrationPeriod.ExistsByAccountIdByName(SessionVariables.CurrentAccount.ID.Value, name, id.ToInteger()))
            {
                msgs.Add("Label is already in use", MessageType.UserError);
            }
            /*
            if (description.IsEmpty())
            {
                msgs.Add("Description is required", MessageType.UserError);
            }
            */
            if (!string.Format("{0} {1}", opendate, opentime).IsDateTime())
            {
                msgs.Add("Please enter a valid open date and time", MessageType.UserError);
            }
            if (!string.Format("{0} {1}", closedate, closetime).IsDateTime())
            {
                msgs.Add("Please enter a valid close date and time", MessageType.UserError);
            }
            

            RegistrationPeriod period = null;
            if (!id.IsNumber() || id.ToInteger() == 0)
            {
                period = new RegistrationPeriod();
                period.CreateDate = DateTime.UtcNow;
                period.CreatedBy = SessionVariables.LoggedInUser.ID;
            }
            else
            {
                period = RegistrationPeriod.Load(id.ToInteger());
            }

            if (name.IsNotEmpty()) period.Name = name.MaxChars(50);
            if (description.IsNotEmpty()) period.Description = description.MaxChars(400);
            if (opendate.IsNotEmpty() && opentime.IsNotEmpty()) period.StartDateTime = string.Format("{0} {1}", opendate, opentime).ToDateTime();
            if (closedate.IsNotEmpty() && closetime.IsNotEmpty()) period.EndDateTime = string.Format("{0} {1}", closedate, closetime).ToDateTime();
            if (regmode.IsNumber()) period.RegistrationModeId = (RegistrationMode)regmode.ToInteger();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        period.AccountId = SessionVariables.CurrentAccount.ID;
                        period.LastModifiedDate = DateTime.UtcNow;
                        period.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                        amsgs.AddRange(period.Save(adc));

                        var activities = period.GetActivities(adc);
                        foreach (var activity in activities)
                        {
                            activity.RegistrationModeId = period.RegistrationModeId;
                            amsgs.AddRange(activity.Save(adc));
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add(string.Format("{0} Saved", UILabel.RegistrationPeriodLabel()), MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }            

            ViewBag.Messages = msgs;
            return View(period);
        }

        [HttpGet]
        public ActionResult Activities(string id, string pn, string rpp)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;
            var period = RegistrationPeriod.Load(id.ToInteger());

            if (pn.IsNumber() && pn.ToInteger() > 0)
            {
                pageNumber = pn.ToInteger();
            }
            if (rpp.IsNumber() && rpp.ToInteger() > 0)
            {
                recordsPerPage = rpp.ToInteger();
            }

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;    

            var account = SessionVariables.CurrentAccount;

            var activities = period.GetActivities();

            var totalCount = activities.Count;
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            var finalActivities = activities.OrderBy(x => x.Name).ToList().GetRange(start, Math.Min(activities.Count - (recordsPerPage * (pageNumber - 1)), length));

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Activities = finalActivities;
            ViewBag.Messages = new SystemMessages();

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(period);
        }

        [HttpGet]
        public ActionResult Registrations(string id, string pn, string rpp)
        {
            var period = RegistrationPeriod.Load(id.ToInteger());
            var registrations = new List<Registration>();
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
            {
                pageNumber = pn.ToInteger();
            }
            if (rpp.IsNumber() && rpp.ToInteger() > 0)
            {
                recordsPerPage = rpp.ToInteger();
            }

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;

            registrations = period.GetRegistrations(start, length);
            totalCount = period.GetRegistrationCount();

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            
            ViewBag.Messages = new SystemMessages();
            ViewBag.Registrations = registrations;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Registrations = registrations;
            ViewBag.RegistrantActivities = period.GetRegistrantActivities(registrations);
            ViewBag.ReservedSeats = period.GetSeatCountByAssignmentType(RegistrationAssignmentType.Reserved);
            ViewBag.WaitingListSeats = period.GetSeatCountByAssignmentType(RegistrationAssignmentType.WaitingList);
            ViewBag.ApplicationSeats = period.GetSeatCountByAssignmentType(RegistrationAssignmentType.Application);
            ViewBag.ReturnUrl = Url.Action("registrations", "period", new { id = period.ID });

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(period);
        }

        [HttpGet]
        public ActionResult DownloadRegistrations(string id)
        {
            var period = RegistrationPeriod.Load(id.ToInteger());
            var registrations = period.GetRegistrations();
            string csv = Registration.ExportCSV(registrations, SessionVariables.CurrentAccount, RegistrationReportType.Detailed, RegistrationStatus.NotSet, RegistrationAssignmentType.NotSet, null);

            Response.Clear();
            Response.AddHeader("Content-Type", BasicLibrary.Utilities.GetMimeType("csv"));
            Response.AddHeader("Content-Disposition", "attachment; filename=registrations.csv");

            byte[] filebytes = System.Text.UTF8Encoding.UTF8.GetBytes(csv);
            Response.AddHeader("Content-Length", filebytes.Length.ToString());
            Response.BinaryWrite(filebytes);
            Response.End();

            return RedirectToAction("registrations");
        }

        [HttpGet]
        public ActionResult NotifyAll(string id)
        {
            var msgs = new SystemMessages();
            var period = RegistrationPeriod.Load(id.ToInteger());
            var registrations = period.GetRegistrations();
            var firstReg = registrations.First();

            BackgroundJob.Enqueue(() => registrations.ForEach(reg => Registration.SendConfirmation(reg.ID.Value, firstReg.ID == reg.ID, null)));

            return RedirectToAction("registrations");
        }

        [HttpGet]
        public ActionResult Placement(string id, string step)
        {
            var msgs = new SystemMessages();
            var period = RegistrationPeriod.Load(id.ToInteger());
            var nextStep = 1;
            if (step.IsNumber())
            {
                nextStep = step.ToInteger();
            }

            if(nextStep == 2)
            {
                var registrations = period.GetRegistrations();
                var ras = period.GetRegistrantActivities(registrations);
                var registrantAssignments = period.RandomizePlacementAttempt(SessionVariables.CurrentAccount, msgs);

                var assignments = new List<int>();
                var assignmentsWL = new List<int>();
                registrantAssignments.ForEach(x => assignments.AddRange(x.Assignments));
                registrantAssignments.ForEach(x => assignmentsWL.AddRange(x.WaitingListAssignments));

                var assignedRegistrationActivities = ras.Where(x => assignments.Contains(x.ID.Value)).ToList();
                var waitingListActivities = ras.Where(x => assignmentsWL.Contains(x.ID.Value)).ToList();

                ViewBag.RegistrantAssignments = registrantAssignments;
                ViewBag.RegistrantActivities = ras;
                ViewBag.AssignedRegistrantActivities = assignedRegistrationActivities;
                ViewBag.WaitingListActivities = waitingListActivities;
                ViewBag.Activities = period.GetActivities().OrderBy(x => x.Name).ToList();
            }

            ViewBag.Step = nextStep;
            ViewBag.Messages = msgs;
            return View(period);
        }

        [HttpGet]
        public ActionResult ResetPlacement(string id)
        {
            var msgs = new SystemMessages();
            var period = RegistrationPeriod.Load(id.ToInteger());

            if(period.RegistrationModeId == RegistrationMode.Apply)
            {
                msgs.AddRange(period.RandomizePlacementReset());

                if (msgs.HasNoErrors())
                {
                    msgs.Add("All Registration Placement has been reset", MessageType.Success);
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.Step = 1;
            return View("Placement", period);
        }

        [HttpPost]
        public ActionResult Placement(string id, string step, string assignmentsJSON)
        {
            var msgs = new SystemMessages();
            var period = RegistrationPeriod.Load(id.ToInteger());
            var nextStep = 1;
            if (step.IsNumber())
            {
                nextStep = step.ToInteger();
            }

            var registrantAssignments = new JavaScriptSerializer().Deserialize<List<RegistrantAssignment>>(assignmentsJSON);

            msgs.AddRange(period.RandomizePlacementApply(registrantAssignments, SessionVariables.CurrentAccount));

            var registrations = period.GetRegistrations();
            var ras = period.GetRegistrantActivities(registrations);
            var assignments = new List<int>();
            var assignmentsWL = new List<int>();
            registrantAssignments.ForEach(x => assignments.AddRange(x.Assignments));
            registrantAssignments.ForEach(x => assignmentsWL.AddRange(x.WaitingListAssignments));

            var assignedRegistrationActivities = ras.Where(x => assignments.Contains(x.ID.Value)).ToList();
            var waitingListActivities = ras.Where(x => assignmentsWL.Contains(x.ID.Value)).ToList();

            ViewBag.RegistrantAssignments = registrantAssignments;
            ViewBag.RegistrantActivities = ras;
            ViewBag.AssignedRegistrantActivities = assignedRegistrationActivities;
            ViewBag.WaitingListActivities = waitingListActivities;
            ViewBag.Activities = period.GetActivities().OrderBy(x => x.Name).ToList();

            ViewBag.Step = nextStep;
            ViewBag.Messages = msgs;
            return View(period);
        }
    }
}