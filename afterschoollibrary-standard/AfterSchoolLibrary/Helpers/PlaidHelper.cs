using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using BasicLibrary;
using Plaid.Exceptions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AfterSchoolLibrary.Helpers
{
    public class PlaidHelper
    {
        private static string ClientId => ConfigurationManager.AppSettings["Plaid.ClientId"];
        public static string PublicKey => ConfigurationManager.AppSettings["Plaid.PublicKey"];
        private static string SecretKey => ConfigurationManager.AppSettings["Plaid.SecretKey"];
        public static string PlaidEnvironment => ConfigurationManager.AppSettings["Plaid.Environment"];

        private static Plaid.Services.TokenExchangeService GetTokenExchangeService()
        {
            return new Plaid.Services.TokenExchangeService(ClientId, SecretKey);
        }

        public static Plaid.Models.TokenExchange GetPlaidToken(string token, SystemMessages msgs)
        {
            var options = new Plaid.Models.TokenExchangeOptions()
            {
                PublicToken = token
            };

            Plaid.Models.TokenExchange plaidToken = null;

            try
            {
                plaidToken = GetTokenExchangeService().ExchangeToken(options);
            }
            catch (Exception ex)
            {
                msgs.Add("Error creating exchanging token", MessageType.Error, ex);
            }

            return plaidToken;
        }

        private static Plaid.Services.BankAccountCreateService GetBankAccountCreateService()
        {
            return new Plaid.Services.BankAccountCreateService(ClientId, SecretKey);
        }

        public static Plaid.Models.BankAccount CreateStripeBankAccount(string token, string accountId, SystemMessages msgs)
        {
            var options = new Plaid.Models.BankAccountCreateOptions()
            {
                AccessToken = token,
                AccountId = accountId
            };

            Plaid.Models.BankAccount bankAccount = null;

            try
            {
                bankAccount = GetBankAccountCreateService().CreateBankAccount(options);
            }
            catch (Exception ex)
            {
                msgs.Add("Error creating bank account", MessageType.Error, ex);
            }

            return bankAccount;
        }
    }
}

namespace Plaid
{
    namespace Services
    {
        public abstract class PlaidService
        {
            protected string ClientId { get; set; }
            protected string SecretKey { get; set; }

            protected HttpClient client;
            protected const string PlaidApiVersion = "2019-05-29";
            protected const string ClientVersion = ".1";

            private static string BaseUrl => ConfigurationManager.AppSettings["Plaid.BaseUrl"];

            public PlaidService(string clientId, string secretKey)
            {
                ClientId = clientId;
                SecretKey = secretKey;

                client = new HttpClient();
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"Plaid/v1 AFTRSCHL/{ClientVersion}");
                client.DefaultRequestHeaders.Add("Plaid-Version", PlaidApiVersion);
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
                        throw new UnauthorizedException();

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        throw new NotFoundException();

                    if (response.StatusCode == HttpStatusCode.BadRequest || (int)response.StatusCode == 422)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        throw new Exception(content);
                    }

                    if ((int)response.StatusCode == 429)
                        throw new TooManyRequestsException();

                    if ((int)response.StatusCode >= 500)
                        throw new ServerErrorException();

                    throw new Exception("An unexpected error occured");
                }

                var resultText = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject(resultText);
                return result;
            }
        }

        public class TokenExchangeService : PlaidService
        {
            private const string Url = "/item/public_token/exchange";
            private const string CreateMethod = "POST";

            public TokenExchangeService(string clientId, string secretKey) : base(clientId, secretKey)
            {

            }

            public Models.TokenExchange ExchangeToken(Models.TokenExchangeOptions options)
            {
                dynamic result = SendRequest(Url, CreateMethod, null, options.ToDynamicObject(ClientId, SecretKey));
                return Models.TokenExchange.FromDynamicObject(result);
            }

        }

        public class BankAccountCreateService: PlaidService
        {
            private const string Url = "/processor/stripe/bank_account_token/create";
            private const string CreateMethod = "POST";

            public BankAccountCreateService(string clientId, string secretKey) : base(clientId, secretKey)
            {

            }

            public Models.BankAccount CreateBankAccount(Models.BankAccountCreateOptions options)
            {
                dynamic result = SendRequest(Url, CreateMethod, null, options.ToDynamicObject(ClientId, SecretKey));
                return Models.BankAccount.FromDynamicObject(result);
            }

        }
    }

    namespace Models
    {
        public abstract class PlaidObject
        {
            public string RequestId { get; set; }
        }
        
        public class TokenExchange : PlaidObject
        {
            public string AccessToken { get; set; }
            public string ItemId { get; set; }

            internal static TokenExchange FromDynamicObject(dynamic result)
            {
                return new TokenExchange()
                {
                    AccessToken = (string)result["access_token"],
                    ItemId = (string)result["item_id"],
                    RequestId = (string)result["request_id"]
                };
            }
        }
        
        public class BankAccount : PlaidObject
        {
            public string StripeBankAccountToken { get; set; }

            internal static BankAccount FromDynamicObject(dynamic result)
            {
                return new BankAccount()
                {
                    StripeBankAccountToken = (string)result["stripe_bank_account_token"],
                    RequestId = (string)result["request_id"]
                };
            }
        }

        public class TokenExchangeOptions
        {
            public string PublicToken { get; set; }

            internal dynamic ToDynamicObject(string clientId, string secretKey)
            {
                return new
                {
                    client_id = clientId,
                    secret = secretKey,
                    public_token = PublicToken,
                };
            }
        }

        public class BankAccountCreateOptions
        {
            public string AccessToken { get; set; }
            public string AccountId { get; set; }

            internal dynamic ToDynamicObject(string clientId, string secretKey)
            {
                return new
                {
                    client_id = clientId,
                    secret = secretKey,
                    access_token = AccessToken,
                    account_id = AccountId,
                };
            }
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