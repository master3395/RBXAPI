using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RBXAPI;

namespace RBXAPI.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			User login = new User("Shedletsky", "hunter2");
			try
			{
				login.Login();
			}
			catch (InvalidOperationException e)
			{
				Console.WriteLine("ERROR: {0}", e.Message);
			}
			Console.Write("Logged in: ");
			Console.WriteLine(login.IsLoggedIn);
			Group TheGroup = new Group(1);
			Console.Write("Setting rank: ");
			Console.WriteLine(TheGroup.SetRole(login, new User("digpoe"), GroupRole.ByName(TheGroup, "Member")));
			Console.Write("Posting to group wall: ");
			Console.WriteLine(login.PostToGroupWall(TheGroup, "This was posted by my C# Assembly, which wraps the ROBLOX API. Hi."));
			Console.Write("Primary Group ID: ");
			Console.WriteLine(login.PrimaryGroup.GroupId);
			Console.Read();
		}
	}
}
