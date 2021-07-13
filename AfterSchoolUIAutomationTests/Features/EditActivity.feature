Feature: EditActivity
	Edit Existing Activity

@EditActivity
Scenario: Login And Edit an Activity
	Given  Login to Edit Activity
	Given  Click on Activities Link
	Given  Click on Manage Activity Button
	Given  Click on Edit Activity Button
	Given  Update settings section and save
	Given  Update restrictions section and save
	Given  Update schedule section and save
	Given  Update about your activity section and save
	Given  Update contact section and save
	Given  Update location section and save
	Given  Update price and payment section and save
	Given  Update email content and save
	Then   Verify Update Test