// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the MIT License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet_etcd.helper
{
    /// <summary>
    /// Helps with running async code in sync methods
    /// </summary>
    /// <remarks>Based on the AsyncHelper of the Microsoft.AspNet.Identity project (https://web.archive.org/web/20200411071640/https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs)</remarks>
    public static class AsyncHelper
    {
        private static readonly TaskFactory MyTaskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Run the provided <paramref name="func"/> async method synchronously 
        /// </summary>
        /// <typeparam name="TResult">The type that is returned in the Task of the <paramref name="func"/></typeparam>
        /// <param name="func">A call to the async method</param>
        /// <returns>The result of the task created by the <paramref name="func"/></returns>
        /// <example>
        /// <code>
        /// AsyncHelper.RunSync(SomeFunctionAsync)
        /// </code>
        /// or
        /// <code>
        /// AsyncHelper.RunSync(async () => await SomeFunctionAsync())
        /// </code>
        /// </example>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return MyTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs the provided async function provided in <paramref name="func"/> in a synchronous way
        /// </summary>
        /// <param name="func">A call to the async method</param>
        /// <example>
        /// <code>
        /// AsyncHelper.RunSync(SomeFunctionAsync)
        /// </code>
        /// or
        /// <code>
        /// AsyncHelper.RunSync(async () => await SomeFunctionAsync())
        /// </code>
        /// </example>
        public static void RunSync(Func<Task> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            MyTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }
    }
}