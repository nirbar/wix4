// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core
{
    using System;
    using WixToolset.Data;

    internal static class CompilerErrors
    {
        public static Message AlreadyDefinedBootstrapperApplicationSource(SourceLineNumber sourceLineNumbers, SourceLineNumber originalSourceLineNumbers, string originalElementName)
        {
            return Message(sourceLineNumbers, Ids.AlreadyDefinedBootstrapperApplicationSource, "More than one BootstrapperApplication source file was specified. Only one is allowed. Another BootstrapperApplication source file was defined via the {0} element at {1}.", originalElementName, originalSourceLineNumbers.ToString());
        }

        public static Message IllegalCharactersInProvider(SourceLineNumber sourceLineNumbers, string attributeName, char illegalChar, string illegalChars)
        {
            return Message(sourceLineNumbers, Ids.IllegalCharactersInProvider, "The provider key authored into the {0} attribute contains an illegal character, '{1}'. Please author the provider key without any of the following characters: {2}", attributeName, illegalChar, illegalChars);
        }

        public static Message ReservedValue(SourceLineNumber sourceLineNumbers, string elementName, string attributeName, string attributeValue)
        {
            return Message(sourceLineNumbers, Ids.ReservedValue, "The {0}/@{1} attribute value '{2}' is reserved and cannot be used here. Please choose a different value.", elementName, attributeName, attributeValue);
        }

        public static Message IllegalBundleVariableName(SourceLineNumber sourceLineNumbers, string elementName,string attributeName, string value)
        {
            return Message(sourceLineNumbers, Ids.IllegalBundleVariableName, "The {0}/@{1} attribute's value, '{2}', is not a legal bundle variable name. Identifiers may contain ASCII characters A-Z, a-z, digits, or underscores (_). Every identifier must begin with either a letter or an underscore.", elementName, attributeName, value);
        }

        public static Message IllegalTagName(SourceLineNumber sourceLineNumbers, string parentElement, string name)
        {
            return Message(sourceLineNumbers, Ids.IllegalName, "The Tag/@Name attribute value, '{1}', contains invalid filename identifiers. The Tag/@Name may have defaulted from the {0}/@Name attrbute. If so, use the Tag/@Name attribute to provide a valid filename. Any character except for the follow may be used: \\ ? | > < : / * \".", parentElement, name);
        }

        public static Message ExampleRegid(SourceLineNumber sourceLineNumbers, string regid)
        {
            return Message(sourceLineNumbers, Ids.ExampleRegid, "Regid '{0}' is a placeholder that must be replaced with an appropriate value for your installation. Use the simplified URI for your organization or project.", regid);
        }

        public static Message ObsoleteAttribute(SourceLineNumber sourceLineNumbers, string elementName, string attributeName, string replacmentElement)
        {
            return Message(sourceLineNumbers, Ids.ObsoleteAttribute, "Attribute '{0}' in element '{1}' is no longer supported. Use element '{2}' instead", attributeName, elementName, replacmentElement);
        }

        private static Message Message(SourceLineNumber sourceLineNumber, Ids id, string format, params object[] args)
        {
            return new Message(sourceLineNumber, MessageLevel.Error, (int)id, format, args);
        }

        public enum Ids
        {
            IllegalCharactersInProvider = 5400,
            ReservedValue = 5401,

            IllegalName = 6601,
            ExampleRegid = 6602,
            IllegalBundleVariableName = 6603,
            AlreadyDefinedBootstrapperApplicationSource = 6604,
            ObsoleteAttribute = 6605,
        } // 5400-5499 and 6600-6699 were the ranges for Dependency and Tag which are now in Core between CompilerWarnings and CompilerErrors.
    }
}
