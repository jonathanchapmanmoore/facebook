<%@ Page Language="C#" MasterPageFile="~/SiteMaster.Master" AutoEventWireup="true" CodeBehind="Members.aspx.cs" Inherits="Fisharoo.FisharooWeb.Groups.Members" %>
<%@ Import Namespace="Fisharoo.FisharooCore.Core.Domain"%>
<%@Register Src="~/UserControls/ProfileDisplay.ascx" TagPrefix="Fisharoo" TagName="Profile" %>
<asp:Content ContentPlaceHolderID="Content" runat="server">
    <div class="divContainer">
        <div class="divContainerBox">
            <div class="divContainerRow">
                <div style="float:left;"><asp:LinkButton OnClick="lbBack_Click" ID="lbBack" runat="server" Text="Back"></asp:LinkButton>&nbsp;</div>
                <div style="float:left;"><asp:LinkButton OnClick="lbPrevious_Click" ID="lbPrevious" runat="server" Text="Previous"></asp:LinkButton>&nbsp;</div>
                <div style="text-align:right;"><asp:LinkButton OnClick="lbNext_Click" ID="lbNext" runat="server" Text="Next"></asp:LinkButton>&nbsp;</div>
            </div>
            <div class="divContainerTitle">Members to approve:</div>
            <div class="divContainerRow">
                <asp:Repeater ID="repMembersToApprove" runat="server" OnItemDataBound="repMembersToApprove_ItemDataBound">
                    <HeaderTemplate><table><tr><td>&nbsp;</td><td>&nbsp;</td></tr></HeaderTemplate>
                    <ItemTemplate>
                        <tr><td>
                            <asp:CheckBox ID="chkProfile" runat="server" />
                        </td><td>
                            <Fisharoo:Profile ID="Profile1" ShowDeleteButton="false" runat="server" />
                        </td></tr>
                    </ItemTemplate>
                    <FooterTemplate></table></FooterTemplate>
                </asp:Repeater>
            </div>    
            <div class="divContainerTitle">Members</div>
            <div class="divContainerRow">            
                <asp:Repeater ID="repMembers" runat="server" OnItemDataBound="repMembers_ItemDataBound">
                    <HeaderTemplate><table><tr><td>&nbsp;</td><td>&nbsp;</td></tr></HeaderTemplate>
                    <ItemTemplate>
                        <tr><td>
                            <asp:CheckBox ID="chkProfile" runat="server" />
                        </td><td>
                            <Fisharoo:Profile ID="Profile1" ShowDeleteButton="false" runat="server" />
                        </td></tr>
                    </ItemTemplate>
                    <FooterTemplate></table></FooterTemplate>
                </asp:Repeater>
            </div>
            <div class="divContainerRow">            
                <asp:Label ForeColor="Red" runat="server" ID="lblMessage"></asp:Label>
            </div>
            <div class="divContainerFooter">&nbsp;
                <asp:Panel ID="pnlButtons" runat="server">
                    <asp:Button ID="btnApprove" OnClick="btnApprove_Click" runat="server" Text="Approve" />
                    <asp:Button ID="btnDelete" OnClick="btnDelete_Click" runat="server" Text="Delete" />
                    <asp:Button ID="btnPromoteToAdmin" OnClick="btnPromoteToAdmin_Click" runat="server" Text="Promote to Admin" />
                    <asp:Button ID="btnDemoteAdmins" OnClick="btnDemoteAdmins_Click" runat="server" Text="Demote Admins" />
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>