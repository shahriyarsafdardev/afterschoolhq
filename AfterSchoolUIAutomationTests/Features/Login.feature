Feature: Login
	Verify login functionality

@Login
Scenario: Try To Login
	Given  Open the browser and navigate to the Login Page
	Given  Manage the browser window size
	Given  Enter username
	Given  Enter Password
    When   Click on login
    Then   Verify Navigated URL