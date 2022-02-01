using System.ComponentModel;

public enum AmountType
{
	NotSet = 0,

	[Description("Percentage")]
	Percentage,

	[Description("Amount")]
	FlatAmount
}