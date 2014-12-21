using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RBXAPI
{
	public class User
	{
		private string _uname;
		private string _pword;
		private uint _uid;
		private bool _canLogIn;
		private string _xsrf;
		internal CookieContainer cookies;

		#region Constructors
		public User(uint UserId)
		{
			_uid = UserId;
			_canLogIn = false;
		}
		public User(uint UserId, string Password)
		{
			_uid = UserId;
			_pword = Password;
			_canLogIn = true;
			cookies = new CookieContainer();
		}

		public User(string Username)
		{
			if (Username.Length > 20)
			{
				throw new InvalidOperationException("Usernames can only be 20 characters!");
			}
			_uname = Username;
			_canLogIn = false;
		}

		public User(string Username, string Password)
		{
			if (Username.Length > 20)
			{
				throw new InvalidOperationException("Usernames can only be 20 characters!");
			}
			_uname = Username;
			_pword = Password;
			_canLogIn = true;
			cookies = new CookieContainer();
		}
		#endregion

		#region Properties
		public bool IsLoggedIn
		{
			get { return _canLogIn && CheckLoggedIn(); }
		}
		public string ROBLOSECURITY
		{
			get
			{
				if (_canLogIn && IsLoggedIn)
					return cookies.GetCookies(new Uri("http://www.roblox.com"))[".ROBLOSECURITY"].Value;

				return default(string);
			}
		}
		public string XSRFToken
		{
			get
			{
				return _xsrf ?? GetXRSFToken(); 
			}
		}
		public string Username
		{
			get { return _uname ?? GetUserName(); }
		}
		public uint UserId
		{
			get { return (_uid < 1 ? GetUserId() : _uid); }
		}
		public TimeSpan AccountAge
		{
			get { return GetAccountAge(); }
		}
		public Group PrimaryGroup
		{
			get { return GetPrimaryGroup(); }
		}
		public string AvatarThumbnail
		{
			get { return GetAvatarThumbnail(); }
		}
		#endregion

		#region Methods
		public void Login(string pw = null)
		{
			if (!_canLogIn && pw == null)
				throw new InvalidOperationException("This user cannot be logged in!");

			string json = JObject.FromObject(new
			{
				Username = _uname,
				Password = _pword ?? pw
			}).ToString();

			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://www.roblox.com/NewLogin");
			req.Method = "POST";
			req.UserAgent = "ROBLOX iOS";
			req.ContentType = "application/json";
			req.ContentLength = json.Length;
			req.CookieContainer = cookies;
			StreamWriter body = new StreamWriter(req.GetRequestStream());
			body.Write(json);
			body.Close();
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			cookies.Add(resp.Cookies);

			if (cookies.GetCookies(resp.ResponseUri)[".ROBLOSECURITY"] == null)
			{
				throw new InvalidOperationException("The provided password is incorrect, or ROBLOX is experiencing technical issues.");
			}
			resp.Close();
		}

		public bool IsFriendsWith(User Other)
		{
			WebRequest req = WebRequest.Create(String.Format("http://www.roblox.com/Game/LuaWebService/HandleSocialRequest.ashx?method=IsFriendsWith&playerId={0}&userId={1}", this.UserId, Other.UserId));
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			StreamReader rstr = new StreamReader(resp.GetResponseStream());
			string response = rstr.ReadToEnd();
			rstr.Close();
			response = response.Split('>')[1].Split('<')[0];
			return bool.Parse(response);
		}

		public bool IsBestFriendsWith(User Other)
		{
			WebRequest req = WebRequest.Create(String.Format("http://www.roblox.com/Game/LuaWebService/HandleSocialRequest.ashx?method=IsBestFriendsWith&playerId={0}&userId={1}", this.UserId, Other.UserId));
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			StreamReader rstr = new StreamReader(resp.GetResponseStream());
			string response = rstr.ReadToEnd();
			rstr.Close();
			response = response.Split('>')[1].Split('<')[0];
			return bool.Parse(response);
		}

		public bool SendPM(User Other, string subject, string body)
		{
			if (!this.IsLoggedIn)
				throw new InvalidOperationException("Unauthorized action");

			string wbody = String.Format("subject={0}&body={1}&recipientid={2}", Uri.EscapeDataString(subject), Uri.EscapeDataString(body), Other.UserId);
			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://www.roblox.com/messages/send");
			req.CookieContainer = cookies;
			req.Method = "POST";
			req.Headers["X-CSRF-TOKEN"] = this.XSRFToken;
			req.Headers["X-Requested-With"] = "XMLHttpRequest";
			req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			req.Referer = String.Format("http://www.roblox.com/My/NewMessage.aspx?RecipientID={0}", Other.UserId);
			req.ContentLength = wbody.Length;

			StreamWriter tbody = new StreamWriter(req.GetRequestStream());
			tbody.Write(wbody);
			tbody.Close();
			try
			{
				HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
				if (cookies != null)
					cookies.Add(resp.Cookies);
				StreamReader tresp = new StreamReader(resp.GetResponseStream());
				JObject response = JObject.Parse(tresp.ReadToEnd());
				tresp.Close();
				return response["success"].Value<bool>();
			}
			catch (WebException)
			{
				return false;
			}
		}

		private const string GetMessagesQueryString = "messageTab=0&pageNumber={1}&pageSize={0}";
		private object[] GetPMs(int MessagesPerPage, int PageNumber)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(String.Format("http://www.roblox.com/messages/api/get-messages?{0}", String.Format(GetMessagesQueryString, MessagesPerPage, PageNumber)));
			req.CookieContainer = cookies;
			req.Referer = "http://www.roblox.com/my/messages/";

			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			StreamReader tresp = new StreamReader(resp.GetResponseStream());
			string rstring = tresp.ReadToEnd();
			tresp.Close();

			JObject response = JObject.Parse(rstring);
			JArray pms = (JArray)response["Collection"];

			return new object[]{
				response["TotalPages"].Value<int>(),
				pms
			};
		}
		public List<PrivateMessage> GetPMs(int MessagesPerPage = 20)
		{
			if (!this.IsLoggedIn)
				throw new InvalidOperationException("Unauthorized action");
			List<PrivateMessage> ret = new List<PrivateMessage>();
			object[] resp = GetPMs(MessagesPerPage, 0);
			int numPages = (int)resp[0];
			JArray pms = (JArray)resp[1];
			foreach (JObject pm in pms)
			{
				ret.Add(pm.ToObject<PrivateMessage>());
			}
			for (int i = 1; i < numPages; i++)
			{
				object[] i_resp = GetPMs(MessagesPerPage, i);
				JArray i_pms = (JArray)i_resp[1];
				foreach (JObject pm in i_pms)
				{
					ret.Add(pm.ToObject<PrivateMessage>());
				}
			}

			return ret;
		}

		private const string PostToGroupWallFormData = "__EVENTTARGET={0}&__EVENTARGUMENT={1}&__LASTFOCUS={2}&__VIEWSTATE={3}&__VIEWSTATEGENERATOR={4}&__PREVIOUSPAGE={5}&__EVENTVALIDATION={6}&__RequestVerificationToken={7}&ctl00%24cphRoblox%24GroupWallPane%24NewPost={8}&ctl00%24cphRoblox%24GroupWallPane%24NewPostButton=Post";
		public bool PostToGroupWall(Group tGroup, string message)
		{
			if (!this.IsLoggedIn)
				throw new InvalidOperationException("Unauthorized action");
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(String.Format("http://www.roblox.com/My/Groups.aspx?gid={0}", tGroup._gid));
			req.CookieContainer = cookies;
			WebResponse resp = req.GetResponse();
			StreamReader rstream = new StreamReader(resp.GetResponseStream());
			string response = rstream.ReadToEnd();
			rstream.Close();

			Match EventTarget              = Regex.Match(response, "<input type=\"hidden\" name=\"__EVENTTARGET\" id=\"__EVENTTARGET\" value=\"(.*)\" />");
			string EventTargetText         = EventTarget.Success ? EventTarget.Groups[1].Value : "";

			Match EventArgument            = Regex.Match(response, "<input type=\"hidden\" name=\"__EVENTARGUMENT\" id=\"__EVENTARGUMENT\" value=\"(.*)\" />");
			string EventArgumentText       = EventArgument.Success ? EventArgument.Groups[1].Value : "";

			Match LastFocus                = Regex.Match(response, "<input type=\"hidden\" name=\"__LASTFOCUS\" id=\"__LASTFOCUS\" value=\"(.*)\" />");
			string LastFocusText           = LastFocus.Success ? LastFocus.Groups[1].Value : "";

			Match ViewState                = Regex.Match(response, "<input type=\"hidden\" name=\"__VIEWSTATE\" id=\"__VIEWSTATE\" value=\"(.*)\" />");
			string ViewStateText           = ViewState.Success ? ViewState.Groups[1].Value : "";

			Match ViewStateGen             = Regex.Match(response, "<input type=\"hidden\" name=\"__VIEWSTATEGENERATOR\" id=\"__VIEWSTATEGENERATOR\" value=\"(.*)\" />");
			string ViewStateGenText        = ViewStateGen.Success ? ViewStateGen.Groups[1].Value : "";

			Match PreviousPage             = Regex.Match(response, "<input type=\"hidden\" name=\"__PREVIOUSPAGE\" id=\"__PREVIOUSPAGE\" value=\"(.*)\" />");
			string PreviousPageText        = PreviousPage.Success ? PreviousPage.Groups[1].Value : "";

			Match EventValidation          = Regex.Match(response, "<input type=\"hidden\" name=\"__EVENTVALIDATION\" id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
			string EventValidationText     = EventValidation.Success ? EventValidation.Groups[1].Value : "";

			Match RequestVerification      = Regex.Match(response, "<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(.*)\" />");
			string RequestVerificationText = RequestVerification.Success ? RequestVerification.Groups[1].Value : "";


			String form = String.Format(PostToGroupWallFormData, Uri.EscapeDataString(EventTargetText), Uri.EscapeDataString(EventArgumentText),
				Uri.EscapeDataString(LastFocusText), Uri.EscapeDataString(ViewStateText), Uri.EscapeDataString(ViewStateGenText), Uri.EscapeDataString(PreviousPageText),
				Uri.EscapeDataString(EventValidationText), Uri.EscapeDataString(RequestVerificationText), Uri.EscapeDataString(message));
			HttpWebRequest req2 = (HttpWebRequest)WebRequest.Create(String.Format("http://www.roblox.com/My/Groups.aspx?gid={0}", tGroup._gid));
			req2.CookieContainer = cookies;
			req2.Method = "POST";
			req2.ContentType = "application/x-www-form-urlencoded";
			req2.ContentLength = form.Length;
			StreamWriter body = new StreamWriter(req2.GetRequestStream());
			body.Write(form);
			body.Close();

			try
			{
				HttpWebResponse resp2 = (HttpWebResponse)req2.GetResponse();
				if (cookies != null)
					cookies.Add(resp2.Cookies);
				return resp2.StatusCode == HttpStatusCode.OK;
			}
			catch (WebException)
			{
				return false;
			}
		}
		#endregion

		#region Property Methods
		private bool CheckLoggedIn()
		{
			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://www.roblox.com/home");
			req.CookieContainer = cookies;
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			Cookie rsec = cookies.GetCookies(resp.ResponseUri)[".ROBLOSECURITY"];
			resp.Close();
			if (rsec == null || rsec.Expired)
				return false;

			return true;
		}
		private string GetXRSFToken()
		{
			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://www.roblox.com/home");
			req.CookieContainer = cookies;
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			StreamReader e = new StreamReader(resp.GetResponseStream());
			string page = e.ReadToEnd();
			e.Close();
			page = page.Split(new string[]{"Roblox.XsrfToken.setToken('"}, 2, StringSplitOptions.None)[1];//.Split("');")[0];
			page = page.Split(new string[] { "');" }, 2, StringSplitOptions.None)[0];
			_xsrf = page;
			return page;
		}
		private string GetUserName()
		{
			WebRequest req = WebRequest.Create("http://api.roblox.com/users/" + this._uid);
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			StreamReader e = new StreamReader(resp.GetResponseStream());
			JObject udata = JObject.Parse(e.ReadToEnd());
			_uname = udata["Username"].Value<string>();
			return udata["Username"].Value<string>();
		}
		private uint GetUserId()
		{
			WebRequest req = WebRequest.Create("http://api.roblox.com/users/get-by-username?username=" + this.Username);
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			StreamReader e = new StreamReader(resp.GetResponseStream());
			JObject udata = JObject.Parse(e.ReadToEnd());
			_uid = udata["Id"].Value<uint>();
			return udata["Id"].Value<uint>();
		}
		private Group GetPrimaryGroup()
		{
			//throw new NotImplementedException();
			//return new Group(0);
			WebRequest req = WebRequest.Create(String.Format("http://www.roblox.com/Groups/GetPrimaryGroupInfo.ashx?users={0}", this.Username));
			WebResponse rsp = req.GetResponse();
			StreamReader resp = new StreamReader(rsp.GetResponseStream());
			JObject response = JObject.Parse(resp.ReadToEnd());
			resp.Close();
			return new Group(response[this.Username]["GroupId"].Value<uint>());
		}
		private TimeSpan GetAccountAge()
		{
			WebRequest req = WebRequest.Create("http://www.roblox.com/Contests/Handlers/Showcases.ashx?userId=" + this.UserId);
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			using (Stream respStream = resp.GetResponseStream())
			{
				using (StreamReader reader = new StreamReader(respStream))
				{
					JObject data = JObject.Parse(reader.ReadToEnd());
					JObject oldest = (JObject)((JArray)data["Showcase"])[data["Showcase"].Count()-1];
					string id = (string)oldest["ID"];

					WebRequest req2 = WebRequest.Create("http://api.roblox.com/marketplace/productinfo?assetId=" + id);
					HttpWebResponse resp2 = (HttpWebResponse)req.GetResponse();
					if (cookies != null)
						cookies.Add(resp2.Cookies);
					using (Stream resp2Stream = resp2.GetResponseStream())
					{
						using (StreamReader reader2 = new StreamReader(resp2Stream))
						{
							string joutput = reader2.ReadToEnd();
							JObject placedata = JObject.Parse(joutput);
							DateTime created = placedata["Created"].Value<DateTime>();
							//Console.WriteLine("\"{0}\"", created);

							return (DateTime.Now - created);
						}
					}
				}
			}
		}

		private string GetAvatarThumbnail()
		{
			WebRequest req = WebRequest.Create("http://www.roblox.com/Thumbs/Avatar.ashx?userId=" + this.UserId);
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			if (cookies != null)
				cookies.Add(resp.Cookies);
			return resp.ResponseUri.ToString();
		}
		#endregion
	}
}
