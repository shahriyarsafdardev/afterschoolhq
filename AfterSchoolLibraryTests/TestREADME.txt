**************Instructions for a newcommer to the Testingcodebase**********

The factories have been implemented to make the creation of objects easier beacuse a lot of the functionality
of one particular object is dependant on others. For instance:

An activity needs and account to start/Host it. It saves the accounts ID so that it has a "owner" it can use
for basic functionality like knowing who started a form through in an activity.

Origionally these dependancies needed to be memorized but now we use Factories which automatically prompt
us to provide the object needed for basic functionality.

If you would like to set aditional fields to the newly created activity above you can simply use the dot trick

			ex: activity.CreateDate = new DateTime (1,1,1) {};

but ALWAYS REMEMBER to save your changes to the database after you have set all of the fields for that object.
(avoid multiple saves beacuse sending this info to the database takes the most time)

			ex: activity.CreateDate = new DateTime (1,1,1) {};
			ex: activity.CreatedBy = "Noah";
			activity.save(dc);


The object oriented structure of the afterschool library allows great expandability:

Now one web user can log in to one account and can manage multiple activities, edit shedules for each create forms
that can be reused in other activities, sign up for activities themself andd any other functionality a normal user would.
this allows one user to have almost all of the functinalities.

all of this functionality relies on dependancies so keep them in mind...

HAPPY TESTING!!!
