Feature: CreateAndManageSessions
	Create New Session, Session Activity And Manage Session.

@mytag
Scenario: Create New Session, Session Activity And Manage Session
	Given Login to the school account
	Given Navigate to the sessions section and click on add session
	Given Populate new session form and click save
	Given Populate Signup Periods and click save
	Given Populate setting form and click save
	Given Populate restrictions form and click save
	Given Populate schedule form and click save
	Given Populate about activity form and click save
	Given Populate activity contact form and click save
	Given Populate activity location form and click save
	Given Populate price and click save
	Then  Veriy the activity is published