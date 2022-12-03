/// MIT License
/// 
/// Copyright (c) 2020-2022 Oleksii Holub
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Xml.Linq;

namespace CompetitiveVerifierResolverTestLogger;
internal static class TestRunCriteriaExtensions
{
    public static string? TryGetTargetFramework(this TestRunCriteria testRunCriteria)
    {
        if (string.IsNullOrWhiteSpace(testRunCriteria.TestRunSettings))
            return null;

        return (string?)XElement
            .Parse(testRunCriteria.TestRunSettings)
            .Element("RunConfiguration")?
            .Element("TargetFrameworkVersion");
    }
}
internal static class GenericExtensions
{
    public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> transform) => transform(input);
}