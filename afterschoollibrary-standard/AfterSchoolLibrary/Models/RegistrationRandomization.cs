using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Models
{    
    [Serializable]
    public class RegistrantAssignment
    {
        public int RegistrantId { get; set; }
        public List<int> RegistrantActivities { get; set; }
        public List<int> Assignments { get; set; }
        public List<int> WaitingListAssignments { get; set; }
        public List<int> NoAssignments { get; set; }
        public List<DateRange> AssignmentDateRanges { get; set; }
        public List<DateRange> WLAssignmentDateRanges { get; set; }
        public List<DateRange> NSAssignmentDateRanges { get; set; }

        public RegistrantAssignment()
        {
            Assignments = new List<int>();
            WaitingListAssignments = new List<int>();
            RegistrantActivities = new List<int>();
            NoAssignments = new List<int>();
            AssignmentDateRanges = new List<DateRange>();
            WLAssignmentDateRanges = new List<DateRange>();
            NSAssignmentDateRanges = new List<DateRange>();
        }
    }

    [Serializable]
    public class ActivityAssignment
    {
        public int ActivityId { get; set; }
        public List<int> RegistrantActivities { get; set; }
        public List<int> Assignments { get; set; }
        public List<int> WaitingListAssignments { get; set; }

        public ActivityAssignment()
        {
            Assignments = new List<int>();
            WaitingListAssignments = new List<int>();
            RegistrantActivities = new List<int>();
        }
    }

    public class ActivityDetail
    {
        public int ActivityId { get; set; }
        public int ReservedSeatsTaken { get; set; }
        public List<DateRange> Dates { get; set; }
    }

    [Serializable]
    public enum AssignmentType
    {
        NotSet = 0,
        Seat,
        WaitingList
    }

    public class RegistrationRandomization
    {
        public ActivityGroup ActivityGroup { get; set; }
        public WebUser LoggedInUser { get; set; }

        private List<Registration> Registrations { get; set; }
        private List<RegistrantActivity> RegistrantActivities { get; set; }
        private List<Activity> Activities { get; set; }
        private List<ActivityDetail> ActivityDetails { get; set; }
        private List<Registrant> Registrants { get; set; }
        private int RoundNumber { get; set; }

        private List<RegistrantAssignment> _RegistrantAssignments;
        public List<RegistrantAssignment> RegistrantAssignments
        {
            get
            {
                if (_RegistrantAssignments.IsEmpty())
                {
                    _RegistrantAssignments = new List<RegistrantAssignment>();
                }
                return _RegistrantAssignments;
            }
        }

        private List<ActivityAssignment> _ActivityAssignments;
        public List<ActivityAssignment> ActivityAssignments
        {
            get
            {
                if (_ActivityAssignments.IsEmpty())
                {
                    _ActivityAssignments = new List<ActivityAssignment>();
                }
                return _ActivityAssignments;
            }
        }

        public RegistrationRandomization()
        {
            RoundNumber = 0;
            ActivityDetails = new List<ActivityDetail>();
        }

        public RegistrationRandomization(ActivityGroup group)
        {
            ActivityGroup = group;
            RoundNumber = 0;
            ActivityDetails = new List<ActivityDetail>();
        }

        /// <summary>
        /// Lottery goes through all registrants in rounds. Each registrant gets their top pick per round.
        /// The registrant keeps going in the round until they get a reserved seat for that round
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns>bool</returns>
        private bool RunLottery()
        {
            this.RoundNumber++;

            var registrants = this.Registrants.Shuffle().ToList();
            var registrantsToRemove = new List<Registrant>();
            var allFull = true;

            foreach(var registrant in registrants)
            {
                //get days to not consider when assigning
                var registrantAssignment = RegistrantAssignments.Find(x => x.RegistrantId == registrant.ID.Value);
                if (registrantAssignment.IsEmpty())
                {
                    registrantAssignment = new RegistrantAssignment() { RegistrantId = registrant.ID.Value };
                    this.RegistrantAssignments.Add(registrantAssignment);
                }

               //get all registrant activities for this registrant
                var registrantActivities = this.RegistrantActivities.Where(ra => ra.RegistrantId == registrant.ID).ToList();

                //get all registrant activities for this registrant that aren't registered yet
                var registrantActivitiesNotRegisteredYet = registrantActivities
                                                            .Where(ra => ra.ID.Value.NotIn(registrantAssignment.Assignments))
                                                            .ToList();
                var registrantActivitiesAlreadyRegistered = registrantActivities
                                                            .Where(ra => ra.ID.Value.In(registrantAssignment.Assignments))
                                                            .ToList();

                //set the highest preference order at the top
                registrantActivitiesNotRegisteredYet = registrantActivitiesNotRegisteredYet.OrderBy(x => x.PreferenceNumber.Value).ToList();

                foreach(var ra in registrantActivitiesNotRegisteredYet)
                {
                    // make sure activity is not full
                    var activity = this.Activities.Find(x => x.ID == ra.ParticipationObjectId && 
                                                            ra.ParticipationObjectTypeId == ParticipationObjectType.Activity);
                    var activityAssignment = ActivityAssignments.Find(x => x.ActivityId == ra.ParticipationObjectId.Value && 
                                                                    ra.ParticipationObjectTypeId == ParticipationObjectType.Activity);

                    if (activityAssignment.IsEmpty())
                    {
                        activityAssignment = new ActivityAssignment() { ActivityId = ra.ParticipationObjectId.Value };
                        this.ActivityAssignments.Add(activityAssignment);
                    }

                    var seatsTaken = activityAssignment.Assignments.Count() + ActivityDetails.Find(x => x.ActivityId == activity.ID).ReservedSeatsTaken;
                    var isFull = seatsTaken >= activity.SeatsAvailable.Value;
                    var activityDates = this.ActivityDetails.Find(x => x.ActivityId == activity.ID).Dates;
                    var hasActivityDuringScheduleAlready = false;

                    // check to see if the person is already registered for any days/times that would overlap, 
                    // if so, you can't register
                    hasActivityDuringScheduleAlready = DateRange.HasOverlappingDates(
                                                            activityDates, 
                                                            registrantAssignment.AssignmentDateRanges
                                                        );


                    // keep going until you find one that is not full, then exit the loop
                    // if not full and doesn't have conflicting dates
                    if (!isFull && !hasActivityDuringScheduleAlready)
                    {
                        AddAssignment(ra, AssignmentType.Seat);
                        allFull = false;
                        break;
                    }
                    // check waiting list and put the person on it, 
                    // unless they were placed in another activity for that same day with a higher preference number
                    else if (!hasActivityDuringScheduleAlready)
                    {
                        AddAssignment(ra, AssignmentType.WaitingList);
                    }
                    // puts it in the not assigned list
                    else
                    {
                        AddAssignment(ra, AssignmentType.NotSet);
                    }
                }

                // check to see if all activities have been completed for this registrant
                if (!registrantActivities.Where(x => x.ID.Value.NotIn(registrantAssignment.Assignments)).Any())
                    registrantsToRemove.Add(registrant);
            }

            registrantsToRemove.ForEach(registrant => this.Registrants.Remove(registrant));

            return allFull;
        }

        private void LoadData(SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (this.ActivityGroup.IsNotEmpty())
            {
                this.Registrations = this.ActivityGroup.GetRegistrations(msgs: msgs, dc: dc);
                this.RegistrantActivities = this.ActivityGroup.GetRegistrantActivities(this.Registrations, msgs, dc: dc);
                this.Activities = this.ActivityGroup.GetActivities(null, false, false, null, null, false, msgs, dc);
            }
            
            foreach(var activity in this.Activities)
            {
                this.ActivityDetails.Add(
                    new ActivityDetail()
                    {
                        ActivityId = activity.ID.Value,
                        ReservedSeatsTaken = activity.GetSeatsTaken(msgs, dc),
                        Dates = activity.GetSchedule(msgs, dc).GetInstances(msgs: msgs, dc: dc).Select(x => new DateRange(x.StartDateTime.Value, x.EndDateTime.Value)).ToList()
            }
                );
            }

            this.Registrants = this.RegistrantActivities.Select(x => x.ToRegistrant()).DistinctBy(x => x.ID).ToList();

            // get registrants current other registations 
            // in order to make sure they can't be registered for the same dates
            foreach(var registrant in Registrants)
            {
                var assignment = new RegistrantAssignment();
                assignment.RegistrantId = registrant.ID.Value;

                var registrations = registrant.GetRegistrations(msgs, dc);
                foreach(var registration in registrations)
                {
                    var related = registration.GetCurrentRelatedRegistrations(msgs, dc);
                    var existingRas = new List<RegistrantActivity>();
                    related.ForEach(rel => 
                        existingRas.AddRange(rel.GetRegistrantActivities(msgs, dc).Where(ra => ra.ParticipationObjectTypeId == ParticipationObjectType.Activity))
                    );

                    // only add date ranges of activities that are reserved
                    foreach(var ra in existingRas.Where(x => x.AssignmentTypeId.In(Registration.ReservedStatuses)))
                    {
                        assignment.AssignmentDateRanges.AddRange(
                            ra.GetActivity(msgs, dc)
                                .GetSchedule(msgs, dc)
                                .GetInstances(false, msgs, dc)
                                .Select(x => new DateRange(x.StartDateTime.Value, x.EndDateTime.Value))
                        );
                    }
                }

                this.RegistrantAssignments.Add(assignment);
            }
        }

        private void DoAssignments(SystemMessages msgs, DatabaseConnection dc)
        {            
            foreach (var registrantAssignment in this.RegistrantAssignments)
            {
                foreach (var assignment in registrantAssignment.Assignments)
                {
                    var ra = this.RegistrantActivities.Find(x => x.ID.Value == assignment);
                    ra.ChangeSeatAssignment(RegistrationAssignmentType.Reserved, this.LoggedInUser, msgs, dc);
                }
                foreach (var assignment in registrantAssignment.WaitingListAssignments)
                {
                    var ra = this.RegistrantActivities.Find(x => x.ID.Value == assignment);
                    ra.ChangeSeatAssignment(RegistrationAssignmentType.WaitingList, this.LoggedInUser, msgs, dc);
                }
            }
        }

        public SystemMessages ApplyLottery(List<RegistrantAssignment> registrantAssignments, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    _RegistrantAssignments = registrantAssignments;
                    LoadData(amsgs, adc);
                    DoAssignments(amsgs, adc);

                    if(amsgs.HasNoErrors() && this.ActivityGroup.IsNotEmpty())
                    {
                        this.ActivityGroup.HasLotteryRun = true;
                        amsgs.AddRange(this.ActivityGroup.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );            

            return msgs;
        }

        public List<RegistrantAssignment> AttemptLottery(SystemMessages msgs, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            LoadData(msgs, dc);
            while (!RunLottery());

            return this.RegistrantAssignments;
        }

        private void AddAssignment(RegistrantActivity ra, AssignmentType type)
        {
            // registrant view
            var assignment = RegistrantAssignments.Find(x => x.RegistrantId == ra.RegistrantId.Value);
            if (assignment.IsEmpty())
            {
                assignment = new RegistrantAssignment();
                assignment.RegistrantId = ra.RegistrantId.Value;
            }

            // don't re-add the same registrant activity into this assignment if it is already there
            if (!assignment.RegistrantActivities.Where(x => x == ra.ID.Value).Any())
            {
                assignment.RegistrantActivities.Add(ra.ID.Value);

                switch (type)
                {
                    case AssignmentType.Seat:
                        assignment.Assignments.Add(ra.ID.Value);
                        break;
                    case AssignmentType.WaitingList:
                        assignment.WaitingListAssignments.Add(ra.ID.Value);
                        break;
                    case AssignmentType.NotSet:
                        assignment.NoAssignments.Add(ra.ID.Value);
                        break;
                }
            }

            // store all dates/times this registrant is enrolled for
            var activity = this.Activities.Find(x => x.ID == ra.ParticipationObjectId && ra.ParticipationObjectTypeId == ParticipationObjectType.Activity);
            var dateRanges = this.ActivityDetails.Find(x => x.ActivityId == activity.ID).Dates;
            switch (type)
            {
                case AssignmentType.Seat:
                    assignment.AssignmentDateRanges.AddRange(dateRanges);
                    break;
                case AssignmentType.WaitingList:
                    assignment.WLAssignmentDateRanges.AddRange(dateRanges);
                    break;
                case AssignmentType.NotSet:
                    assignment.NSAssignmentDateRanges.AddRange(dateRanges);
                    break;
            }

            // activity view
            var actassignment = ActivityAssignments.Find(x => x.ActivityId == ra.ParticipationObjectId && ra.ParticipationObjectTypeId == ParticipationObjectType.Activity);
            if (actassignment.IsEmpty())
            {
                actassignment = new ActivityAssignment();
                actassignment.ActivityId = ra.ParticipationObjectId.Value;
            }

            if (!actassignment.RegistrantActivities.Where(x => x == ra.ParticipationObjectId && ra.ParticipationObjectTypeId == ParticipationObjectType.Activity).Any())
            {
                actassignment.RegistrantActivities.Add(ra.ID.Value);

                switch (type)
                {
                    case AssignmentType.Seat:
                        actassignment.Assignments.Add(ra.ID.Value);
                        break;
                    case AssignmentType.WaitingList:
                        actassignment.WaitingListAssignments.Add(ra.ID.Value);
                        break;
                }
            }
        }

    }
}