// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Extensibility
{
    /// <summary>
    /// Time taking factory.
    /// </summary>
    public interface ITimeTakerFactory
    {
        /// <summary>
        /// Get a new time taker
        /// </summary>
        /// <param name="name">Name of the time measurement.</param>
        /// <returns>A time taker instance</returns>
        ITimeTaker GetTimeTaker(string name);

        /// <summary>
        /// Print measurements summary
        /// </summary>
        void PrintMeasurements();
    }
}
