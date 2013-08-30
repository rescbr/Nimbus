﻿// <copyright file="ErrorPageMiddleware.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Diagnostics.Views;

namespace Microsoft.Owin.Diagnostics
{
    /// <summary>
    /// Captures synchronous and asynchronous exceptions from the pipeline and generates HTML error responses.
    /// </summary>
    public class ErrorPageMiddleware : OwinMiddleware
    {
        private readonly ErrorPageOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public ErrorPageMiddleware(OwinMiddleware next, ErrorPageOptions options)
            : base(next)
        {
            _options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "For diagnostics")]
        public override Task Invoke(IOwinContext context)
        {
            try
            {
                return Next.Invoke(context).ContinueWith(appTask =>
                {
                    if (appTask.IsFaulted)
                    {
                        return DisplayExceptionWrapper(context, appTask.Exception);
                    }
                    if (appTask.IsCanceled)
                    {
                        return DisplayExceptionWrapper(context, new TaskCanceledException(appTask));
                    }
                    return TaskHelpers.Completed();
                });
            }
            catch (Exception ex)
            {
                // If there's a Exception while generating the error page, re-throw the original exception.
                try
                {
                    return DisplayException(context, ex);
                }
                catch (Exception)
                {
                }

                throw;
            }
        }

        // If there's a Exception while generating the error page, re-throw the original exception.
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to re-throw the original.")]
        private Task DisplayExceptionWrapper(IOwinContext context, Exception ex)
        {
            try
            {
                return DisplayException(context, ex);
            }
            catch (Exception)
            {
                return TaskHelpers.FromError(ex);
            }
        }

        // Assumes the response headers have not been sent.  If they have, still attempt to write to the body.
        private Task DisplayException(IOwinContext context, Exception ex)
        {
            var request = context.Request;
            var errorPage = new ErrorPage
            {
                Model = new ErrorPageModel
                {
                    Error = ex,
                    StackFrames = StackFrames(ex),
                    Environment = request.Environment,
                    Query = request.Query,
                    Cookies = request.Cookies,
                    Headers = request.Headers,
                }
            };
            errorPage.Execute(context);
            return TaskHelpers.Completed();
        }

        private IEnumerable<StackFrame> StackFrames(Exception ex)
        {
            return StackFrames(StackTraces(ex).Reverse());
        }

        private static IEnumerable<string> StackTraces(Exception ex)
        {
            for (Exception scan = ex; scan != null; scan = scan.InnerException)
            {
                yield return scan.StackTrace;
            }
        }

        private IEnumerable<StackFrame> StackFrames(IEnumerable<string> stackTraces)
        {
            foreach (var stackTrace in stackTraces.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                var heap = new Chunk { Text = stackTrace + Environment.NewLine, End = stackTrace.Length + 2 };
                for (Chunk line = heap.Advance(Environment.NewLine); line.HasValue; line = heap.Advance(Environment.NewLine))
                {
                    yield return StackFrame(line);
                }
            }
        }

        private StackFrame StackFrame(Chunk line)
        {
            line.Advance("  at ");
            string function = line.Advance(" in ").ToString();
            string file = line.Advance(":line ").ToString();
            int lineNumber = line.ToInt32();

            return string.IsNullOrEmpty(file)
                ? LoadFrame(line.ToString(), string.Empty, 0)
                : LoadFrame(function, file, lineNumber);
        }

        private StackFrame LoadFrame(string function, string file, int lineNumber)
        {
            var frame = new StackFrame { Function = function, File = file, Line = lineNumber };
            if (File.Exists(file))
            {
                string[] code = File.ReadAllLines(file);
                frame.PreContextLine = Math.Max(lineNumber - _options.SourceCodeLineCount, 1);
                frame.PreContextCode = code.Skip(frame.PreContextLine - 1).Take(lineNumber - frame.PreContextLine).ToArray();
                frame.ContextCode = code.Skip(lineNumber - 1).FirstOrDefault();
                frame.PostContextCode = code.Skip(lineNumber).Take(_options.SourceCodeLineCount).ToArray();
            }
            return frame;
        }

        internal class Chunk
        {
            public string Text { get; set; }
            public int Start { get; set; }
            public int End { get; set; }

            public bool HasValue
            {
                get { return Text != null; }
            }

            public Chunk Advance(string delimiter)
            {
                int indexOf = HasValue ? Text.IndexOf(delimiter, Start, End - Start, StringComparison.Ordinal) : -1;
                if (indexOf < 0)
                {
                    return new Chunk();
                }

                var chunk = new Chunk { Text = Text, Start = Start, End = indexOf };
                Start = indexOf + delimiter.Length;
                return chunk;
            }

            public override string ToString()
            {
                return HasValue ? Text.Substring(Start, End - Start) : string.Empty;
            }

            public int ToInt32()
            {
                int value;
                return HasValue && Int32.TryParse(
                    Text.Substring(Start, End - Start),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value) ? value : 0;
            }
        }
    }
}