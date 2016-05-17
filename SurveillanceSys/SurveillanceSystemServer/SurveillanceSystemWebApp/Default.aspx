<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SurveillanceSystemWebApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
   
     <!--
    <div class="jumbotron">
        <h1>ASP.NET</h1>
        <p class="lead">ASP.NET is a free web framework for building great Web sites and Web applications using HTML, CSS, and JavaScript.</p>
        <p><a href="http://www.asp.net" class="btn btn-primary btn-lg">Learn more &raquo;</a></p>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Getting started</h2>
            <p>
                ASP.NET Web Forms lets you build dynamic websites using a familiar drag-and-drop, event-driven model.
            A design surface and hundreds of controls and components let you rapidly build sophisticated, powerful UI-driven sites with data access.
            </p>
            <p>
                <a class="btn btn-default" href="http://go.microsoft.com/fwlink/?LinkId=301948">Learn more &raquo;</a>
            </p>
        </div>
        <div class="col-md-4">
            <h2>Get more libraries</h2>
            <p>
                NuGet is a free Visual Studio extension that makes it easy to add, remove, and update libraries and tools in Visual Studio projects.
            </p>
            <p>
                <a class="btn btn-default" href="http://go.microsoft.com/fwlink/?LinkId=301949">Learn more &raquo;</a>
            </p>
        </div>
        <div class="col-md-4">
            <h2>Web Hosting</h2>
            <p>
                You can easily find a web hosting company that offers the right mix of features and price for your applications.
            </p>
            <p>
                <a class="btn btn-default" href="http://go.microsoft.com/fwlink/?LinkId=301950">Learn more &raquo;</a>
            </p>
        </div>
        
    </div>
    -->
    <div class="container">
        <div class="row">
            
            <div>
                <asp:DropDownList CssClass="col-xs-12 col-sm-8 col-sm-offset-2 col-md-4 col-md-offset-4 " ID="DropDownList1" runat="server" DataSourceID="SqlDataSource1" DataTextField="Time" DataValueField="Time" OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged" AutoPostBack="True">
            </asp:DropDownList> 
            </div>      
                          
        </div>
       
        
        <asp:SqlDataSource  ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:SurveillanceSysDBConnectionString %>" SelectCommand="SELECT * FROM [Table] ORDER BY [Time] DESC"></asp:SqlDataSource>
        
        
        
        
        <div class="row">
             <div class="col-sm-8 col-sm-offset-2 center-block">
                <asp:Image CssClass="img-responsive" ID="Image1" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-sm-8 col-sm-offset-2 center-block">
                <asp:Label CssClass="center-block" ID="Label1" runat="server" Text="Label" style="text-align: center"></asp:Label>
            </div>
            
        </div>
        

    </div>
    
    
     
    

</asp:Content>
