using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolLibrary
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

    public class ActivityAndSeatCount
    {
        public int ActivityId { get; set; }
        public int ReservedSeatsTaken { get; set; }
        public int WaitingListSeatsTaken { get; set; }
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
        public RegistrationPeriod RegistrationPeriod { get; set; }
        public ActivityGroup ActivityGroup { get; set; }

        private List<Registration> Registrations { get; set; }
        private List<RegistrantActivity> RegistrantActivities { get; set; }
        private List<Activity> Activities { get; set; }
        private List<ActivityAndSeatCount> ActivityAndSeatCounts { get; set; }
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
            ActivityAndSeatCounts = new List<ActivityAndSeatCount>();
        }

        public RegistrationRandomization(RegistrationPeriod period)
        {
            RegistrationPeriod = period;
            RoundNumber = 0;
            ActivityAndSeatCounts = new List<ActivityAndSeatCount>();
        }

        public RegistrationRandomization(ActivityGroup group)
        {
            ActivityGroup = group;
            RoundNumber = 0;
            ActivityAndSeatCounts = new List<ActivityAndSeatCount>();
        }

        /// <summary>
        /// Lottery goes through all registrants in rounds. Each registrant gets their top pick per round.
        /// The registrant keeps going in the round until they get a reserved seat for that round
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns>bool</returns>
        private bool RunLottery(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            this.RoundNumber++;

            if (msgs.IsEmpty()) msgs = new SystemMessages();
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
                var registrantActivities = this.RegistrantActivities.Where(x => x.RegistrantId == registrant.ID).ToList();

                //get all registrant activities for this registrant that aren't registered yet
                var registrantActivitiesNotRegisteredYet = registrantActivities.Where(x => !registrantAssignment.Assignments.Contains(x.ID.Value)).ToList();
                var registrantActivitiesAlreadyRegistered = registrantActivities.Where(x => registrantAssignment.Assignments.Contains(x.ID.Value)).ToList();

                //set the highest preference order at the top
                registrantActivitiesNotRegisteredYet = registrantActivitiesNotRegisteredYet.OrderBy(x => x.PreferenceNumber.Value).ToList();

                foreach(var ra in registrantActivitiesNotRegisteredYet)
                {
                    // make sure activity is not full
                    var activity = this.Activities.Find(x => x.ID == ra.ActivityId);
                    var activityAssignment = ActivityAssignments.Find(x => x.ActivityId == ra.ActivityId.Value);
                    if (activityAssignment.IsEmpty())
                    {
                        activityAssignment = new ActivityAssignment() { ActivityId = ra.ActivityId.Value };
                        this.ActivityAssignments.Add(activityAssignment);
                    }

                    var seatsTaken = activityAssignment.Assignments.Count() + ActivityAndSeatCounts.Find(x => x.ActivityId == activity.ID).ReservedSeatsTaken;
                    var wlSeatsTaken = activityAssignment.WaitingListAssignments.Count() + ActivityAndSeatCounts.Find(x => x.ActivityId == activity.ID).WaitingListSeatsTaken;
                    var isFull = seatsTaken >= activity.SeatsAvailable.Value;
                    var isWLFull = wlSeatsTaken >= activity.WaitingListSeats.Value;
                    var activityDays = activity.GetSchedule(dc: dc).GetInstances(dc: dc);
                    var hasActivityDuringScheduleAlready = false;

                    // check to see if the person is already registered for any days/times that would overlap, 
                    // if so, you can't register
                    hasActivityDuringScheduleAlready = DateRange.HasOverlappingDates(
                                                            activityDays.Select(x => new DateRange(x.StartDateTime.Value, x.EndDateTime.Value)), 
                                                            registrantAssignment.AssignmentDateRanges
                                                        );


                    // keep going until you find one that is not full, then exit the loop
                    // if not full and doesn't have conflicting dates
                    if (!isFull && !hasActivityDuringScheduleAlready)
                    {
                        msgs.AddRange(AddAssignment(ra, AssignmentType.Seat, dc: dc));
                        allFull = false;
                        break;
                    }
                    // check waiting list and put the person on it, 
                    // unless they were placed in another activity for that same day with a higher preference number
                    else if (!isWLFull && !hasActivityDuringScheduleAlready)
                    {
                        msgs.AddRange(AddAssignment(ra, AssignmentType.WaitingList, dc: dc));
                    }
                    // puts it in the not assigned list
                    else
                    {
                        msgs.AddRange(AddAssignment(ra, AssignmentType.NotSet, dc: dc));
                    }
                }

                // check to see if all activities have been completed for this registrant
                if (!registrantActivities.Where(x => !registrantAssignment.Assignments.Contains(x.ID.Value)).Any())
                {
                    registrantsToRemove.Add(registrant);
                }
            }

            foreach(var registrant in registrantsToRemove)
            {
                this.Registrants.Remove(registrant);
            }            

            return allFull;
        }

        private void LoadData(DatabaseConnection dc = null)
        {
            if (this.RegistrationPeriod.IsNotEmpty())
            {
                this.Registrations = this.RegistrationPeriod.GetRegistrations(dc: dc);
                this.RegistrantActivities = this.RegistrationPeriod.GetRegistrantActivities(this.Registrations, dc: dc);
                this.Activities = this.RegistrationPeriod.GetActivities(dc);
            }
            else if (this.ActivityGroup.IsNotEmpty())
            {
                this.Registrations = this.ActivityGroup.GetRegistrations(dc: dc);
                this.RegistrantActivities = this.ActivityGroup.GetRegistrantActivities(this.Registrations, dc: dc);
                this.Activities = this.ActivityGroup.GetActivities(false, false, null, null, dc: dc);
            }
            
            foreach(var activity in this.Activities)
            {
                this.ActivityAndSeatCounts.Add(
                    new ActivityAndSeatCount()
                    {
                        ActivityId = activity.ID.Value,
                        ReservedSeatsTaken = activity.GetSeatsTaken(dc),
                        WaitingListSeatsTaken = activity.GetWaitingListSeatsTaken(dc)
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
                this.RegistrantAssignments.Add(assignment);

                var registrations = registrant.GetRegistrations(dc);
                foreach(var registration in registrations)
                {
                    var related = registration.GetCurrentRelatedRegistrations(dc);
                    var existingRas = new List<RegistrantActivity>();
                    foreach(var rel in related)
                    {
                        existingRas.AddRange(rel.GetRegistrantActivities(dc));
                    }

                    // only add date ranges of activities that are reserved
                    foreach(var ra in existingRas.Where(x => x.NumberOfSeats > 0))
                    {
                        assignment.AssignmentDateRanges.AddRange(
                            ra.ToActivity(dc: dc)
                                .GetSchedule(dc: dc)
                                .GetInstances(dc: dc)
                                .Select(x => new DateRange(x.StartDateTime.Value, x.EndDateTime.Value))
                        );
                    }
                }
            }
        }

        private SystemMessages DoAssignments(DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            foreach (var registrantAssignment in this.RegistrantAssignments)
            {
                foreach (var assignment in registrantAssignment.Assignments)
                {
                    var ra = this.RegistrantActivities.Find(x => x.ID.Value == assignment);
                    msgs.AddRange(ra.ChangeSeatAssignment(RegistrationAssignmentType.Reserved, dc: dc));
                }
                foreach (var assignment in registrantAssignment.WaitingListAssignments)
                {
                    var ra = this.RegistrantActivities.Find(x => x.ID.Value == assignment);
                    msgs.AddRange(ra.ChangeSeatAssignment(RegistrationAssignmentType.WaitingList, dc: dc));
                }
            }
            return msgs;
        }

        public SystemMessages ApplyLottery(List<RegistrantAssignment> registrantAssignments, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    _RegistrantAssignments = registrantAssignments;
                    LoadData(adc);
                    amsgs.AddRange(DoAssignments(adc));

                    if(amsgs.HasNoErrors() && this.ActivityGroup.IsNotEmpty())
                    {
                        this.ActivityGroup.HasLotteryRun = true;
                        amsgs.AddRange(this.ActivityGroup.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );            

            return msgs;
        }

        public List<RegistrantAssignment> AttemptLottery(SystemMessages msgs, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            LoadData();
            while (!RunLottery(msgs));

            return this.RegistrantAssignments;
        }

        private SystemMessages AddAssignment(RegistrantActivity ra, AssignmentType type, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

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
            var activity = this.Activities.Find(x => x.ID == ra.ActivityId);
            var dateRanges = activity.GetSchedule(dc: dc).GetInstances(dc: dc).Select(x => new DateRange(x.StartDateTime.Value, x.EndDateTime.Value));
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
            var actassignment = ActivityAssignments.Find(x => x.ActivityId == ra.ActivityId.Value);
            if (actassignment.IsEmpty())
            {
                actassignment = new ActivityAssignment();
                actassignment.ActivityId = ra.ActivityId.Value;
            }

            if (!actassignment.RegistrantActivities.Where(x => x == ra.ActivityId).Any())
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

            return msgs;
        }


    }
}