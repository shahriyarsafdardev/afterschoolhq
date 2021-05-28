Feature: Login
	Verify login functionality

@mytag
Scenario: Try To Login
	Given  Open the browser and navigate to the Invoice App
	Given  Manage the browser window size
	Given  Enter username
	Given  Enter Password
    When   Click on login
    Then   Verify Navigated URL