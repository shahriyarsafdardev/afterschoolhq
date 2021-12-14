Feature: TransactionMatchesPriceOnPaymentScreen
	Match Activity Price On Payment Screen Transaction

@TransactionMatchesPriceOnPaymentScreen
Scenario: Match Activity Price On Payment Screen Transaction
	Given Navigate to the session publish url, read activity cost and click register now
	Given Go through parent login and activity registration steps
	Given Enter Parent E Signature And Continue
	Then  Verify price on payment page with activity price