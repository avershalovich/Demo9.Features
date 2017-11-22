using Demo9.Features.Services;
using Microsoft.Extensions.Logging;
using Sitecore.Marketing.Automation.Activity;
using Sitecore.Marketing.Automation.Models;
using Sitecore.XConnect.Collection.Model;
using Sitecore.Xdb.MarketingAutomation.Core.Activity;
using Sitecore.Xdb.MarketingAutomation.Core.Processing.Plan;
using System;

namespace Demo9.Features.MarketingAutomation.Activities
{
    public class SendPromoEmailActivity: IActivity
    {
        public IActivityServices Services
        {
            get; set;
        }

        public ActivityResult Invoke(IContactProcessingContext context)
        {
            //get email facet from context contact
            EmailAddressList facet = context.Contact.GetFacet<EmailAddressList>();

            //exiting activity with failure
            if (facet == null || facet.PreferredEmail == null)
                return (ActivityResult)new Failure(Resources.TheEmailAddressListFacetHasNotBeenSetSuccessfully);

            string email = facet.PreferredEmail.SmtpAddress;

            //instantiating email service without DI for simplicity
            var emailService = new EmailService();

            if (!emailService.SendPromoEmail(email))
                return (ActivityResult)new Failure("Failed to send promo email");

            return (ActivityResult)new SuccessMove();
        }
    }
}
