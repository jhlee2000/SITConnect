<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SITConnect.Login" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" />
    <script src="https://www.google.com/recaptcha/api.js?render=6LcIiT8aAAAAAEDLc7hahmnfKeqn1YeHLKF6TIFG"></script>
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
            <h2 class="text-center">Login</h2>
            
            <div class="form-group">
                <asp:TextBox CssClass="form-control" ID="tbEmail" placeholder="Email" runat="server" required="required"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:TextBox CssClass="form-control" ID="tbPassword" placeholder="Password" runat="server" required="required" TextMode="Password"></asp:TextBox>
            </div>
            <div>
                <asp:Button CssClass="btn btn-primary btn-block" ID="btnLogin" OnClick="btnLogin_Click" Text="Login" runat="server"/>
            </div>
            <div class="clearfix">
                <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/Registration.aspx">Create Account</asp:HyperLink>
            </div>
            <div class="form-group">
                <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response" />
            </div>
            <div class="form-group">
                <br />
                <asp:Label ID="lbMsg" runat="server" Font-Bold="True" ForeColor="#8E1600"></asp:Label>
            </div>
        </form>
    </div>
    <script src="https://code.jquery.com/jquery-3.3.1.slim.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('6LcIiT8aAAAAAEDLc7hahmnfKeqn1YeHLKF6TIFG', { action: 'Login' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>
</body>
</html>
