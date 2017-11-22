using Microsoft.Extensions.Logging;
using Sitecore.Framework.Conditions;
using Sitecore.Marketing.Automation.Activity;
using Sitecore.Xdb.MarketingAutomation.Core.Activity;
using Sitecore.Xdb.MarketingAutomation.Core.Processing.Plan;

namespace Demo9.Features.MarketingAutomation.Activities
{
    public class PublishInteractionActivity : BaseActivity
    {
        public PublishInteractionActivity(ILogger<PublishInteractionActivity> logger) 
            : base((ILogger<IActivity>)logger)
        {

        }

        public override ActivityResult Invoke(IContactProcessingContext context)
        {
           
            Condition.Requires<IContactProcessingContext>(context, nameof(context)).IsNotNull<IContactProcessingContext>();

            if (context.Interaction == null)
            {
                Logger.LogDebug("context interaction is null, processing as success to the next action");
                return (ActivityResult)new SuccessMove();
            }

            Logger.LogDebug("Debug context interaction id: " + context.Interaction.Id);

            return (ActivityResult)new SuccessMove();
        }

    }
}
