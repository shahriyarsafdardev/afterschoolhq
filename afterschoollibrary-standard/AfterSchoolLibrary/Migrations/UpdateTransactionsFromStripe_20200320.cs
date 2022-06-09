using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateTransactionsFromStripe_20200320
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    string lastObjectId = null;
                    var charges = new List<Stripe.Charge>();
                    var pageLimit = 50;
                    var untilDate = ("12/01/2019").ToDateTime();
                    var lastDate = DateTime.UtcNow;

                    do
                    {
                        var stripeHelper = new Billing.StripeHelper();
                        charges = stripeHelper.GetCharges(lastObjectId, pageLimit, amsgs);

                        charges.ForEach(charge =>
                        {
                            if (charge.InvoiceId.IsNotEmpty() && charge.Status == "succeeded" && !charge.Refunded && charge.Created >= untilDate)
                            {
                                var transaction = Transaction.LoadByStripeChargeOrRefundId(charge.Id, amsgs, adc);

                                if (transaction.IsEmpty())
                                {
                                    var invoice = stripeHelper.GetInvoice(charge.InvoiceId, amsgs);
                                    var subscription = stripeHelper.GetSubscription(invoice.SubscriptionId, null, amsgs);

                                    if (subscription.IsNotEmpty())
                                    {
                                        var stripePlan = subscription.Plan;
                                        var plan = SubscriptionPlan.Load(new Hashtable() { { "StripePlanId", stripePlan.Id } }, null, null, amsgs, adc).FirstOrDefault();
                                        var rsp = RegistrantSubscriptionPlan.LoadByStripeSubscriptionId(subscription.Id, amsgs, adc);
                                        var registrant = rsp.GetRegistrant(amsgs, adc);
                                        var masterRegistrant = registrant.GetMasterRegistrant(amsgs, adc);
                                        var registration = registrant.GetRegistrations(amsgs, adc).First();
                                        var billingAccountUid = subscription.Metadata["BillingAccountUid"].ToGuid();
                                        var payingAccountUid = subscription.Metadata["PayingAccountUid"].ToGuid();
                                        var activityUid = stripePlan.Metadata["ActivityUid"].ToGuid();

                                        var billingAccount = Account.LoadByUid(billingAccountUid, amsgs, adc);
                                        var payingAccount = Account.LoadByUid(payingAccountUid, amsgs, adc);
                                        var activity = Activity.LoadByUid(activityUid, amsgs, dc);

                                        var paymentType = charge.PaymentMethodDetails.Card.IsNotEmpty() ? PaymentType.CreditCard : PaymentType.ACH;
                                        var convenienceFeeOverrideAmount = plan.GetConvenienceFee(billingAccount, amsgs, adc);
                                        var paymentTypeFeeOverrideAmount = paymentType == PaymentType.CreditCard ? plan.GetCreditCardFee(billingAccount, amsgs, dc) : 0;
                                        var participationObject = activity.GetParticipationObject(amsgs, adc);

                                        var checkoutObject = new CheckoutObject()
                                        {
                                            PayingAccount = payingAccount,
                                            BillingAccount = billingAccount,
                                            OverrideAmount = charge.Amount / 100M,
                                            ConvenienceFeeOverrideAmount = convenienceFeeOverrideAmount,
                                            PaymentTypeFeeOverrideAmount = paymentTypeFeeOverrideAmount,
                                            ParticipationObject = participationObject,
                                            PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                                            PaymentType = paymentType,
                                            StripeChargeId = charge.Id,
                                            SubscriptionPlan = plan.PlanTypeId == SubscriptionPlanType.Subscription ? plan : null,
                                            PaymentPlan = plan.PlanTypeId == SubscriptionPlanType.PaymentPlan ? plan : null,
                                            MasterRegistrant = masterRegistrant,
                                            NumberOfSeats = 1,
                                            Registration = registration,
                                            RegistrantSubscriptionPlan = rsp,
                                            ConvenienceFeePayer = payingAccount,
                                            CreditCardFeePayer = payingAccount,
                                            Date = charge.Created,
                                        };

                                      billingAccount.ProcessSubscriptionPlanPayment(checkoutObject, plan, rsp, amsgs, adc);
                                    }
                                }
                            }

                            lastObjectId = charge.Id;
                            lastDate = charge.Created;
                        });
                    } 
                    while (charges.Count == pageLimit && lastDate >= untilDate);
                },
                uniqueIdenfier: "UpdateTransactionsFromStripe_20200320",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
