Feature: AccountSignUp
	Create new Individual Account

@CreateIndividualAccount
Scenario:  Try to create new individual account
	Given  Navigate to Signup Page
	Given  Navigate to signup welcome and click Get started button
	Given  Click individual Account and provide programs for student 
	Given  Poulate Program Name and click continue
	Given  Poulate firstname, lastname, email, password and click continue
	Given  Click create free profile and populate organization info form and click continue
	Given  Populate share info with parent form and click continue
	Given  Populate contact info and click continue
	Given  Populate customize your profile images and click continue
	Given  Populate parents find your activity easily and click continue
	Given  Populate enter links and click continue
	When   Click on go to your profile 
	Then   Match the current url with the expected url