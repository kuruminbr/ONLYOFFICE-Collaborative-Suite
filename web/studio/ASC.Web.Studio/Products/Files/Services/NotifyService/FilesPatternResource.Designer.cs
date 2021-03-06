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

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASC.Web.Files.Services.NotifyService {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class FilesPatternResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal FilesPatternResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ASC.Web.Files.Services.NotifyService.FilesPatternResource", typeof(FilesPatternResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1. Access Granted to Document: &quot;$DocumentTitle&quot;:&quot;$DocumentURL&quot;
        ///
        ///$__DateTime $__AuthorName (&quot;$UserEmail&quot;:&quot;mailto:$UserEmail&quot;) granted you the access to the &quot;$DocumentTitle&quot;:&quot;$DocumentURL&quot; document with the following access rights: &quot;$AccessRights&quot;.
        ///
        ///$Message.
        /// </summary>
        public static string pattern_LinkToEmail {
            get {
                return ResourceManager.GetString("pattern_LinkToEmail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1.$__DateTime Пользователем &quot;$__AuthorName&quot;:&quot;$__AuthorUrl&quot; 
        ///
        ///был предоставлен доступ к &quot;$DocumentTitle&quot;:&quot;$DocumentURL&quot;
        ///
        ///с правами: &quot;$AccessRights&quot;.
        ///
        ///$Message 
        ///
        
        /// </summary>
        public static string pattern_ShareDocument {
            get {
                return ResourceManager.GetString("pattern_ShareDocument", resourceCulture);
            }
        }
        
        /// <summary>
        
        ///
        
        ///
        
        ///
        ///$Message
        ///
        
        /// </summary>
        public static string pattern_ShareFolder {
            get {
                return ResourceManager.GetString("pattern_ShareFolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;patterns&gt;
        ///  &lt;formatter type=&quot;ASC.Notify.Patterns.NVelocityPatternFormatter, ASC.Common&quot; /&gt;
        ///
        ///  &lt;pattern id=&quot;ShareDocument&quot; sender=&quot;email.sender&quot;&gt;
        ///    &lt;subject resource=&quot;|subject_ShareDocument|ASC.Web.Files.Services.NotifyService.FilesPatternResource,ASC.Web.Files&quot; /&gt;
        ///    &lt;body styler=&quot;ASC.Notify.Textile.TextileStyler,ASC.Notify.Textile&quot; resource=&quot;|pattern_ShareDocument|ASC.Web.Files.Services.NotifyService.FilesPatternResource,ASC.Web.Files&quot; /&gt;
        ///  &lt;/pattern&gt;
        ///  &lt;pattern id=&quot;ShareDocument&quot; sender=&quot;messan [rest of string was truncated]&quot;;.
        /// </summary>
        public static string patterns {
            get {
                return ResourceManager.GetString("patterns", resourceCulture);
            }
        }
        
        /// <summary>
        
        /// </summary>
        public static string subject_LinkToEmail {
            get {
                return ResourceManager.GetString("subject_LinkToEmail", resourceCulture);
            }
        }
        
        /// <summary>
        
        /// </summary>
        public static string subject_ShareDocument {
            get {
                return ResourceManager.GetString("subject_ShareDocument", resourceCulture);
            }
        }
        
        /// <summary>
        
        /// </summary>
        public static string subject_ShareFolder {
            get {
                return ResourceManager.GetString("subject_ShareFolder", resourceCulture);
            }
        }
    }
}
