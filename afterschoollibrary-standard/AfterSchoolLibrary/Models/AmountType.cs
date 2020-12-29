using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public enum AmountType
{
	NotSet = 0,

	[Display(Name = "Percentage")]
	Percentage,

	[Display(Name = "Amount")]
	FlatAmount
}