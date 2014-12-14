using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RBXAPI
{
	public class Group
	{
		private uint _gid;

		public Group(uint GroupId)
		{
			_gid = GroupId;
		}

		public bool SetRole(User Auth, User TargetUser, int RoleSetId)
		{
			if (!Auth.IsLoggedIn)
				throw new InvalidOperationException("The given User is not logged in.");

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(String.Format("http://www.roblox.com/groups/api/change-member-rank?groupId={0}&newRoleSetId={1}&targetUserId={2}", this._gid, RoleSetId, TargetUser.UserId));
			req.Method = "POST";
			req.ContentLength = 0;
			req.CookieContainer = Auth.cookies;
			req.Headers["X-CSRF-TOKEN"] = Auth.XSRFToken;
			req.Headers["X-Requested-With"] = "XMLHttpRequest";
			req.Referer = String.Format("http://www.roblox.com/My/GroupAdmin.aspx?gid={0}", this._gid);
			try
			{
				WebResponse resp = req.GetResponse();
				StreamReader tresp = new StreamReader(resp.GetResponseStream());
				JObject thing = JObject.Parse(tresp.ReadToEnd());
				tresp.Close();
				return thing["success"].Value<bool>();
			}
			catch (WebException e)
			{
				return false;
			}
		}
	}
}
