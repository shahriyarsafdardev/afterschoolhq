Feature: NewParentAccountActivityPayment
	New Parent, Student Account Registration And Payment For Registering Activity

@NewParentAccountActivityPayment
Scenario: New Parent, Student Account Registration And Payment For Registering Activity
	Given Navigate to the session publish url for registration and click register now
	Given Enter parent email address for registration and click next
	Given Populate fields in step 1 parent signup form details for registration and click save & continue
	Given Populate fields in step 2 home address form details for registration and click save & continue
	Given Populate fields in step 3 additional parent, guardian form details for registration and click save & continue
	Given Populate fields in step 4 emergency contacts form details for registration and click save & continue
	Given Populate fields in step 5 student information form details for registration and click save & continue
	Given Populate fields in step 6 health information form details for registration and click save & continue
	Given Enter parent e-signature for registration and click e-sign and continue to payment button
	Given Enter credit card info for registration and click PayViaCredit button
	Then  Verify thank you message for NewParentAccountActivityPayment