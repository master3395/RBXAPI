using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RBXAPI
{
	public class PrivateMessageUser
	{
		public int UserId { get; set; }
		public string UserName { get; set; }
	}
	public class PrivateMessage
	{
		int Id { get; set; }
		public PrivateMessageUser Sender { get; set; }
		public PrivateMessageUser Recipient { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
	}
}
