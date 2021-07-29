Feature: AcceptPayments
	Accept payments for the account

@AcceptPayments
Scenario: Accept payments for the account
	#Given  Go through new provider signup flow for accept payments
	#Given  Go through create profile flow for accept payments
	#Given  Go through create activity flow for accept payments
	#Given  Go through email confirmation flow for accept payments
	Given  Go through login flow for accept payments
	Given  Navigate to the finish setup page
	Given  Click close button of modal
	Given  Click the got it button
	Given  Click more dropdown option button
	Given  Click enable payments button
	Given  Click choose direct deposits button
	Given  Populate get paid by after school hq form details and click next
	Given  Click use test code button
	Given  Populate personal form details and click next
	Given  Populate bussiness details form and click next
	Given  Populate select an account for payouts and click next
	Given  Review your details and click save
	Then   Verify account success message