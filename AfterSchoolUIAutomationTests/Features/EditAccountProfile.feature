Feature: EditAccountProfile
	Update Account Profile Data

@EditAccountProfile
Scenario: Login And Edit Profile
	Given  Login to the website
	Given  Navigate to the profile edit page
	Given  Enter user first name
	Given  Enter user last name
	Given  Enter user email
	Given  Enter user password
	Given  Enter user confirm password
    When   Click on Update
    Then   Verify Success Message