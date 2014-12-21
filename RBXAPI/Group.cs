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
		internal uint _gid;
		internal string _gname;
		internal List<GroupRole> _cachedRoles;

		#region Constructors
		public Group(uint GroupId)
		{
			_gid = GroupId;
		}
		public Group(string GroupName)
		{
			_gname = GroupName;
		}
		#endregion

		#region Properties
		public uint GroupId
		{
			get { return (_gid < 1 ? GetGroupId() : _gid); }
		}
		public string Groupname
		{
			get { return (_gname ?? GetInfo<string>("Name")); }
		}
		public User Owner
		{
			get
			{
				JObject owner = GetInfo<JObject>("Owner");
				return new User(owner["Id"].Value<uint>());
			}
		}
		public string GroupEmblem
		{
			get { return GetInfo<string>("EmblemUrl"); }
		}
		public string Description
		{
			get { return GetInfo<string>("Description"); }
		}
		public List<GroupRole> Roles
		{
			get { return _cachedRoles ?? GetGroupRoles(); }
		}
		#endregion

		#region Methods
		public bool SetRole(User Auth, User TargetUser, GroupRole TargetRole)
		{
			if (!Auth.IsLoggedIn)
				throw new InvalidOperationException("The `Auth` User is not logged in.");

			if (TargetRole._gid != this._gid)
				throw new InvalidOperationException("The target rank is not in this group!");

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(String.Format("http://www.roblox.com/groups/api/change-member-rank?groupId={0}&newRoleSetId={1}&targetUserId={2}", this._gid, TargetRole.RolesetId, TargetUser.UserId));
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
			catch (WebException)
			{
				return false;
			}
		}
		#endregion

		#region Property Methods
		private uint GetGroupId()
		{
			throw new NotImplementedException();
		}
		private T GetInfo<T>(string key)
		{
			WebRequest req = WebRequest.Create(String.Format("http://api.roblox.com/groups/{0}", this.GroupId));
			WebResponse resp = req.GetResponse();
			StreamReader tresp = new StreamReader(resp.GetResponseStream());
			JObject response = JObject.Parse(tresp.ReadToEnd());
			tresp.Close();

			return response[key].Value<T>();
		}
		private List<GroupRole> GetGroupRoles()
		{
			List<GroupRole> ret = new List<GroupRole>();
			WebRequest req = WebRequest.Create(String.Format("http://www.roblox.com/api/groups/{0}/RoleSets/", this.GroupId));
			WebResponse resp = req.GetResponse();
			StreamReader tresp = new StreamReader(resp.GetResponseStream());
			JArray response = JArray.Parse(tresp.ReadToEnd());
			tresp.Close();

			foreach (JObject item in response)
			{
				GroupRole role = new GroupRole(this.GroupId);
				role.Name = item["Name"].Value<string>();
				role.Rank = item["Rank"].Value<byte>();
				role.RolesetId = item["ID"].Value<uint>();
				Console.WriteLine(String.Format("\tName: {0}\n\tRank: {1}\n\tRolesetId: {2}", role.Name, role.Rank, role.RolesetId));
				ret.Add(role);
			}
			_cachedRoles = ret;
			return ret;
		}
		#endregion
	}
}
