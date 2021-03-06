﻿<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" MasterPageFile="~/Masters/basetemplate.master" CodeBehind="Wizard.aspx.cs" Inherits="ASC.Web.Studio.Wizard" Title="ONLYOFFICE™" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<asp:Content ContentPlaceHolderID="PageContent" runat="server">

    <div class="wizardContent">
        <div class="wizardTitle">
            <%= Resources.Resource.WelcomeTitle %>
        </div>
        <div class="wizardDesc"><%= Resources.Resource.WelcomeDescription %></div>
        <asp:PlaceHolder ID="content" runat="server"></asp:PlaceHolder>
    </div>

    <% if (!CoreContext.Configuration.Standalone && !CoreContext.Configuration.Personal && SetupInfo.CustomScripts.Length != 0)
       { %>
    <!-- Google Code for Tag for created portal -->
    <!-- Remarketing tags may not be associated with personally identifiable information or placed on pages related to sensitive categories. For instructions on adding this tag and more information on the above requirements, read the setup guide: google.com/ads/remarketingsetup -->
    <script type="text/javascript">
        /* <![CDATA[ */
        var google_conversion_id = 1025072253;
        var google_conversion_label = "l8P4CPOiswgQ_bjl6AM";
        var google_custom_params = window.google_tag_params;
        var google_remarketing_only = true;
        /* ]]> */
    </script>
    <script type="text/javascript" src="//www.googleadservices.com/pagead/conversion.js">
    </script>
    <noscript>
        <div style="display: inline;">
            <img height="1" width="1" style="border-style: none;" alt="" src="//googleads.g.doubleclick.net/pagead/viewthroughconversion/1025072253/?value=0&label=l8P4CPOiswgQ_bjl6AM&guid=ON&script=0" />
        </div>
    </noscript>

<!-- Google Code for PortalReg_042014 Conversion Page -->
<script type="text/javascript">
/* <![CDATA[ */
var google_conversion_id = 1025072253;
var google_conversion_language = "en";
var google_conversion_format = "2";
var google_conversion_color = "ffffff";
var google_conversion_label = "VaHvCPOCkQkQ_bjl6AM";
var google_remarketing_only = false;
/* ]]> */
</script>
<script type="text/javascript" src="//www.googleadservices.com/pagead/conversion.js">
</script>
<noscript>
<div style="display:inline;">
<img height="1" width="1" style="border-style:none;" alt="" src="//www.googleadservices.com/pagead/conversion/1025072253/?label=VaHvCPOCkQkQ_bjl6AM&guid=ON&script=0"/>
</div>
</noscript>

    <% } %>
</asp:Content>
