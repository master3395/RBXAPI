using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RBXAPI
{
	public class GroupRole
	{
		internal uint _gid;
		#region Constructors
		internal GroupRole(uint gid)
		{
			_gid = gid;
		}
		#endregion
		#region Properties
		public string Name { get; internal set; }
		public byte Rank { get; internal set; }
		public uint RolesetId { get; internal set; }
		#endregion
		#region Methods
		public static GroupRole ByName(Group tGroup, string Name)
		{
			List<GroupRole> roles = tGroup.Roles;
			try
			{
				return roles.First(x => x.Name == Name);
			}
			catch (InvalidOperationException e)
			{
				throw new InvalidOperationException(String.Format("The role by name `{0}` does not exist.", Name), e);
			}
		}
		public static GroupRole ByRank(Group tGroup, byte Rank)
		{
			List<GroupRole> roles = tGroup.Roles;
			try
			{
				return roles.First(x => x.Rank == Rank);
			}
			catch (InvalidOperationException e)
			{
				throw new InvalidOperationException(String.Format("The role by rank `{0}` does not exist.", Rank), e);
			}
		}
		public static GroupRole ByRoleSetId(Group tGroup, uint RolesetId)
		{
			List<GroupRole> roles = tGroup.Roles;
			try
			{
				return roles.First(x => x.RolesetId == RolesetId);
			}
			catch (InvalidOperationException e)
			{
				throw new InvalidOperationException(String.Format("The role by id `{0}` does not exist.", RolesetId), e);
			}
		}
		#endregion
	}
}
