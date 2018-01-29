<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Hello.aspx.cs" Inherits="Sample2.Hello" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    
<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js" type="text/javascript"></script>
<script type = "text/javascript">
    var xhr;
    var loggedIn ="false";

    function CheckIfLoggedIn() {
        xhr = $.ajax({
            type: "POST",
            url: "CheckSession.asmx/GetSessionValue",
            data: '{name: "hello" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            complete: function () { setTimeout(CheckIfLoggedIn,4000) },
            failure: function(response) {
                alert(response.d);
            }
        });
    }

    function OnSuccess(response) {
        var resp = response.d;
        console.log(response.d);

        if (loggedIn == "false" && resp == "true") {
            loggedIn = "true";
            console.log("logged In");
        }

        else if (loggedIn == "true" && resp== "false") {
            console.log("logged Out");

            PerformRedirect();           
        }
    }

    

    function PerformRedirect() {
        //xhr.abort();
        window.location.href = "https://www.google.com";
    }

    function SetterFunction() {
        xhr = $.ajax({
            type: "POST",
            url: "CheckSession.asmx/SetSessionValue",
            data: '{name: "hello" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccessDoer,
            failure: function (response) {
                alert(response.d);
            }
        });
    }

    function OnSuccessDoer(response) {
        console.log("in success doer");
        console.log(response.d);
    }

</script>

</head>
<body>
    <form id="form1" runat="server">
        
    <div>
Your Name :
<asp:TextBox ID="txtUserName" runat="server"></asp:TextBox>
<input id="btnGetTime" type="button" value="Show Current Time"
    onclick = "CheckIfLoggedIn();" />
</div>
        

<div>
setter
<input id="btnSetter" type="button" value="Set"
    onclick = "SetterFunction();" />
</div>


    </form>
</body>
</html>
