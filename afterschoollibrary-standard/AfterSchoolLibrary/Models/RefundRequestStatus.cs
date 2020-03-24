public enum RefundRequestStatus
{
	NotSet = 0,
	RequestedByParent,
	RequestedByProvider,
	ApprovedByProvider,
	ApprovedByAfterSchoolHQ,
	Completed,
	DeniedByProvider,
	DeniedByAfterSchoolHQ,
	ApprovedManualRefund
}