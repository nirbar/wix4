// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Extensibility
{
    using System;

    /// <summary>
    /// Time taker.
    /// </summary>
    public class TimeTaker: ITimeTaker
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Start()
        {
            if (this._startTime < DateTime.MaxValue)
            {
                throw new InvalidProgramException();
            }

            this._startTime = DateTime.Now;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Stop()
        {
            if ((this._startTime == DateTime.MaxValue) || (this._endTime > DateTime.MinValue))
            {
                throw new InvalidProgramException();
            }

            this._endTime = DateTime.Now;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TimeSpan GetMeasurement()
        {
            if (this._endTime < this._startTime)
            {
                throw new InvalidProgramException();
            }

            return this._endTime - this._startTime;
        }

        private DateTime _startTime = DateTime.MaxValue;
        private DateTime _endTime = DateTime.MinValue;
    }
}
