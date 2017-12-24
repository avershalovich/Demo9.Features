using Sitecore.XConnect.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ListManagement.XConnect;
using Sitecore.XConnect;
using Sitecore.XConnect.Schema;
using Sitecore.Xdb.Common.Web;
using Sitecore.XConnect.Client.WebApi;
using System.Configuration;
using Sitecore.XConnect.Collection.Model;
using Sitecore.Xdb.MarketingAutomation.ReportingClient;
using Sitecore.Xdb.MarketingAutomation.Core.Reporting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Sinks.SystemConsole;
using Sitecore.Xdb.MarketingAutomation.OperationsClient;
using Sitecore.Xdb.MarketingAutomation.Core.Requests;
using Sitecore.Xdb.MarketingAutomation.Core.Results;

namespace Demo9.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = "email";
            string identifier = "avh@brimit.com";

            //AddInteractionTest(source, identifier);

            AddContactToPlan();
            //GetPlanStatistics();
            //GetPlanReport();

            //GetContactPlans();

            System.Console.Read();
        }

        private static void AddInteractionTest(string source, string identifier)
        {
            using (XConnectClient client = GetClient())
            {
                try
                {
                    // Get contact
                    IdentifiedContactReference reference = new IdentifiedContactReference(source, identifier);

                    Contact existingContact = client.Get<Contact>(
                        reference,
                        new ContactExpandOptions(PersonalInformation.DefaultFacetKey));

                    if (existingContact == null)
                    {

                        return;
                    }

                    //Creating even to add to interaction for testing purposes
                    var interactionEvent = new Event(Settings.OnlineEventId, DateTime.UtcNow);
                    interactionEvent.EngagementValue = 0;
                    interactionEvent.Text = "Not available";

                    Interaction interaction = new Interaction(existingContact, InteractionInitiator.Contact,
                        Settings.OnlineChannelId, "console demo test");

                    interaction.Events.Add(interactionEvent);

                    client.AddInteraction(interaction);

                    client.Submit();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);                
                    System.Console.WriteLine(ex.StackTrace);
                }
            }
        }

        private static void AddContactTest(string source, string identifier)
        {
            using (XConnectClient client = GetClient())
            {
                // Get a known contact
                IdentifiedContactReference reference = new IdentifiedContactReference(source, identifier);

                Contact existingContact = client.Get<Contact>(
                    reference,
                    new ContactExpandOptions(PersonalInformation.DefaultFacetKey));

                if (existingContact != null) { return; }

                var contactIdentifier = new[] { new ContactIdentifier(source, identifier, ContactIdentifierType.Known) };

                Contact knownContact = new Contact(contactIdentifier);
                PersonalInformation personalInformation = new PersonalInformation();

                personalInformation.FirstName = "Test Firstname " + DateTime.Now.Ticks;
                personalInformation.LastName = "Test Lastname " + DateTime.Now.Ticks;
                personalInformation.Gender = "male";

                client.SetFacet<PersonalInformation>(knownContact, PersonalInformation.DefaultFacetKey, personalInformation);

                client.AddContact(knownContact);

                client.Submit();

                var operations = client.LastBatch;

                // Loop through operations and check status
                foreach (var operation in operations)
                {
                    System.Console.WriteLine(operation.OperationType + operation.Target.GetType().ToString() + " Operation: " + operation.Status);
                }
            }
        }

        private static void AddIdentifierTest()
        {
            Guid contactId = Guid.Parse("BEC76A9E-F958-0000-0000-0520EB67E0F0");

            using (XConnectClient client = GetClient())
            {
                Contact existingContact = client.Get<Contact>(new ContactReference(contactId),
                    new ContactExpandOptions(new string[] { }));

                if (existingContact != null)
                {
                    var emailIdentifier = new ContactIdentifier("email", "avh@brimit.com", ContactIdentifierType.Known);

                    client.AddContactIdentifier(existingContact, emailIdentifier);
                    client.Submit();
                }
            }
        }

        private static void AddToListTest()
        {
            Guid listId = Guid.Parse("01b28284-4bff-456f-eb1a-f1e28db6edf6");
            Guid contactId = Guid.Parse("BEC76A9E-F958-0000-0000-0520EB67E0F0");

            using (XConnectClient client = GetClient())
            {
                Contact contact = client.Get<Contact>(new ContactReference(contactId),
                    (ExpandOptions)new ContactExpandOptions(new string[1] { "ListSubscriptions" })
                );

                contact.SetListSubscriptionsFacet(client, listId);

                client.Submit();
            }
        }

        private static void AddContactToPlan()
        {
            var operationsClient = GetAutomationOperationsClient();

            var contactXconnectId = Guid.Parse("{bec76a9e-f958-0000-0000-0520eb67e0f0}");
            var planId = Guid.Parse("{4a85ddac-cc2f-47b1-a9d5-8972b3409b24}");
            var request = new EnrollmentRequest(contactXconnectId, planId); // Contact ID, Plan ID

            request.Priority = 1; // Optional
            request.ActivityId = Guid.Parse("{c01b8533-f524-b384-5614-346f0b6a7544}"); // Optional
            request.CustomValues.Add("test", "test"); // Optional

            BatchEnrollmentRequestResult result = operationsClient.EnrollInPlanDirect(new[] { request });
        }

        private static void GetPlanStatistics()
        {
            var reportingClient = GetAutomationReportingClient();

            var planId = Guid.Parse("{4a85ddac-cc2f-47b1-a9d5-8972b3409b24}");

            var result = reportingClient.GetPlanStatistics(new[] {planId });
            foreach (PlanStatistics stat in result)
            {
                Guid planID = stat.PlanDefinitionId; // The plan ID
                long allEnrollmentsCount = stat.AllEnrollmentCount; // All enrollments in the history of this plan
                long currentEnrollmentsCount = stat.CurrentEnrollmentCount; // Current enrollments
            }
        }

        private static void GetPlanReport()
        {
            var reportingClient = GetAutomationReportingClient();

            var planId = Guid.Parse("{4a85ddac-cc2f-47b1-a9d5-8972b3409b24}");
            var reportStart = DateTime.UtcNow.AddDays(-1);
            var reportEnd = DateTime.UtcNow;

            var result = reportingClient.GetPlanReport(planId, reportStart, reportEnd);

            var allContactsByActivity = result.ActivityAllContactCount; // The number of contacts that have ever been enrolled by each activity. The dictionary key is the ID of the activity and the value if the enrollment count.
            var allCurrentContactsByActivity = result.ActivityCurrentContactCount; // The number of contacts enrolled by each activity within the start and end dates. The dictionary key is the ID of the activity and the value if the enrollment count.
            var allContacts = result.PlanAllContactCount; // The number of contacts that have ever been enrolled in any activity of the plan. This is basically a sum of all of the counts from the ActivityAllContactCount dictionary.
            var currentContactCount = result.PlanCurrentContactCount; // The number of contacts enrolled in any activity of the plan within the start and end dates. This is basically a sum of all of the counts from the ActivityCurrentContactCount dictionary.
        }

        private static void GetContactPlans()
        {
            using (XConnectClient client = GetClient())
            {
                var contactXconnectId = Guid.Parse("{7861dfd4-b578-0000-0000-052854642e00}");
                IEntityReference<Contact> contactReference = new ContactReference(contactXconnectId);

                try
                {
                    // Get contact with AutomationPlanEnrollmentCache facet
                    Contact existingContact = client.Get<Contact>(
                        contactReference,
                        new ContactExpandOptions(AutomationPlanEnrollmentCache.DefaultFacetKey));

                    if (existingContact == null) return; 

                    AutomationPlanEnrollmentCache enrollmentCache = existingContact.GetFacet<AutomationPlanEnrollmentCache>();

                    foreach (var enrollment in enrollmentCache.ActivityEnrollments)
                    {
                        Guid activityID = enrollment.ActivityId;
                        System.Console.WriteLine("activityID:" + activityID.ToString());

                        Guid planID = enrollment.AutomationPlanDefinitionId;
                        System.Console.WriteLine("planID:" + planID.ToString());

                        DateTime activityEntryDate = enrollment.ActivityEntryDate;
                        System.Console.WriteLine("activityEntryDate:" + activityEntryDate.ToString());

                        string contextKey = enrollment.ContextKey;
                        System.Console.WriteLine("contextKey:" + contextKey);

                        Dictionary<string, string> customData = enrollment.CustomValues;
                    }
                   
                }
                catch (Exception ex)
                {
                    System.Console.Write(ex.Message);
                }
            }
        }
        private static XConnectClient GetClient()
        {
            string XconnectUrl = ConfigurationManager.AppSettings["xconnect.url"];
            string XconnectThumbprint = ConfigurationManager.AppSettings["xconnect.thumbprint"];

            System.Console.WriteLine("XconnectThumbprint:" + XconnectThumbprint);
            System.Console.WriteLine("XconnectUrl:" + XconnectUrl);


            XConnectClientConfiguration cfg;
            if (string.IsNullOrEmpty(XconnectThumbprint))
            {
                cfg = new XConnectClientConfiguration(
                    new XdbRuntimeModel(Sitecore.XConnect.Collection.Model.CollectionModel.Model),
                    new Uri(XconnectUrl),
                    new Uri(XconnectUrl));

            }
            else
            {
                CertificateWebRequestHandlerModifierOptions options =
                CertificateWebRequestHandlerModifierOptions.Parse("StoreName=My;StoreLocation=LocalMachine;FindType=FindByThumbprint;FindValue=" + XconnectThumbprint);
                var certificateModifier = new CertificateWebRequestHandlerModifier(options);

                // Step 2 - Client Configuration

                var collectionClient = new CollectionWebApiClient(new Uri(XconnectUrl + "/odata"), null, new[] { certificateModifier });
                var searchClient = new SearchWebApiClient(new Uri(XconnectUrl + "/odata"), null, new[] { certificateModifier });
                var configurationClient = new ConfigurationWebApiClient(new Uri(XconnectUrl + "/configuration"), null, new[] { certificateModifier });


                cfg = new XConnectClientConfiguration(
                new XdbRuntimeModel(Sitecore.XConnect.Collection.Model.CollectionModel.Model), collectionClient, searchClient, configurationClient);
            }

            cfg.Initialize();

            var client = new XConnectClient(cfg);

            return client;
        }

        private static AutomationReportingClient GetAutomationReportingClient()
        {
            string XconnectUrl = ConfigurationManager.AppSettings["xconnect.url"];
            string XconnectThumbprint = ConfigurationManager.AppSettings["xconnect.thumbprint"];

            System.Console.WriteLine("XconnectThumbprint:" + XconnectThumbprint);
            System.Console.WriteLine("XconnectUrl:" + XconnectUrl);

            CertificateWebRequestHandlerModifierOptions options =
            CertificateWebRequestHandlerModifierOptions.Parse("StoreName=My;StoreLocation=LocalMachine;FindType=FindByThumbprint;FindValue=" + XconnectThumbprint);
            var certificateModifier = new CertificateWebRequestHandlerModifier(options);

            ILogger<AutomationReportingClient> logger =  LoggerFactory.CreateLogger<AutomationReportingClient>();

            var result = new AutomationReportingClient(new Uri(XconnectUrl), null, null, logger); //new[] { certificateModifier }

            return result;
        }

        private static AutomationOperationsClient GetAutomationOperationsClient()
        {
            string XconnectUrl = ConfigurationManager.AppSettings["xconnect.url"];
            string XconnectThumbprint = ConfigurationManager.AppSettings["xconnect.thumbprint"];

            System.Console.WriteLine("XconnectThumbprint:" + XconnectThumbprint);
            System.Console.WriteLine("XconnectUrl:" + XconnectUrl);

            CertificateWebRequestHandlerModifierOptions options =
            CertificateWebRequestHandlerModifierOptions.Parse("StoreName=My;StoreLocation=LocalMachine;FindType=FindByThumbprint;FindValue=" + XconnectThumbprint);
            var certificateModifier = new CertificateWebRequestHandlerModifier(options);

            ILogger<AutomationOperationsClient> logger = LoggerFactory.CreateLogger<AutomationOperationsClient>();

            var result = new AutomationOperationsClient(new Uri(XconnectUrl), null, null, logger); //new[] { certificateModifier }

            return result;
        }

        private static ILoggerFactory _Factory = null;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_Factory == null)
                {
                    _Factory = new LoggerFactory();
                    ConfigureLogger(_Factory);
                }
                return _Factory;
            }
            set { _Factory = value; }
        }

        public static void ConfigureLogger(ILoggerFactory factory)
        {
            
        }

    }
}
