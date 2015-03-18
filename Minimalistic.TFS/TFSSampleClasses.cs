using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.IdentityModel.S2S.Tokens;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json.Linq;
using Attachment = Microsoft.Exchange.WebServices.Data.Attachment;
using AudienceRestriction = Microsoft.IdentityModel.Tokens.AudienceRestriction;
using ConfigurationBasedIssuerNameRegistry = Microsoft.IdentityModel.Tokens.ConfigurationBasedIssuerNameRegistry;
using SecurityTokenHandlerConfiguration = Microsoft.IdentityModel.Tokens.SecurityTokenHandlerConfiguration;

namespace Minimalistic.TFS
{
	public class MembersController
	{
		// GET: api/Members
		public IEnumerable<string> Get()
		{

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Add(
			   new MediaTypeWithQualityHeaderValue("application/json"));

				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(
							string.Format("{0}:{1}", ConfigurationManager.AppSettings["VSOUsername"], ConfigurationManager.AppSettings["VSOPassword"]))));

				var uri = "https://dpeted.visualstudio.com/DefaultCollection/_api/_identity/ReadGroupMembers?__v=5&scope=aacd916a-ba75-4820-b582-731c9525fa5d&readMembers=true";
				var hasMore = true;
				var tenantMembers = new List<string>();

