<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="sample._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Online Trip Guider</h1>
        <p class="lead">This is a sample web app developed using ASp.NET to demostrate capabilities of .NET Agents for Identity Server.</p>

        <% if (Claims != null)
           {%>
                <p><a href="samllogout" class="btn btn-primary btn-lg">Logout &raquo;</a></p>
        <% }
           else
           {%>
                <p><a href="samlsso" class="btn btn-primary btn-lg">Login &raquo;</a></p>
        <% }%>
    </div>
    <% if(Claims != null)
       { %>
            <div class="jumbotron">
                <p>You have successfully Logged In. Following claims were recieved. <br/>
                    <% foreach (var claim in Claims)
                       {%>
                            <%=claim.Key + " = " + claim.Value%><br/>
                    <% }%>
                </p>
            </div>
    <% }%>
</asp:Content>
