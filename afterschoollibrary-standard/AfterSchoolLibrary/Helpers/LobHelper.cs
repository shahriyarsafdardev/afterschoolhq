using BasicLibrary;
using Lob.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AfterSchoolLibrary.Helpers
{
    public class LobHelper
    {
        private static string TestSecretKey { get { return "test_51a0a27eacb130427cd132a01e658c94740"; } }
        private static string TestPublishableKey { get { return "test_pub_2d3cb69363d82c6e3ed80dc3037bbac"; } }
        private static string LiveSecretKey { get { return ConfigurationManager.AppSettings["Lob.LiveSecretKey"]; } }
        private static string LivePublishableKey { get { return ConfigurationManager.AppSettings["Lob.LivePublishableKey"]; } }

        public static string SecretKey
        {
            get
            {
                if (Helpers.Utilities.IsProduction)
                    return LiveSecretKey;

                return TestSecretKey;
            }
        }
        public static string PublishableKey
        {
            get
            {
                if (Helpers.Utilities.IsProduction)
                    return LivePublishableKey;

                return TestPublishableKey;
            }
        }

        private static Lob.Services.AddressVerificationService GetVerificationService()
        {
            return new Lob.Services.AddressVerificationService(SecretKey);
        }

        public static bool IsDeliverableAddress(string address1, string city, string state, string postalCode, SystemMessages msgs)
        {
            var options = new Lob.Models.AddressVerificationOptions()
            {
                AddressLine1 = address1,
                City = city,
                State = state,
                Zip = postalCode
            };

            Lob.Models.AddressVerification addressVerification = null;

            try
            {
                addressVerification = GetVerificationService().Verify(options);
            }
            catch (Exception ex)
            {
                msgs.Add("Error creating check", MessageType.Error);
            }

            return addressVerification.Deliverability != Lob.Models.Deliverability.Undeliverable;
        }

        private static Lob.Services.CheckService GetCheckService()
        {
            return new Lob.Services.CheckService(SecretKey);
        }

        public static Lob.Models.Check CreateCheck(string description, Lob.Models.AccountType accountType, string toName, string toAddress1, string toAddress2, string toCity, string toState, string toPostalCode, string fromAddressId, string bankAccountId, decimal amount, string memo, string checkBottom, string attachment, dynamic metaData, SystemMessages msgs)
        {
            var toAddress = new Lob.Models.AddressCreateOptions()
            {
                AddressLine1 = toAddress1,
                AddressLine2 = toAddress2,
                City = toCity,
                State = toState,
                Zip = toPostalCode
            };

            if (accountType == Lob.Models.AccountType.Company)
                toAddress.Company = toName;
            else
                toAddress.Name = toName;

            var options = new Lob.Models.CheckCreateOptions()
            {
                Description = description,
                ToAddress = toAddress,
                FromAddressId = fromAddressId,
                BankAccountId = bankAccountId,
                Amount = amount,
                Memo = memo,
                CheckBottom = checkBottom,
                Attachment = attachment,
                MetaData = metaData,                
            };

            Lob.Models.Check check = null;

            try
            {
                check = GetCheckService().Create(options);
            }
            catch(Exception ex)
            {
                msgs.Add("Error creating check.", MessageType.Error, ex);
            }

            return check;
        }

        public static SystemMessages CancelCheck(string checkId)
        {
            var msgs = new SystemMessages();

            Lob.Models.Check check = null;

            try
            {
                check = GetCheckService().Cancel(checkId);

                if (!check.Deleted)
                    msgs.Add($"Check ({checkId}) was not canceled", MessageType.UserError);
            }
            catch (Exception ex)
            {
                msgs.Add("Error canceling check", MessageType.Error);
            }

            return msgs;
        }
    }
}

namespace Lob
{
    namespace Services
    {
        public abstract class LobService
        {
            protected HttpClient client;
            protected const string LobApiVersion = "2018-06-05";
            protected const string ClientVersion = ".1";
            protected const string BaseUrl = "https://api.lob.com";

            public LobService(string secretKey)
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{secretKey}:{""}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"Lob/v1 AFTRSCHL/{ClientVersion}");
                client.DefaultRequestHeaders.Add("Lob-Version", LobApiVersion);
            }

            protected dynamic SendRequest(string path, string method, string idPath = null, dynamic body = null)
            {
                // Get the full path from the path and url parameters
                string fullPath = $"{BaseUrl}{path}";

                if (idPath.IsNotEmpty())
                    fullPath += idPath;

                // Set the http headers

                HttpResponseMessage response;
                // Get the response
                if (method == "GET")
                    response = client.GetAsync(fullPath).Result;
                else if (method == "POST")
                    response = client.PostAsync(fullPath, new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")).Result;
                else if (method == "DELETE")
                    response = client.DeleteAsync(fullPath).Result;
                else
                    response = client.GetAsync(fullPath).Result;

                // If not a succesful response than process throw the appropriate exception based on response code
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new NotFoundException();
                    }
                    if (response.StatusCode == HttpStatusCode.BadRequest || (int)response.StatusCode == 422)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        throw new Exception(content);
                    }
                    if ((int)response.StatusCode == 429)
                    {
                        throw new TooManyRequestsException();
                    }
                    if ((int)response.StatusCode >= 500)
                    {
                        throw new ServerErrorException();
                    }

