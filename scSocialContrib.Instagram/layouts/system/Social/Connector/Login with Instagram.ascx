<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login with Instagram.ascx.cs" Inherits="scSocialContrib.Instagram.layouts.system.Social.Connector.Login_with_Instagram" %>
<link rel="stylesheet" type="text/css" href='<%= ResolveUrl("~/layouts/system/Social/Connector/Style.css") %>' />
<asp:ImageButton runat="server" ID="instagramLoginButton" src="/sitecore/shell/Themes/Standard/Custom/24x24/Instagram.png"
  CssClass="button" ToolTip="Login with Instagram" OnClick="InstagramLoginButtonOneClick" />