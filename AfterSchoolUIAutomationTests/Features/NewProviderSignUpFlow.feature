Feature: NewProviderSignUpFlow
	New Provider Sign Up Flow

@NewProviderSignUpFlow
Scenario: New Provider Sign Up Flow
	Given  Navigate to Signup Page
	Given  Navigate to signup welcome and click Get started button
	Given  Click create individual account button
	Given  Click provide programs for student button
	Given  Enter Program Name and click continue
	Given  Enter firstname, lastname, email, password and click continue
	Then   Match window current url with the expected url