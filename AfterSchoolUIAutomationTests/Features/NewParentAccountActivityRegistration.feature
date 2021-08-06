Feature: NewParentAccountActivityRegistration
	New Parent Account And Activity Registration

@mytag
Scenario: New Parent Account And Activity Registration
	Given Navigate to the session publish url and click register now
	Given Enter parent email address and click next
	Given Populate fields in step 1 parent signup form details and click save & continue
	Given Populate fields in step 2 home address form details and click save & continue
	Given Populate fields in step 3 additional parent, guardian form details and click save & continue
	Given Populate fields in step 4 emergency contacts form details and click save & continue
	Given Populate fields in step 5 student information form details and click save & continue
	Given Populate fields in step 6 health information form details and click save & continue
	Given Enter parent e signature and click complete registration