                    throw new Exception("An unexpected error occured");
                }

                var resultText = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject(resultText);
                return result;
            }
        }

        public class AddressVerificationService: LobService
        {
            private const string Url = "/v1/us_verifications";
            private const string CreateMethod = "POST";

            public AddressVerificationService(string secretKey) : base(secretKey)
            {

            }

            public Models.AddressVerification Verify(Models.AddressVerificationOptions options)
            {
                dynamic result = SendRequest(Url, CreateMethod, null, options.ToDynamicObject());
                return Models.AddressVerification.FromDynamicObject(result);
            }

        }

        public class CheckService : LobService
        {
            private const string Url = "/v1/checks";
            private const string CreateMethod = "POST";

            public CheckService(string secretKey) : base(secretKey)
            {

            }

            public Models.Check Create(Models.CheckCreateOptions options)
            {
                dynamic result = SendRequest(Url, CreateMethod, null, options.ToDynamicObject());
                return Models.Check.FromDynamicObject(result);
            }

            public Models.Check Cancel(string checkId)
            {
                dynamic result = SendRequest(Url, CreateMethod, $"/{checkId}", null);
                return new Models.Check()
                {
                    Id = result["id"],
                    Deleted = result["deleted"],                    
                };
            }
        }
    }

    namespace Models
    {
        public abstract class LobObject
        {
            public string Id { get; set; }
            public string Description { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime DateModified { get; set; }
            public bool Deleted { get; set; }
            public string Object { get; set; }
            public string MetaData { get; set; }
        }

        public class Address : LobObject
        {
            public string Name { get; set; }
            public string Company { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public string Country { get; set; }
            public bool RecipientMoved { get; set; }

            public Address()
            {

            }

            internal static Address FromDynamicObject(dynamic result)
            {
                var address = new Models.Address()
                {
                    Id = (string)result["id"],
                    Description = (string)result["description"],
                    Name = (string)result["name"],
                    Company = (string)result["company"],
                    Phone = (string)result["phone"],
                    Email = (string)result["email"],
                    AddressLine1 = (string)result["address_line_1"],
                    AddressLine2 = (string)result["address_line_2"],
                    City = (string)result["address_city"],
                    State = (string)result["address_state"],
                    Zip = (string)result["address_zip"],
                    Country = (string)result["address_country"],
                    // MetaData = result["metadata"],
                    DateCreated = (DateTime)result["date_created"],
                    DateModified = (DateTime)result["date_modified"],
                    Object = (string)result["object"]
                };

                if (result["deleted"] != null)
                    address.Deleted = (bool)result["deleted"];

                return address;
            }
        }

        public class BankAccount : LobObject
        {
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public AccountType AccountType { get; set; }
            public string Signatory { get; set; }
            public string SignatureUrl { get; set; }
            public string BankName { get; set; }
            public bool Verified { get; set; }

            public BankAccount()
            {

            }

            internal static BankAccount FromDynamicObject(dynamic result)
            {
                return new BankAccount()
                {
                    Id = (string)result["id"],
                    Description = (string)result["description"],
                    RoutingNumber = (string)result["routing_number"],
                    AccountNumber = (string)result["account_number"],
                    Signatory = (string)result["signatory"],
                    BankName = (string)result["bank_name"],
                    Verified = (bool)result["verified"],
                    AccountType = ((string)result["account_type"]).Replace("_", " ").ToProperCase().Replace(" ", "").ParseEnum<Lob.Models.AccountType>(),
                    DateCreated = (DateTime)result["date_created"],
                    DateModified = (DateTime)result["date_modified"],
                    Object = (string)result["object"]
                };
            }
        }

        public class Thumbnail
        {
            public string Small { get; set; }
            public string Medium { get; set; }
            public string Large { get; set; }

            public Thumbnail()
            {

            }
        }

        public class TrackingEvent : LobObject
        {
            public string Location { get; set; }
            public DateTime Time { get; set; }

            public TrackingEvent()
            {

            }
        }

        public class AddressVerification
        {
            public string Id { get; set; }
            public Deliverability Deliverability { get; set; }

            internal static AddressVerification FromDynamicObject(dynamic result)
            {
                return new AddressVerification()
                {
                    Id = (string)result["id"],
                    Deliverability = ((string)result["deliverability"]).Replace("_", " ").ToProperCase().Replace(" ", "").ParseEnum<Deliverability>(),
                };
            }
        }

        public class Check : LobObject
        {
            public int CheckNumber { get; set; }
            public string Memo { get; set; }
            public decimal Amount { get; set; }
            public string Message { get; set; }
            public string Url { get; set; }
            public string CheckBottomTemplateId { get; set; }
            public string AttachmentTemplateVersionId { get; set; }
            public Address To { get; set; }
            public Address From { get; set; }
            public BankAccount BankAccount { get; set; }
            public CarrierType Carrier { get; set; }
            public string TrackingNumber { get; set; }
            public List<TrackingEvent> TrackingEvents { get; set; }
            public DateTime ExpectedDeliveryDate { get; set; }
            public MailType MailType { get; set; }
            public DateTime SendDate { get; set; }
            public List<Thumbnail> Thumbnails { get; set; }

            public Check()
            {

            }

            internal static Check FromDynamicObject(dynamic result)
            {
                return new Check()
                {
                    Id = (string)result["id"],
                    Description = (string)result["description"],
                    CheckNumber = (int)result["check_number"],
                    Memo = (string)result["memo"],
                    Amount = (decimal)result["amount"],
                    Message = (string)result["message"],
                    Url = (string)result["url"],
                    CheckBottomTemplateId = (string)result["check_bottom_template_id"],
                    AttachmentTemplateVersionId = (string)result["attachment_template_version_id"],
                    To = Address.FromDynamicObject(result["to"]),
                    From = Address.FromDynamicObject(result["from"]),
                    BankAccount = BankAccount.FromDynamicObject(result["bank_account"]),
                    Carrier = ((string)result["carrier"]).Replace("_", " ").ToProperCase().Replace(" ", "").ParseEnum<CarrierType>(),
                    TrackingNumber = (string)result["tracking_number"],
                    TrackingEvents = ((Newtonsoft.Json.Linq.JArray)result["tracking_events"]).ToObject<List<TrackingEvent>>(),
                    Thumbnails = ((Newtonsoft.Json.Linq.JArray)result["thumbnails"]).ToObject<List<Thumbnail>>(),
                    ExpectedDeliveryDate = (DateTime)result["expected_delivery_date"],
                    MailType = ((string)result["mail_type"]).Replace("_", " ").ToProperCase().Replace(" ", "").ParseEnum<Lob.Models.MailType>(),
                    SendDate = (DateTime)result["send_date"],
                    DateCreated = (DateTime)result["date_created"],
                    DateModified = (DateTime)result["date_modified"],
                    Object = (string)result["object"]
                };
            }
        }

        public class CheckCreateOptions
        {
            public string Description { get; set; }
            public AddressCreateOptions ToAddress { get; set; }
            public string ToAddressId { get; set; }
            public string FromAddressId { get; set; }
            public string BankAccountId { get; set; }
            public decimal Amount { get; set; }
            public string Memo { get; set; }
            public string Logo { get; set; }
            public string CheckBottom { get; set; }
            public NameValueCollection MergeVariables { get; set; }
            public object MetaData { get; set; }
            public string Attachment { get; set; }

            internal dynamic ToDynamicObject()
            {
                return new
                {
                    description = Description,
                    to = new
                    {
                        name = ToAddress.Name,
                        company = ToAddress.Company,
                        address_line1 = ToAddress.AddressLine1,
                        address_line2 = ToAddress.AddressLine2,
                        address_city = ToAddress.City,
                        address_state = ToAddress.State,
                        address_zip = ToAddress.Zip
                    },
                    from = FromAddressId,
                    bank_account = BankAccountId,
                    amount = Amount,
                    memo = Memo,
                    check_bottom  = CheckBottom,
                    attachment = Attachment,
                    metadata = MetaData
                };
            }
        }

        public class AddressVerificationOptions
        {
            public string AddressLine1 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }

            internal dynamic ToDynamicObject()
            {
                return new
                {
                    primary_line = AddressLine1,
                    city = City,
                    state = State,
                    zip_code = Zip
                };
            }
        }

        public class AddressCreateOptions
        {
            public string Name { get; set; }
            public string Company { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
        }

        public enum AccountType
        {
            Company,
            Individual
        }

        public enum CarrierType
        {
            USPS,
            UPS
        }

        public enum MailType
        {
            USPSFirstClass,
            UPSNextDayAir
        }

        public enum Deliverability
        {
            Deliverable,
            DeliverableUnnecessaryUnit,
            DeliverableIncorrectUnit,
            DeliverableMissingUnit,
            Undeliverable
        }
    }

    namespace Exceptions
    {
        public class UnauthorizedException : Exception
        {
        }

        public class ForbiddenException : Exception
        {
        }

        public class NotFoundException : Exception
        {
        }

        public class BadRequestException : Exception
        {
        }

        public class TooManyRequestsException : Exception
        {
        }

        public class ServerErrorException : Exception
        {
        }
    }
}

