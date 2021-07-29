Feature: EmailConfirmation
	Email Confirmation For Account Registartion

@mytag
Scenario: Email Confirmation For Account Registartion
	Given  Go through new provider signup flow for email confirmation
	Given  Go through create profile flow for email confirmation
	Given  Go through create activity flow for email confirmation
	Given  Close the email confirmation modal by clicking close button
	Given  Open yopmail and populate email input box
	Given  Click yopmail text input button
	Given  Click confirmation email and link
	Then   Verify the email verification message