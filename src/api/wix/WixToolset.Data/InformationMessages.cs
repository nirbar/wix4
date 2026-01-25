// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Data
{
    using System;

    public static class InformationMessages
    {
        public static Message TimeMeasurementTitle()
        {
            return Message(null, Ids.TimeMeasurementTitle, "Build timing measurements:");
        }

        public static Message TimeMeasurement(string name, TimeSpan timeSpan)
        {
            return Message(null, Ids.TimeMeasurement, "\t{0,-30}{1:c}", name, timeSpan);
        }

        private static Message Message(SourceLineNumber sourceLineNumber, Ids id, string format, params object[] args)
        {
            return new Message(sourceLineNumber, MessageLevel.Information, (int)id, format, args);
        }

        public enum Ids
        {
            TimeMeasurementTitle = 1000,
            TimeMeasurement = 1001,
        }
    }
}
