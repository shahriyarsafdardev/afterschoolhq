Feature: ActivityPayLaterRegistration
	Parent registers a student for pay later activity. 

@mytag
Scenario: Parent registers a student for pay later activity.
	Given Navigate to the Pay later activity registration URL
	Given Click on Register Now
	Given Enter already registered parent email and click Next
	Given Enter password and click Log me in
	Given Click add child button
	Given Fill in the student information form and click save & continue button
	Given Fill in the health information form and click save & continue button
	Given Enter parent e-signature and click e-sign and continue to payment button
	Given Click on request to pay later link
	Then  Verify registration success message