				while (hasMore)
				{
					using (var response = client.GetAsync(uri).Result)
					{
						response.EnsureSuccessStatusCode();
						var responseBody = response.Content.ReadAsStringAsync().Result;
						var responseArray = (JArray)JObject.Parse(responseBody)["identities"];

						var userIdentities = (from JToken token in responseArray
											  select token["FriendlyDisplayName"].Value<string>()).ToList();

						tenantMembers.AddRange(userIdentities.Select(i => i).ToArray());

						if (JObject.Parse(responseBody)["hasMore"].Value<bool>())
						{
							uri = "https://dpeted.visualstudio.com/DefaultCollection/_api/_identity/ReadGroupMembers?__v=5&scope=aacd916a-ba75-4820-b582-731c9525fa5d&lastSearchResult=" + userIdentities.Last() + "&readMembers=true";
						}
						else
							hasMore = false;

					}
				}
				return tenantMembers;

			}

		}
	}

	#region stuff
	public class JsonAuthMetadataDocument
	{
		public string id { get; set; }
		public string version { get; set; }
		public string name { get; set; }
		public string realm { get; set; }
		public string serviceName { get; set; }
		public string issuer { get; set; }
		public string[] allowedAudiences { get; set; }
		public JsonKey[] keys;
		public JsonEndpoint[] endpoints;
	}

	public class JsonEndpoint
	{
		public string location { get; set; }
		public string protocol { get; set; }
		public string usage { get; set; }
	}

	public class JsonKey
	{
		public string usage { get; set; }
		public JsonKeyValue keyValue { get; set; }
	}

	public class JsonKeyValue
	{
		public string type { get; set; }
		public string value { get; set; }
	}

	public class IdentityTokenRequest
	{
		public string token { get; set; }
	}

	#endregion
	public class AttachmentSampleServiceResponse
	{
		public string[] attachmentNames { get; set; }
		public int attachmentsProcessed { get; set; }
	}

	public class IdentityToken
	{
		public string msexchuid { get; set; }
		public string amurl { get; set; }
		public string uniqueID
		{
			get { return ComputeUniqueIdentification(); }
		}

		public string iss { get; set; }
		public string x5t { get; set; }
		public DateTime nbf { get; set; }
		public DateTime exp { get; set; }
		public string aud { get; set; }
		public string version { get; set; }
		public bool isbrowserhostedapp { get; set; }
		public string appctxsender { get; set; }

		// Salt to apply when creating unique ID.
		private byte[] Salt = new byte[] { 25, 139, 201, 13 };

		private string ComputeUniqueIdentification()
		{
			byte[] inputBytes = Encoding.ASCII.GetBytes(string.Concat(msexchuid, amurl));

			// Combine input bytes and salt.
			byte[] saltedInput = new byte[Salt.Length + inputBytes.Length];
			Salt.CopyTo(saltedInput, 0);
			inputBytes.CopyTo(saltedInput, Salt.Length);

			// Compute the unique key.
			byte[] hashedBytes = SHA256CryptoServiceProvider.Create().ComputeHash(saltedInput);

			// Convert the hashed value to a string and return.
			return BitConverter.ToString(hashedBytes);
		}

		private JsonWebSecurityTokenHandler GetSecurityTokenHandler(string audience,
			string authMetadataEndpoint,
			X509Certificate2 currentCertificate)
		{
			JsonWebSecurityTokenHandler jsonTokenHandler = new JsonWebSecurityTokenHandler();
			jsonTokenHandler.Configuration = new SecurityTokenHandlerConfiguration();

			jsonTokenHandler.Configuration.AudienceRestriction = new AudienceRestriction(AudienceUriMode.Always);
			jsonTokenHandler.Configuration.AudienceRestriction.AllowedAudienceUris.Add(
			  new Uri(audience, UriKind.RelativeOrAbsolute));

			jsonTokenHandler.Configuration.CertificateValidator = X509CertificateValidator.None;

			jsonTokenHandler.Configuration.IssuerTokenResolver =
			  SecurityTokenResolver.CreateDefaultSecurityTokenResolver(
				new ReadOnlyCollection<SecurityToken>(new List<SecurityToken>(
				  new SecurityToken[]
			{
			  new X509SecurityToken(currentCertificate)
			})), false);

			ConfigurationBasedIssuerNameRegistry issuerNameRegistry =
				new ConfigurationBasedIssuerNameRegistry();
			issuerNameRegistry.AddTrustedIssuer(currentCertificate.Thumbprint, "VSOBug");
			jsonTokenHandler.Configuration.IssuerNameRegistry = issuerNameRegistry;

			return jsonTokenHandler;
		}

		private X509Certificate2 GetSigningCertificate(Uri authMetadataEndpoint)
		{
			JsonAuthMetadataDocument document = GetMetadataDocument(authMetadataEndpoint);

			if (null != document.keys && document.keys.Length > 0)
			{
				JsonKey signingKey = document.keys[0];

				if (null != signingKey && null != signingKey.keyValue)
				{
					return new X509Certificate2(Encoding.UTF8.GetBytes(signingKey.keyValue.value));
				}
			}

			throw new ApplicationException("The metadata document does not contain a signing certificate.");
		}
		private JsonAuthMetadataDocument GetMetadataDocument(Uri authMetadataEndpoint)
		{
			// Uncomment the next line if your Exchange server uses the default
			// self-signed certificate.
			// ServicePointManager.ServerCertificateValidationCallback = Config.CertificateValidationCallback;

			byte[] acsMetadata;
			using (WebClient webClient = new WebClient())
			{
				acsMetadata = webClient.DownloadData(authMetadataEndpoint);
			}
			string jsonResponseString = Encoding.UTF8.GetString(acsMetadata);

			JsonAuthMetadataDocument document = new JavaScriptSerializer().Deserialize<JsonAuthMetadataDocument>(jsonResponseString);

			if (null == document)
			{
				throw new ApplicationException(String.Format("No authentication metadata document found at {0}.", authMetadataEndpoint));
			}

			//using (WebClient webClient = new WebClient())
			//{
			//    var x = webClient.DownloadData("https://outlook.office365.com/EWS/OData/Me");

			//}

			return document;
		}



		public IdentityToken(IdentityTokenRequest rawToken, string audience, string authMetadataEndpoint)
		{
			X509Certificate2 currentCertificate = null;

			currentCertificate = GetSigningCertificate(new Uri(authMetadataEndpoint));

			JsonWebSecurityTokenHandler jsonTokenHandler =
				GetSecurityTokenHandler(audience, authMetadataEndpoint, currentCertificate);

			SecurityToken jsonToken = jsonTokenHandler.ReadToken(rawToken.token);
			JsonWebSecurityToken webToken = (JsonWebSecurityToken)jsonToken;

			x5t = currentCertificate.Thumbprint;
			iss = webToken.Issuer;
			aud = webToken.Audience;
			exp = webToken.ValidTo;
			nbf = webToken.ValidFrom;
			foreach (JsonWebTokenClaim claim in webToken.Claims)
			{
				if (claim.ClaimType.Equals("appctxsender"))
				{
					appctxsender = claim.Value;
				}

				if (claim.ClaimType.Equals("isbrowserhostedapp"))
				{
					isbrowserhostedapp = claim.Value == "true";
				}

				if (claim.ClaimType.Equals("appctx"))
				{
					string[] appContextClaims = claim.Value.Split(',');
					Dictionary<string, string> appContext =
						new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(claim.Value);
					amurl = appContext["amurl"];
					msexchuid = appContext["msexchuid"];
					version = appContext["version"];
				}
			}
		}

	}


	internal class JsonToken
	{
		public bool IsValid;
		public Dictionary<string, string> headerClaims;
		public Dictionary<string, string> payloadClaims;
		public string signature;
		public Dictionary<string, string> appContext;

		private void ValidateHeaderClaim(string key, string value)
		{
			if (!this.headerClaims.ContainsKey(key))
			{
				throw new ApplicationException(String.Format("Header does not contain \"{0}\" claim.", key));
			}

			if (!value.Equals(this.headerClaims[key]))
			{
				throw new ApplicationException(String.Format("\"{0}\" claim must be \"{0}\".", key, value));
			}
		}

		private void ValidateHeader()
		{
			ValidateHeaderClaim("typ", "JWT");
			ValidateHeaderClaim("alg", "RS256");

			if (!this.headerClaims.ContainsKey("x5t"))
			{
				throw new ApplicationException(String.Format("Header does not contain \"{0}\" claim.", "x5t"));
			}
		}
		private void ValidateLifetime()
		{
			if (!this.payloadClaims.ContainsKey("nbf"))
			{
				throw new ApplicationException(
				  String.Format("The \"{0}\" claim is missing from the token.", "nbf"));
			}

			if (!this.payloadClaims.ContainsKey("exp"))
			{
				throw new ApplicationException(
				  String.Format("The \"{0}\" claim is missing from the token.", "exp"));
			}

			DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			TimeSpan padding = new TimeSpan(0, 5, 0);

			DateTime validFrom = unixEpoch.AddSeconds(int.Parse(this.payloadClaims["nbf"]));
			DateTime validTo = unixEpoch.AddSeconds(int.Parse(this.payloadClaims["exp"]));

			DateTime now = DateTime.UtcNow;

			if (now < (validFrom - padding))
			{
				throw new ApplicationException(String.Format("The token is not valid until {0}.", validFrom));
			}

			if (now > (validTo + padding))
			{
				throw new ApplicationException(String.Format("The token is not valid after {0}.", validFrom));
			}
		}
		private void ValidateMetadataLocation()
		{
			if (!this.appContext.ContainsKey("amurl"))
			{
				throw new ApplicationException(String.Format("The \"{0}\" claim is missing from the token.", "amurl"));
			}
		}



		private void ValidateAudience()
		{
			if (!this.payloadClaims.ContainsKey("aud"))
			{
				throw new ApplicationException(String.Format("The \"{0}\" claim is missing from the application context.", "aud"));
			}

		}



		public JsonToken(Dictionary<string, string> header, Dictionary<string, string> payload, string signature)
		{

			// Assume that the token is invalid to start out.
			this.IsValid = false;

			// Set the private dictionaries that contain the claims.
			this.headerClaims = header;
			this.payloadClaims = payload;
			this.signature = signature;

			// If there is no "appctx" claim in the token, throw an ApplicationException.
			if (!this.payloadClaims.ContainsKey("appctx"))
			{
				throw new ApplicationException(String.Format("The {0} claim is not present.", "appctx"));
			}

			appContext = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(payload["appctx"]);


			// Validate the header fields.
			this.ValidateHeader();

			// Determine whether the token is within its valid time.
			this.ValidateLifetime();

			// Validate that the token was sent to the correct URL.
			this.ValidateAudience();

			// Make sure that the appctx contains an authentication
			// metadata location.
			this.ValidateMetadataLocation();

			// If the token passes all the validation checks, we
			// can assume that it is valid.
			this.IsValid = true;
		}

	}
	internal class TokenValidator
	{
		public static Encoding TextEncoding = Encoding.UTF8;

		private static char Base64PadCharacter = '=';
		private static char Base64Character62 = '+';
		private static char Base64Character63 = '/';
		private static char Base64UrlCharacter62 = '-';
		private static char Base64UrlCharacter63 = '_';

		private static byte[] DecodeBytes(string arg)
		{
			if (String.IsNullOrEmpty(arg))
			{
				throw new ApplicationException("String to decode cannot be null or empty.");
			}

			StringBuilder s = new StringBuilder(arg);
			s.Replace(Base64UrlCharacter62, Base64Character62);
			s.Replace(Base64UrlCharacter63, Base64Character63);

			int pad = s.Length % 4;
			s.Append(Base64PadCharacter, (pad == 0) ? 0 : 4 - pad);

			return Convert.FromBase64String(s.ToString());
		}

		private static string Base64Decode(string arg)
		{
			return TextEncoding.GetString(DecodeBytes(arg));
		}

		public static JsonToken ValidateToken(string token)
		{
			JsonToken jsonToken = Decode(token);

			return jsonToken;
		}


		public static JsonToken Decode(string rawToken)
		{
			string[] tokenParts = rawToken.Split('.');

			if (tokenParts.Length != 3)
			{
				throw new ApplicationException("Token must have three parts separated by '.' characters.");
			}

			string encodedHeader = tokenParts[0];
			string encodedPayload = tokenParts[1];
			string signature = tokenParts[2];

			string decodedHeader = Base64Decode(encodedHeader);
			string decodedPayload = Base64Decode(encodedPayload);

			JavaScriptSerializer serializer = new JavaScriptSerializer();

			Dictionary<string, string> header = serializer.Deserialize<Dictionary<string, string>>(decodedHeader);
			Dictionary<string, string> payload = serializer.Deserialize<Dictionary<string, string>>(decodedPayload);

			return new JsonToken(header, payload, signature);
		}


	}
	public class AppController
	{
		private string StripHTML(string source)
		{

			string result;

			// Remove HTML Development formatting
			// Replace line breaks with space
			// because browsers inserts space
			result = source.Replace("\r", " ");
			// Replace line breaks with space
			// because browsers inserts space
			result = result.Replace("\n", " ");
			// Remove step-formatting
			result = result.Replace("\t", string.Empty);
			// Remove repeating spaces because browsers ignore them
			result = Regex.Replace(result,
																  @"( )+", " ");

			// Remove the header (prepare first by clearing attributes)
			result = Regex.Replace(result,
					 @"<( )*head([^>])*>", "<head>",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"(<( )*(/)( )*head( )*>)", "</head>",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 "(<head>).*(</head>)", string.Empty,
					 RegexOptions.IgnoreCase);

			// remove all scripts (prepare first by clearing attributes)
			result = Regex.Replace(result,
					 @"<( )*script([^>])*>", "<script>",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"(<( )*(/)( )*script( )*>)", "</script>",
					 RegexOptions.IgnoreCase);
			//result = System.Text.RegularExpressions.Regex.Replace(result,
			//         @"(<script>)([^(<script>\.</script>)])*(</script>)",
			//         string.Empty,
			//         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"(<script>).*(</script>)", string.Empty,
					 RegexOptions.IgnoreCase);

			// remove all styles (prepare first by clearing attributes)
			result = Regex.Replace(result,
					 @"<( )*style([^>])*>", "<style>",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"(<( )*(/)( )*style( )*>)", "</style>",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 "(<style>).*(</style>)", string.Empty,
					 RegexOptions.IgnoreCase);

			// insert tabs in spaces of <td> tags
			result = Regex.Replace(result,
					 @"<( )*td([^>])*>", "\t",
					 RegexOptions.IgnoreCase);

			// insert line breaks in places of <BR> and <LI> tags
			result = Regex.Replace(result,
					 @"<( )*br( )*>", "\r",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"<( )*li( )*>", "\r",
					 RegexOptions.IgnoreCase);

			// insert line paragraphs (double line breaks) in place
			// if <P>, <DIV> and <TR> tags
			result = Regex.Replace(result,
					 @"<( )*div([^>])*>", "\r\r",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"<( )*tr([^>])*>", "\r\r",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"<( )*p([^>])*>", "\r\r",
					 RegexOptions.IgnoreCase);

			// Remove remaining tags like <a>, links, images,
			// comments etc - anything that's enclosed inside < >
			result = Regex.Replace(result,
					 @"<[^>]*>", string.Empty,
					 RegexOptions.IgnoreCase);

			// replace special characters:
			result = Regex.Replace(result,
					 @" ", " ",
					 RegexOptions.IgnoreCase);

			result = Regex.Replace(result,
					 @"&bull;", " * ",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"&lsaquo;", "<",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"&rsaquo;", ">",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"&trade;", "(tm)",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"&frasl;", "/",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"&lt;", "<",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"&gt;", ">",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"&copy;", "(c)",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 @"&reg;", "(r)",
					 RegexOptions.IgnoreCase);
			// Remove all others. More can be added, see
			// http://hotwired.lycos.com/webmonkey/reference/special_characters/
			result = Regex.Replace(result,
					 @"&(.{2,6});", string.Empty,
					 RegexOptions.IgnoreCase);

			// for testing
			//System.Text.RegularExpressions.Regex.Replace(result,
			//       this.txtRegex.Text,string.Empty,
			//       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			// make line breaking consistent
			result = result.Replace("\n", "\r");

			// Remove extra line breaks and tabs:
			// replace over 2 breaks with 2 and over 4 tabs with 4.
			// Prepare first to remove any whitespaces in between
			// the escaped characters and remove redundant tabs in between line breaks
			result = Regex.Replace(result,
					 "(\r)( )+(\r)", "\r\r",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 "(\t)( )+(\t)", "\t\t",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 "(\t)( )+(\r)", "\t\r",
					 RegexOptions.IgnoreCase);
			result = Regex.Replace(result,
					 "(\r)( )+(\t)", "\r\t",
					 RegexOptions.IgnoreCase);
			// Remove redundant tabs
			result = Regex.Replace(result,
					 "(\r)(\t)+(\r)", "\r\r",
					 RegexOptions.IgnoreCase);
			// Remove multiple tabs following a line break with just one tab
			result = Regex.Replace(result,
					 "(\r)(\t)+", "\r\t",
					 RegexOptions.IgnoreCase);
			// Initial replacement target string for line breaks
			string breaks = "\r\r\r";
			// Initial replacement target string for tabs
			string tabs = "\t\t\t\t\t";
			for (int index = 0; index < result.Length; index++)
			{
				result = result.Replace(breaks, "\r\r");
				result = result.Replace(tabs, "\t\t\t\t");
				breaks = breaks + "\r";
				tabs = tabs + "\t";
			}

			// That's it.
			return result;

		}
		// GET api/<controller>/5
		public string Get(int id)
		{
			return null;
		}

		//Stub class (OEO 2015.03.17)
		public class NewBugResponse
		{
			public int ID { get; set; }
		}

		//Stub class (OEO 2015.03.17)
		public class NewBugRequest
		{
			public string Token;
			public string EWSUrl;
			public string AttachmentToken;
			public ItemId ItemID { get; set; }
			public string Title;
			public string Area;
			public string AssignedTo;
			public string Notes;
			public List<Attachment> Attachments;
		}

		//Stub class (OEO 2015.03.17)
		public class AppIdentityToken
		{
			public void Validate(Uri uri) {}
		}

		//Stub class (OEO 2015.03.17)
		public class AuthToken { public static object Parse(string token) {return token;} }

		//Stub class (OEO 2015.03.17)
		public class ExchangeService
		{
			public ExchangeService(ExchangeVersion exchange) {}
			public Uri Url { get; set; }

			public OAuthCredentials Credentials;
		}

		//Stub class (OEO 2015.03.17)
		public enum ExchangeVersion
		{
			Exchange2007_SP1
		}

		//Stub class (OEO 2015.03.17)
		public class OAuthCredentials
		{
			public OAuthCredentials(string cred) {}
		}

		//Stub class (OEO 2015.03.17)
		public class EmailMessage
		{
			public static EmailMessage Bind(ExchangeService service, ItemId item, PropertySet ps)
			{
				return new EmailMessage();
			}

			public EmailMessage() {}

			public void Load(PropertySet ps) {}

			public class body
			{
				public string Text { get; set; }
			}

			public body Body { get; set; }

			public List<Attachment> Attachments;
		}

		//Stub class (OEO 2015.03.17)
		public class ItemId
		{
			public ItemId(ItemId item) {}
		}

		//Stub class (OEO 2015.03.17)
		public class PropertySet
		{
			public PropertySet(BasePropertySet bps, ItemSchema itemSchema) {}
		}

		public enum BasePropertySet
		{
			IdOnly,
			FirstClassProperties
		}

		public enum ItemSchema
		{
			Attachments
		}

		//Stub class (OEO 2015.03.17)
		public class FileAttachment : Attachment
		{
			public void Load() {}

			public byte[] Content;
		}

		// POST api/<controller>
		public NewBugResponse App(NewBugRequest bugData)
		{
			try
			{
				AppIdentityToken idtoken = (AppIdentityToken)AuthToken.Parse(bugData.Token);
				idtoken.Validate(new Uri("https://testoutlookaddin.azurewebsites.net/AppRead/Home/Home.html"));

				JsonToken token = TokenValidator.ValidateToken(bugData.Token);
				IdentityTokenRequest tokenRequest = new IdentityTokenRequest();
				tokenRequest.token = bugData.Token;

				IdentityToken identityToken = new IdentityToken(tokenRequest, token.payloadClaims["aud"], token.appContext["amurl"]);

				ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);

				service.Url = new Uri(bugData.EWSUrl);
				service.Credentials = new OAuthCredentials(bugData.AttachmentToken);

				EmailMessage message = EmailMessage.Bind(service, new ItemId(bugData.ItemID), new PropertySet(BasePropertySet.IdOnly, ItemSchema.Attachments));
				message.Load(new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments));

				Uri collectionUri = new Uri("https://dpeted.visualstudio.com/DefaultCollection");
				string projectName = "TED Devices and Services";
				string baseAreaPath = projectName + @"\";

				NetworkCredential netCred = new NetworkCredential(ConfigurationManager.AppSettings["VSOUsername"], ConfigurationManager.AppSettings["VSOPassword"]);
				BasicAuthCredential basicCred = new BasicAuthCredential(netCred);
				TfsClientCredentials tfsCred = new TfsClientCredentials(basicCred);
				tfsCred.AllowInteractive = false;
				var tpc = new TfsTeamProjectCollection(collectionUri, tfsCred);

				tpc.Authenticate();

				var workItemStore = tpc.GetService<WorkItemStore>();
				var teamProject = workItemStore.Projects[projectName];
				var workItemType = teamProject.WorkItemTypes["Bug"];

				// Create the work item
				WorkItem bug = new WorkItem(workItemType)
				{
					Title = bugData.Title,
					AreaPath = baseAreaPath + bugData.Area,
				};


				var reproStepsField = bug.Fields.Cast<Field>().Single(f => f.Name == "Repro Steps");
				reproStepsField.Value = StripHTML(message.Body.Text);


				List<string> filesToDelete = new List<string>();

				var name = Guid.NewGuid() + ".html";
				string htmlFilePath = HttpContext.Current.Server.MapPath("~/" + name);
				using (FileStream fs = File.OpenWrite(htmlFilePath))
				{
					StreamWriter sr = new StreamWriter(fs);
					sr.WriteLine(message.Body.Text);
					fs.Flush();
					filesToDelete.Add(htmlFilePath);
				}


				bug.Attachments.Add(new Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment(htmlFilePath));




				foreach (Attachment attachment in message.Attachments)
				{
					if (bugData.Attachments.Contains(attachment.Id))
					{
						if (attachment is FileAttachment)
						{

							FileAttachment fileAttachment = attachment as FileAttachment;
							fileAttachment.Load();

							string filePath = HttpContext.Current.Server.MapPath("~/" + attachment.Name);
							using (FileStream fs = File.OpenWrite(filePath))
							{
								fs.Write(fileAttachment.Content, 0, fileAttachment.Content.Length);
								fs.Flush();
								filesToDelete.Add(filePath);
							}


							bug.Attachments.Add(new Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment(filePath));

						}
					}
				}
				bug.Fields["Assigned To"].Value = bugData.AssignedTo;
				bug.History = bugData.Notes;


				var validation = bug.Validate();
				bug.Save();

				foreach (string f in filesToDelete)
				{
					try
					{
						File.Delete(f);
					}
					catch (Exception Exception)
					{

					}
				}



				NewBugResponse response = new NewBugResponse();
				response.ID = bug.Id;

				return response;
			}
			catch (Exception ex)
			{
				throw ex;

			}
		}



	}

}

//Stub class OEO 2015.03.17
namespace Microsoft.Exchange.WebServices.Data
{
	public class Attachment
	{
		public Attachment Id;
		public string Name;
	}
}
