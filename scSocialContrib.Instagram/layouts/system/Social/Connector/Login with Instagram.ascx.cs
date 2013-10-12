using System;
using System.Web.UI;
using Sitecore.Social.Core.Connector;

namespace scSocialContrib.Instagram.layouts.system.Social.Connector
{
	public partial class Login_with_Instagram : System.Web.UI.UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void InstagramLoginButtonOneClick(object sender, ImageClickEventArgs e)
		{
			var connectUserManager = new ConnectUserManager();
			const bool IsAsyncProfileUpdate = true; 

			if (Sitecore.Context.User.IsAuthenticated)
			{
				connectUserManager.AttachUser("Instagram", IsAsyncProfileUpdate);
			}
			else
			{
				connectUserManager.LoginUser("Instagram", IsAsyncProfileUpdate);	
			}
			
		}
	}
}