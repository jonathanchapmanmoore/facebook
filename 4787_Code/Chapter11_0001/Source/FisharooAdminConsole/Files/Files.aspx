<%@ Page Language="C#" MasterPageFile="~/SiteMaster.Master" AutoEventWireup="true" CodeBehind="Files.aspx.cs" Inherits="Fisharoo.FisharooAdminConsole.Files.Files" %>

<asp:Content runat="server" ContentPlaceHolderID="Content">
    <asp:GridView 
        ID="gvFiles" 
        runat="server" 
        DataSourceID="FilesDataSource" 
        AllowPaging="true" 
        AllowSorting="true" 
        AutoGenerateDeleteButton="true" 
        AutoGenerateEditButton="true"></asp:GridView>
        
    <asp:LinqDataSource 
        ID="FilesDataSource" 
        ContextTypeName="Fisharoo.FisharooCore.Core.Domain.FisharooDataContext" 
        TableName="Files" 
        EnableDelete="true" 
        EnableInsert="true" 
        EnableUpdate="true" 
        runat="server"></asp:LinqDataSource>
</asp:Content>
