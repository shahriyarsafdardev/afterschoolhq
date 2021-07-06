Feature: CreateActivity
	Create New Activity

@CreateActivity
Scenario: Login And Create New Activity
	Given  Login to Create Activity
	Given  Navigate to the new activity page
	Given  Populate settings section and save
	Given  Populate restrictions section and save
	Given  Populate schedule section and save
	Given  Populate about your activity section and save
	Given  Populate contact section and save
	Given  Populate location section and save
	Given  Populate price and payment section and save
	Given  Populate email content and save
