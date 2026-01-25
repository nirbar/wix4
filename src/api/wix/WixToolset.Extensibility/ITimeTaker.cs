// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Extensibility
{
    using System;

    /// <summary>
    /// Time taker.
    /// </summary>
    public interface ITimeTaker
    {
        /// <summary>
        /// Start a new time measurement
        /// </summary>
        /// <returns></returns>
        void Start();

        /// <summary>
        /// Stop time measurement
        /// </summary>
        void Stop();

        /// <summary>
        /// Stop time measurement
        /// </summary>
        /// <returns>TimeSpan</returns>
        TimeSpan GetMeasurement();
    }
}
