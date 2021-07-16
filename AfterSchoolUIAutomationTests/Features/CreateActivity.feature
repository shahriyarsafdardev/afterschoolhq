Feature: CreateActivity
	Create New Activity

@CreateActivity
Scenario: Login And Create New Activity
	Given  Go through new provider signup flow for activity creation
	Given  Go through create profile flow for activity creation
	Given  Close getting started modal
	Given  Click got it button
	Given  Click on Create First Activity
	Given  Populate settings section and save
	Given  Populate restrictions section and save
	Given  Populate schedule section and save
	Given  Populate about your activity section and save
	Given  Populate contact section and save
	Given  Populate location section and save
	Given  Populate price and payment section and save
	Given  Populate email content and save
	Then   Verify Test
