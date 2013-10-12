using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json.Linq;
using Sitecore.Diagnostics;
using Sitecore.Social.Core.Connector;
using Sitecore.Social.Core.Connector.Managers;
using Sitecore.Social.Core.NetworkFields;
using Sitecore.Social.Core.Networks;
using Sitecore.Social.Core.Networks.Accounts;
using Sitecore.Social.Core.Networks.Args;
using Sitecore.Social.Core.Networks.Providers;
using Sitecore.Social.Core.Networks.Providers.Interfaces;

namespace scSocialContrib.Instagram.Providers
{
	public class InstagramProvider : NetworkProvider, IAuth, IGetAccountInfo
	{
		public InstagramProvider(Application application)
			: base(application)
		{
		}

		public void AuthGetCode(AuthArgs args)
		{
			HttpRequest request = HttpContext.Current.Request;
			HttpResponse response = HttpContext.Current.Response;
			string redirectUri = request.Url.Scheme + "://" + request.Url.Host + "/layouts/system/Social/Connector/SocialLogin.ashx?type=access_" +
			                     args.StateKey;

			string url =
				string.Format("https://api.instagram.com/oauth/authorize/?client_id={0}&response_type=code&redirect_uri={1}&type=access&state={2}",
				              (object) args.Application.ApplicationKey, (object) redirectUri, args.StateKey);
			Log.Info("Redirecting to url " + url, this);
			response.Redirect(url);
		}

		public void AuthGetAccessToken(AuthArgs args)
		{
			HttpRequest httpRequest = HttpContext.Current.Request;
			if (!string.IsNullOrEmpty(httpRequest.QueryString.Get("error")))
				return;
			string codeReturnedFromInstagram = httpRequest.QueryString.Get("code");
			if (string.IsNullOrEmpty(codeReturnedFromInstagram))
				return;

			string uriString = "https://api.instagram.com/oauth/access_token";

			string redirectUri = httpRequest.Url.Scheme + "://" + httpRequest.Url.Host +
			                     "/layouts/system/Social/Connector/SocialLogin.ashx?type=access_" + args.StateKey;
			Log.Info("Code:" + codeReturnedFromInstagram, this);
			Log.Info("redirectUri:" + redirectUri, this);
			string formData =
				string.Format("client_id={0}&client_secret={1}&redirect_uri={2}&code={3}&grant_type=authorization_code",
				              args.Application.ApplicationKey,
				              args.Application.ApplicationSecret,
				              redirectUri,
				              codeReturnedFromInstagram);
			var webRequestManager = new WebRequestManager(uriString, "POST", formData);
			var response = webRequestManager.GetResponse();

			Log.Info(response, this);
			JObject o = JObject.Parse(response);


			AuthCompletedArgs authCompletedArgs = new AuthCompletedArgs
				{
					AccessToken = (string) o["access_token"],
					Application = args.Application,
					AttachAccountToLoggedInUser = args.AttachAccountToLoggedInUser,
					ExternalData = args.ExternalData,
					CallbackPage = args.CallbackPage,
					IsAsyncProfileUpdate = args.IsAsyncProfileUpdate

				};
			this.InvokeAuthCompleted(args.CallbackType, authCompletedArgs);


		}

		public AccountBasicData GetAccountBasicData(Account account)
		{
			IDictionary<string, object> accountData = GetAccountData(account);
			var accountBasicData = new AccountBasicData
				{
					Account = account,
					FullName = (string) accountData["full_name"],
					Id = (string) accountData["id"]
				};
			return accountBasicData;
		}

		private IDictionary<string, object> GetAccountData(Account account)
		{
			var jsonObject = GetServiceResponseAsJsonObject(account, string.Empty);
			Log.Info("o[\"data\"][\"id\"]", (string) jsonObject["data"]["id"]);

			var returnValue = new Dictionary<string, object>();
			returnValue.Add("id", ExtractStringFromJsonDataElement(jsonObject, "id"));
			returnValue.Add("username", ExtractStringFromJsonDataElement(jsonObject, "username"));
			returnValue.Add("full_name", ExtractStringFromJsonDataElement(jsonObject, "full_name"));
			returnValue.Add("profile_picture", ExtractStringFromJsonDataElement(jsonObject, "profile_picture"));
			return returnValue;
		}

		private JObject GetServiceResponseAsJsonObject(Account account, string serviceRequest)
		{
			string uriString = string.Format("https://api.instagram.com/v1/users/self/{1}?access_token={0}", account.AccessToken,
			                                 serviceRequest);

			var webRequestManager = new WebRequestManager(uriString);
			var response = webRequestManager.GetResponse();
			Log.Info(string.Format("SeviceRequest:{0} Response:{1}" , serviceRequest, response), this);
			JObject o = JObject.Parse(response);
			return o;
		}

		private static string ExtractStringFromJsonDataElement(JObject jsonObject, string key)
		{
			return (string) jsonObject["data"][key];
		}

		public IEnumerable<Field> GetAccountInfo(Account account, IEnumerable<FieldInfo> acceptedFields)
		{
			var userDataFromInstagram = GetAccountData(account);
			var returnCollection = new List<Field>();

			foreach (FieldInfo acceptedField in acceptedFields)
			{

				string loadFromservice = GetAttributeValueOrNull(acceptedField, "loadFromService");
					if (!string.IsNullOrWhiteSpace(loadFromservice))
					{
						returnCollection.Add(new Field
							{
								Name = acceptedField.SitecoreKey,
								Value = GetFromService(account, loadFromservice)
							});
					}
					else
					{
						if (userDataFromInstagram.Keys.Contains(acceptedField.OriginalKey))
						{
							returnCollection.Add(new Field
								{
									Name = acceptedField.SitecoreKey,
									Value = (string) userDataFromInstagram[acceptedField.OriginalKey]
								});
						}
					}
				}

			
			return returnCollection;
		}

		private static string GetAttributeValueOrNull(FieldInfo acceptedField, string attributeName)
		{
			try
			{
				return acceptedField[attributeName];
			}
			catch (KeyNotFoundException)
			{
				return null;
			}
			 
		}


		public string GetDisplayName(Account account)
		{
			var userDataFromInstagram = GetAccountData(account);
			return (string)userDataFromInstagram["full_name"];
		}

		public string GetAccountId(Account account)
		{
			var userDataFromInstagram = GetAccountData(account);
			return (string)userDataFromInstagram["id"];
		}


		private string GetFromService(Account account, string loadFromService)
		{
			var response = GetServiceResponseAsJsonObject(account, loadFromService);
			return response["data"].ToString();
		}
	}
}