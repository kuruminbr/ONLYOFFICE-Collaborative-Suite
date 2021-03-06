/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/


(function ($, win, doc, body) {
    var contactadvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.contactadvancedSelector.defaults, options);

        this.init();
    };

    contactadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: contactadvancedSelector,
        initCreationBlock: function (data) {
            var that = this,
                opts = {},
                itemsSimpleSelect = [],
                items = data.items;

            opts.newoptions = [
                         {
                             title: ASC.Resources.Master.Resource.SelectorContactType, type: "choice", tag: "type", items: [
                                      { type: "person", title: ASC.Resources.Master.Resource.SelectorPerson },
                                      { type: "company", title: ASC.Resources.Master.Resource.SelectorCompany }
                             ]
                         },
                         { title: ASC.Resources.Master.Resource.SelectorFirstName, type: "input", tag: "first-name" },
                         { title: ASC.Resources.Master.Resource.SelectorLastName, type: "input", tag: "last-name" },
                         { title: ASC.Resources.Master.Resource.SelectorCompany, type: "select", tag: "company" },
                         { title: ASC.Resources.Master.Resource.SelectorCompanyName, type: "input", tag: "title" }
            ];
            opts.newbtn = ASC.Resources.Master.Resource.CreateButton;

            that.displayAddItemBlock.call(that, opts);

            var $select = that.$advancedSelector.find(".advanced-selector-field-wrapper.type .advanced-selector-field");

            $select.on("change", function () {
                var typeContact = $select.val();
                switch (typeContact) {
                    case "person":
                        that.$advancedSelector.find(".advanced-selector-field-wrapper.first-name, .advanced-selector-field-wrapper.last-name, .advanced-selector-field-wrapper.company").show();
                        that.$advancedSelector.find(".advanced-selector-field-wrapper.title").hide();
                        break;
                    case "company":
                        that.$advancedSelector.find(".advanced-selector-field-wrapper.first-name, .advanced-selector-field-wrapper.last-name, .advanced-selector-field-wrapper.company").hide();
                        that.$advancedSelector.find(".advanced-selector-field-wrapper.title").show();
                        break;
                }
            });
            $select.trigger("change");

            for (var i = 0, length = items.length; i < length; i++) {
                if (items[i].type == "company") {
                    itemsSimpleSelect.push(
                        {
                            title: items[i].title,
                            id: items[i].id
                        }
                    );
                }
            }
            that.initDataSimpleSelector.call(that, { tag: "company", items: itemsSimpleSelect });
            that.hideLoaderSimpleSelector.call(that, "company");

        },

        initAdvSelectorData: function () {
            var that = this;
            Teamlab.getCrmContacts({}, {
                before: function () {
                    that.showLoaderListAdvSelector.call(that, 'items');
                },
                after: function () {
                    that.hideLoaderListAdvSelector.call(that, 'items');
                },
                success: function (params, data) {
                    that.rewriteObjectItem.call(that, data);
                },
                error: function (params, errors) {
                    toastr.error(errors);
                }
            });
        },

        createNewItemFn: function () {
            var that = this,
                $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
                $btn = $addPanel.find(".advanced-selector-btn-add"),
                isError,

            isCompany = $addPanel.find(".type select").val() == "company" ? true : false,
            newContact = {};

            if (isCompany) {
                newContact.companyName = $addPanel.find(".title input").val().trim();
            } else {
                newContact.firstName = $addPanel.find(".first-name input").val().trim();
                newContact.lastName = $addPanel.find(".last-name input").val().trim();
                newContact.companyId = $addPanel.find(".company input").attr("data-id");
            }
            if (isCompany && !newContact.companyName) {
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: ASC.Resources.Master.Resource.ContactSelectorEmptyNameError });
                isError = true;
            }
            if (!isCompany && !newContact.firstName) {
                that.showErrorField.call(that, { field: $addPanel.find(".first-name"), error: ASC.Resources.Master.Resource.ErrorEmptyUserFirstName });
                isError = true;
            }
            if (!isCompany && !newContact.lastName) {
                that.showErrorField.call(that, { field: $addPanel.find(".last-name"), error: ASC.Resources.Master.Resource.ErrorEmptyUserLastName });
                isError = true;
            }
            if (!isCompany && !newContact.companyId && $addPanel.find(".company input").val()) {
                that.showErrorField.call(that, { field: $addPanel.find(".company"), error: ASC.Resources.Master.Resource.ContactSelectorNotFoundError });
                isError = true;
            }
            if (isError) {
                $addPanel.find(".error input").first().focus();
                return;
            }
            Teamlab.addCrmContact({}, isCompany, newContact, {
                before:function(){
                    that.displayLoadingBtn.call(that, { btn: $btn, text: ASC.Resources.Master.Resource.LoadingProcessing });
                },
                error: function (params, errors) {
                    that.showServerError.call(that, { field: $btn, error: errors });
                },
                success: function (params, contact) {
                    var newcontact = {
                        id: contact.id,
                        title: contact.displayName
                    };
                    that.actionsAfterCreateItem.call(that, { newitem: newcontact, response: contact });
                },
                after: function () { that.hideLoadingBtn.call(that, $btn); }
            })

        },

        rewriteObjectItem: function (data) {
            var that = this;
            that.items = [];

            for (var i = 0, length = data.length; i < length; i++) {
                var newObj = {};
                newObj.title = data[i].displayName || data[i].title || data[i].name || data[i].Name;
                newObj.id = (data[i].id && data[i].id.toString()) || data[i].Id.toString();

                if (data[i].hasOwnProperty("contactclass")) {
                    newObj.type = data[i].contactclass;
                }

                that.items.push(newObj);
            }

            that.items = that.items.sort(SortData);
            that.showItemsListAdvSelector.call(that);
        },

    });

   

    $.fn.contactadvancedSelector = function (option, val) {
        return this.each(function () {
            var $this = $(this),
                data = $this.data('contactadvancedSelector'),
                options = $.extend({},
                        $.fn.contactadvancedSelector.defaults,
                        $this.data(),
                        typeof option == 'object' && option);
            if (!data) $this.data('contactadvancedSelector', (data = new contactadvancedSelector(this, options)));
            if (typeof option == 'string') data[option](val);
        });
    }
    $.fn.contactadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        showme: true,
        addtext: ASC.Resources.Master.Resource.ContactSelectorAddText,
        noresults: ASC.Resources.Master.Resource.ContactSelectorNoResult,
        noitems: ASC.Resources.Master.Resource.ContactSelectorNoItems,
        emptylist: ASC.Resources.Master.Resource.ContactSelectorEmptyList
    });

})(jQuery, window, document, document.body);