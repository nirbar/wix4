// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Extensibility
{
    using System;
    using System.Collections.Generic;
    using WixToolset.Data;
    using WixToolset.Extensibility.Services;

    /// <summary>
    /// Time taking factory.
    /// </summary>
    public class TimeTakerFactory : ITimeTakerFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider"></param>
        public TimeTakerFactory(IServiceProvider provider)
        {
            this._provider = provider;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITimeTaker GetTimeTaker(string name)
        {
            var timeTaker = this._provider.GetService<ITimeTaker>();

            if (!this._timeMeasurements.ContainsKey(name))
            {
                this._timeMeasurements[name] = new List<ITimeTaker>();
            }

            this._timeMeasurements[name].Add(timeTaker);
            return timeTaker;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void PrintMeasurements()
        {
            var allTimesDict = new Dictionary<string, TimeSpan>();
            var allTimes = new List<Tuple<string, TimeSpan>>();
            foreach (var group in this._timeMeasurements)
            {
                TimeSpan groupTime = new TimeSpan(0);
                foreach (var time in group.Value)
                {
                    groupTime = groupTime.Add(time.GetMeasurement());
                }
                allTimesDict[group.Key] = groupTime;
                allTimes.Add(new Tuple<string, TimeSpan>(group.Key, groupTime));
            }
            allTimes.Sort((a,b) => allTimesDict[a.Item1].CompareTo(allTimesDict[b.Item1]));

            var messaging = this._provider.GetService<IMessaging>();
            messaging.Write(InformationMessages.TimeMeasurementTitle());
            foreach (var item in allTimes)
            {
                messaging.Write(InformationMessages.TimeMeasurement(item.Item1, item.Item2));
            }
        }

        private readonly IServiceProvider _provider;
        private readonly Dictionary<string, List<ITimeTaker>> _timeMeasurements = new Dictionary<string, List<ITimeTaker>>();
    }
}
