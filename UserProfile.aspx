<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserProfile.aspx.cs" Inherits="SITConnect.UserProfile" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" />
    <style>
	.login-form {
		width: 440px;
    	margin: 50px auto;
	}
    .login-form form {
    	margin-bottom: 15px;
        background: #f7f7f7;
        box-shadow: 0px 2px 2px rgba(0, 0, 0, 0.3);
        padding: 30px;
    }
    .login-form h2 {
        margin: 0 0 15px;
    }
    .form-control, .btn {
        min-height: 38px;
        border-radius: 2px;
    }
    .btn {        
        font-size: 15px;
        font-weight: bold;
    }
</style>
</head>
<body>
    <div class="login-form">
        <form id="form1" runat="server">
            <h2 class="text-center">User Profile</h2>
            <div class="form-group">
                <asp:Label ID="Label1" runat="server" Text="Email:"></asp:Label>
                <asp:Label ID="lbEmail" runat="server" Text=""></asp:Label>
            </div>
            <div class="form-group">
                <asp:Label ID="Label2" runat="server" Text="Credit Card Info:"></asp:Label>
                <asp:Label ID="lbCCInfo" runat="server" Text=""></asp:Label>
            </div>
            <div>
                <asp:Button CssClass="btn btn-primary btn-block" ID="btnLogout" runat="server" OnClick="btnLogout_Click" Text="Logout" />
            </div>
            <br />
            <br />
            <h2 class="text-center">Change Password</h2>
            <div class="form-group">
                <asp:Button CssClass="btn btn-primary btn-block" ID="btnChangePassword" runat="server" OnClick="btnChangePassword_Click" Text="Change Password" />
            </div>
            <%--<div class="clearfix">
                <a href="#" class="pull-right">Create Account</a>
            </div>--%>
        </form>
    </div>
    <script src="https://code.jquery.com/jquery-3.3.1.slim.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
</body>
</html>
