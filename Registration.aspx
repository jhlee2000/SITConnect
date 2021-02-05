<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="SITConnect.Registration" ValidateRequest="false"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Registration</title>
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" />
    <%-- Client-Side Password Validation --%>
    <script type="text/javascript">
        function validate() {
            var str = document.getElementById('<%=tbPassword.ClientID %>').value;

            if (str.length < 8) {
                document.getElementById("lbl_pwdchecker").innerHTML = "Password Length Must be at Least 8 Characters";
                document.getElementById("lbl_pwdchecker").style.color = "Red";
                return ("too_short");
            } else if (str.search(/[0-9]/) == -1) {
                document.getElementById("lbl_pwdchecker").innerHTML = "Password require at least 1 number";
                document.getElementById("lbl_pwdchecker").style.color = "Red";
                return ("no_number");
            } else if (str.search(/[A-Z]/) == -1) {
                document.getElementById("lbl_pwdchecker").innerHTML = "Password require at least 1 Uppercase character";
                document.getElementById("lbl_pwdchecker").style.color = "Red";
                return ("no_upperchar");
            } else if (str.search(/[a-z]/) == -1) {
                document.getElementById("lbl_pwdchecker").innerHTML = "Password require at least 1 Lowercase character";
                document.getElementById("lbl_pwdchecker").style.color = "Red";
                return ("no_lowerchar");
            } else if (str.search(/[^A-Za-z0-9]/) == -1) {
                document.getElementById("lbl_pwdchecker").innerHTML = "Password require at least 1 special case character";
                document.getElementById("lbl_pwdchecker").style.color = "Red";
                return ("no_specialchar");
            }
            //TODO: Might want to remove this excellent
            document.getElementById("lbl_pwdchecker").innerHTML = "Excellent!";
            document.getElementById("lbl_pwdchecker").style.color = "Blue";

        }
    </script>
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
            <h2 class="text-center">Registration</h2>
            <div class="form-group">
                <asp:Label ID="lbError" runat="server" Font-Bold="True" Font-Size="X-Large"></asp:Label>
                <br />
            </div>
            <div class="form-group">
                <asp:TextBox CssClass="form-control" ID="tbFirstName" placeholder="First Name" runat="server" required="required"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:TextBox CssClass="form-control" ID="tbLastName" placeholder="Last Name" runat="server" required="required"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:TextBox CssClass="form-control" ID="tbCCInfo" placeholder="Credit Card Info" runat="server" required="required"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:TextBox CssClass="form-control" ID="tbEmail" placeholder="Email" runat="server" required="required" TextMode="Email"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:TextBox CssClass="form-control" onkeyup="javascript:validate()" ID="tbPassword" placeholder="Password" runat="server" required="required" TextMode="Password"></asp:TextBox>
                <asp:Label ID="lbl_pwdchecker" runat="server"></asp:Label>     
                <br />
            </div>
            <div class="form-group">
                <asp:Label ID="Label1" runat="server">Birthdate:</asp:Label>
                <asp:TextBox CssClass="form-control" ID="tbDob" placeholder="Birth Date" runat="server" required="required" TextMode="Date"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Button CssClass="btn btn-primary btn-block" ID="btnRegister" runat="server" OnClick="btnRegister_Click" Text="Register" />
            </div>
            
            <div class="clearfix">
                <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/Login.aspx">Back</asp:HyperLink>
            </div>
        </form>
    </div>
    <script src="https://code.jquery.com/jquery-3.3.1.slim.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
</body>
</html>
