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

namespace Demo9.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = "email";
            string identifier = "avh@brimit.com"; 

            AddInteractionTest(source, identifier);

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
    }
}
