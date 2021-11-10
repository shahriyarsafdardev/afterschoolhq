Feature: CreateProfile
	Create Profile

@CreateProfile
Scenario: Create Account Profile
	Given Open the browser and navigate to AfterSchool home page
	Given Go through new provider signup flow
	Given Click create your free profile button
	Given Populate about your organization fields and click continue
	Given Populate share your organization info fields with parents and click continue
	Given Populate your contact info fields and click continue
	Given Populate customize your profile images fields and click continue
	Given Populate finding your activities fields and click continue
	Given Populate enter links fields and click continue
	When  Click go to your profile button
	Then  Match current window url with the expected url