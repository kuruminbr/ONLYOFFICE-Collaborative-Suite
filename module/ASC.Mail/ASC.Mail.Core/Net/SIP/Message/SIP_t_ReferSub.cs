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

namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "refer-sub" value. Defined in RFC 4488.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4488 Syntax:
    ///     Refer-Sub       = refer-sub-value *(SEMI exten)
    ///     refer-sub-value = "true" / "false"
    ///     exten           = generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_ReferSub : SIP_t_ValueWithParams
    {
        #region Members

        private bool m_Value;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets refer-sub-value value.
        /// </summary>
        public bool Value
        {
            get { return m_Value; }

            set { m_Value = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_ReferSub() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Refer-Sub value.</param>
        public SIP_t_ReferSub(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Refer-Sub" from specified value.
        /// </summary>
        /// <param name="value">SIP "Refer-Sub" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>value</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "Refer-Sub" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Refer-Sub       = refer-sub-value *(SEMI exten)
                refer-sub-value = "true" / "false"
                exten           = generic-param        
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // refer-sub-value
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Refer-Sub refer-sub-value value is missing !");
            }
            try
            {
                m_Value = Convert.ToBoolean(word);
            }
            catch
            {
                throw new SIP_ParseException("Invalid Refer-Sub refer-sub-value value !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "contact-param" value.
        /// </summary>
        /// <returns>Returns "contact-param" value.</returns>
        public override string ToStringValue()
        {
            /*
                Refer-Sub       = refer-sub-value *(SEMI exten)
                refer-sub-value = "true" / "false"
                exten           = generic-param        
            */

            StringBuilder retVal = new StringBuilder();

            // refer-sub-value
            retVal.Append(m_Value.ToString());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}