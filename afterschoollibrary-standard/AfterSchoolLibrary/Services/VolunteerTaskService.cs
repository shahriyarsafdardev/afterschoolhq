using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AfterSchool.Models;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Stripe;
using VolunteerTask = AfterSchoolLibrary.Models.VolunteerTask;

namespace AfterSchoolLibrary.Services
{
    public static class VolunteerTaskService
    {
        public static VolunteerTask CreateNewTask(int? WebUserId, TaskStatus task, ApplicationStatus applicationStatus, int? fiscalAccountId ,DateTime? dateCompleted , SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var volunteerTask = new VolunteerTask();

            volunteerTask.WebUserId = WebUserId;
            volunteerTask.Task = task;
            volunteerTask.ApplicationStatus = applicationStatus;
            volunteerTask.DateSubmitted = DateTime.UtcNow;
            volunteerTask.DateCompleted = dateCompleted;
            volunteerTask.AccountId = fiscalAccountId;
            volunteerTask.CreatedBy = WebUserId;
            volunteerTask.CreateDate = DateTime.UtcNow;

            msgs.AddRange(volunteerTask.Save(dc: dc));
            return volunteerTask;
        }
    }
}