using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json.Linq;
using Attachment = Microsoft.Exchange.WebServices.Data.Attachment;

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

				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(
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

    #endregion

    internal class JsonToken
	{
		bool isValid;
        readonly Dictionary<string, string> headerClaims;
        readonly Dictionary<string, string> payloadClaims;
        readonly Dictionary<string, string> appContext;

		private void ValidateHeaderClaim(string key, string value)
		{
			if (!headerClaims.ContainsKey(key))
			{
				throw new ApplicationException(String.Format("Header does not contain \"{0}\" claim.", key));
			}

			if (!value.Equals(headerClaims[key]))
			{
				throw new ApplicationException(String.Format("\"{0}\" claim must be \"{0}\".", key, value));
			}
		}

		private void ValidateHeader()
		{
			ValidateHeaderClaim("typ", "JWT");
			ValidateHeaderClaim("alg", "RS256");

			if (!headerClaims.ContainsKey("x5t"))
			{
				throw new ApplicationException(String.Format("Header does not contain \"{0}\" claim.", "x5t"));
			}
		}
		private void ValidateLifetime()
		{
			if (!payloadClaims.ContainsKey("nbf"))
			{
				throw new ApplicationException(
				  String.Format("The \"{0}\" claim is missing from the token.", "nbf"));
			}

			if (!payloadClaims.ContainsKey("exp"))
			{
				throw new ApplicationException(
				  String.Format("The \"{0}\" claim is missing from the token.", "exp"));
			}

			var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			var padding = new TimeSpan(0, 5, 0);

			var validFrom = unixEpoch.AddSeconds(int.Parse(payloadClaims["nbf"]));
			var validTo = unixEpoch.AddSeconds(int.Parse(payloadClaims["exp"]));

			var now = DateTime.UtcNow;

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
			if (!appContext.ContainsKey("amurl"))
			{
				throw new ApplicationException(String.Format("The \"{0}\" claim is missing from the token.", "amurl"));
			}
		}



		private void ValidateAudience()
		{
			if (!payloadClaims.ContainsKey("aud"))
			{
				throw new ApplicationException(String.Format("The \"{0}\" claim is missing from the application context.", "aud"));
			}

		}



		public JsonToken(Dictionary<string, string> header, Dictionary<string, string> payload, string signature)
		{

			// Assume that the token is invalid to start out.
			isValid = false;

			// Set the private dictionaries that contain the claims.
			headerClaims = header;
			payloadClaims = payload;

		    // If there is no "appctx" claim in the token, throw an ApplicationException.
			if (!payloadClaims.ContainsKey("appctx"))
			{
				throw new ApplicationException(String.Format("The {0} claim is not present.", "appctx"));
			}

			appContext = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(payload["appctx"]);


			// Validate the header fields.
			ValidateHeader();

			// Determine whether the token is within its valid time.
			ValidateLifetime();

			// Validate that the token was sent to the correct URL.
			ValidateAudience();

			// Make sure that the appctx contains an authentication
			// metadata location.
			ValidateMetadataLocation();

			// If the token passes all the validation checks, we
			// can assume that it is valid.
			isValid = true;
		}

	}
	internal class TokenValidator
	{
		public static Encoding TextEncoding = Encoding.UTF8;

		const char Base64PadCharacter = '=';
		const char Base64Character62 = '+';
		const char Base64Character63 = '/';
		const char Base64UrlCharacter62 = '-';
		const char Base64UrlCharacter63 = '_';

		private static byte[] DecodeBytes(string arg)
		{
			if (String.IsNullOrEmpty(arg))
			{
				throw new ApplicationException("String to decode cannot be null or empty.");
			}

			var s = new StringBuilder(arg);
			s.Replace(Base64UrlCharacter62, Base64Character62);
			s.Replace(Base64UrlCharacter63, Base64Character63);

			var pad = s.Length % 4;
			s.Append(Base64PadCharacter, (pad == 0) ? 0 : 4 - pad);

			return Convert.FromBase64String(s.ToString());
		}

		private static string Base64Decode(string arg)
		{
			return TextEncoding.GetString(DecodeBytes(arg));
		}
	}
	public abstract class AppController
	{
		static string StripHtml(string source)
		{
			// Remove HTML Development formatting
			// Replace line breaks with space
			// because browsers inserts space
			var result = source.Replace("\r", " ");
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
			var breaks = "\r\r\r";
			// Initial replacement target string for tabs
			var tabs = "\t\t\t\t\t";
			for (var index = 0; index < result.Length; index++)
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
			public int Id { get; set; }
		}

		//Stub class (OEO 2015.03.17)
		public abstract class NewBugRequest
		{
			public readonly string Token;
			public readonly string EwsUrl;
			public readonly string AttachmentToken;
			public ItemId ItemId { get; set; }
			public readonly string Title;
			public readonly string Area;
			public readonly string AssignedTo;
			public readonly string Notes;
			public readonly List<Attachment> Attachments;

		    protected NewBugRequest(string token, string ewsUrl, string attachmentToken, string title, string area, string assignedTo, string notes, List<Attachment> attachments)
		    {
		        Token = token;
		        EwsUrl = ewsUrl;
		        AttachmentToken = attachmentToken;
		        Title = title;
		        Area = area;
		        AssignedTo = assignedTo;
		        Notes = notes;
		        Attachments = attachments;
		    }
		}

		//Stub class (OEO 2015.03.17)
		class AppIdentityToken
		{
			public static void Validate(Uri uri) {}
		}

		//Stub class (OEO 2015.03.17)
		class AuthToken { public static object Parse(string token) {return token;} }

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
			Exchange2007Sp1
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

			public static void Load(PropertySet ps) {}

			public abstract class body
			{
			    protected body(string text)
			    {
			        Text = text;
			    }

			    public string Text { get; private set; }
			}

			public body Body { get; private set; }

			public readonly List<Attachment> Attachments;

		    EmailMessage(body body, List<Attachment> attachments)
		    {
		        Body = body;
		        Attachments = attachments;
		    }
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
		class FileAttachment : Attachment
		{
			public void Load() {}

			public byte[] Content;
		}

		// POST api/<controller>
		public NewBugResponse App(NewBugRequest bugData)
		{
			var idtoken = (AppIdentityToken)AuthToken.Parse(bugData.Token);
			AppIdentityToken.Validate(new Uri("https://testoutlookaddin.azurewebsites.net/AppRead/Home/Home.html"));

			var service = new ExchangeService(ExchangeVersion.Exchange2007Sp1)
			{
				Url = new Uri(bugData.EwsUrl),
				Credentials = new OAuthCredentials(bugData.AttachmentToken)
			};


			var message = EmailMessage.Bind(service, new ItemId(bugData.ItemId), new PropertySet(BasePropertySet.IdOnly, ItemSchema.Attachments));
			EmailMessage.Load(new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments));

			var collectionUri = new Uri("https://dpeted.visualstudio.com/DefaultCollection");
			const string projectName = "TED Devices and Services";
			const string baseAreaPath = projectName + @"\";

			var netCred = new NetworkCredential(ConfigurationManager.AppSettings["VSOUsername"], ConfigurationManager.AppSettings["VSOPassword"]);
			var basicCred = new BasicAuthCredential(netCred);
			var tfsCred = new TfsClientCredentials(basicCred) {AllowInteractive = false};
			var tpc = new TfsTeamProjectCollection(collectionUri, tfsCred);

			tpc.Authenticate();

			var workItemStore = tpc.GetService<WorkItemStore>();
			var teamProject = workItemStore.Projects[projectName];
			var workItemType = teamProject.WorkItemTypes["Bug"];

			// Create the work item
			var bug = new WorkItem(workItemType)
			{
				Title = bugData.Title,
				AreaPath = baseAreaPath + bugData.Area,
			};


			var reproStepsField = bug.Fields.Cast<Field>().Single(f => f.Name == "Repro Steps");
			reproStepsField.Value = StripHtml(message.Body.Text);


			var filesToDelete = new List<string>();

			var name = Guid.NewGuid() + ".html";
			var htmlFilePath = HttpContext.Current.Server.MapPath("~/" + name);
			using (var fs = File.OpenWrite(htmlFilePath))
			{
				var sr = new StreamWriter(fs);
				sr.WriteLine(message.Body.Text);
				fs.Flush();
				filesToDelete.Add(htmlFilePath);
			}


			bug.Attachments.Add(new Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment(htmlFilePath));




			foreach (var attachment in message.Attachments)
			{
				if (!bugData.Attachments.Contains(attachment.Id)) continue;
				if (!(attachment is FileAttachment)) continue;
				var fileAttachment = attachment as FileAttachment;
				fileAttachment.Load();

				var filePath = HttpContext.Current.Server.MapPath("~/" + attachment.Name);
				using (var fs = File.OpenWrite(filePath))
				{
					fs.Write(fileAttachment.Content, 0, fileAttachment.Content.Length);
					fs.Flush();
					filesToDelete.Add(filePath);
				}


				bug.Attachments.Add(new Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment(filePath));
			}
			bug.Fields["Assigned To"].Value = bugData.AssignedTo;
			bug.History = bugData.Notes;


			bug.Validate();
			bug.Save();

			foreach (var f in filesToDelete) File.Delete(f); 

			return new NewBugResponse { Id = bug.Id };
		}



	}

}

//Stub class OEO 2015.03.17
namespace Microsoft.Exchange.WebServices.Data
{
	public abstract class Attachment
	{
		public readonly Attachment Id;
		public readonly string Name;

	    protected Attachment(Attachment id, string name)
	    {
	        Id = id;
	        Name = name;
	    }
	}
}
