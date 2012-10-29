﻿// <copyright file="Utils.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class Utils
    {
        /// <summary>
        /// Converts path value to a normal form.
        /// Null values are treated as string.empty.
        /// A path segment is always accompanied by it's leading slash.
        /// A root path is string.empty 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path ?? string.Empty;
            }
            if (path.Length == 1)
            {
                return path[0] == '/' ? string.Empty : '/' + path;
            }
            return path[0] == '/' ? path : '/' + path;
        }
    }
}
