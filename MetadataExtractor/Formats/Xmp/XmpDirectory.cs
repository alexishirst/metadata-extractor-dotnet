#region License
//
// Copyright 2002-2019 Drew Noakes
// Ported from Java to C# by Yakov Danilov for Imazen LLC in 2014
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// More information about this project is available at:
//
//    https://github.com/drewnoakes/metadata-extractor-dotnet
//    https://drewnoakes.com/code/exif/
//
#endregion

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using XmpCore;
using XmpCore.Impl;
using XmpCore.Options;

namespace MetadataExtractor.Formats.Xmp
{
    /// <summary>
    /// Wraps an instance of Adobe's <see cref="IXmpMeta"/> object, which holds XMP data.
    /// </summary>
    /// <remarks>
    /// XMP uses a namespace and path format for identifying values, which does not map to metadata-extractor's
    /// integer based tag identifiers. Therefore, XMP data is extracted and exposed via <see cref="XmpMeta"/>
    /// which returns an instance of Adobe's <see cref="IXmpMeta"/> which exposes the full XMP data set.
    /// </remarks>
    /// <author>Torsten Skadell</author>
    /// <author>Drew Noakes https://drewnoakes.com</author>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class XmpDirectory : Directory
    {
        public const int TagXmpValueCount = 0xFFFF;


        private static readonly Dictionary<int, string> _tagNameMap = new Dictionary<int, string>
        {
            { TagXmpValueCount, "XMP Value Count" }
        };

        /// <summary>Gets the <see cref="IXmpMeta"/> object within this directory.</summary>
        /// <remarks>This object provides a rich API for working with XMP data.</remarks>
        [CanBeNull]
        public IXmpMeta XmpMeta { get; private set; }

        public XmpDirectory()
        {
            SetDescriptor(new XmpDescriptor(this));
        }

        public override string Name => "XMP";

        protected override bool TryGetTagName(int tagType, out string tagName)
        {
            return _tagNameMap.TryGetValue(tagType, out tagName);
        }

        // set only once to save some allocations
        private static readonly IteratorOptions _iteratorOptions = new IteratorOptions() { IsJustLeafNodes = true };

        /// <summary>Gets a map of all XMP properties in this directory, not just the known ones.</summary>
        /// <remarks>
        /// This is required because XMP properties are represented as strings, whereas the rest of this library
        /// uses integers for keys.
        /// </remarks>
        [NotNull]
        public IDictionary<string, string> GetXmpProperties()
        {
            var propertyValueByPath = new Dictionary<string, string>();
            if (XmpMeta != null)
            {
                try
                {
                    XmpIterator i = new XmpIterator((XmpMeta)XmpMeta, null, null, _iteratorOptions);
                    while (i.HasNext())
                    {
                        var prop = (IXmpPropertyInfo)i.Next();
                        var path = prop.Path;
                        var value = prop.Value;
                        if (path != null && value != null)
                            propertyValueByPath.Add(path, value);
                    }
                }
                catch (XmpException) { } // ignored
            }

            return propertyValueByPath;
        }

        public void SetXmpMeta([NotNull] IXmpMeta xmpMeta)
        {
            XmpMeta = xmpMeta;

            int valueCount = 0;
            XmpIterator i = new XmpIterator((XmpMeta)XmpMeta, null, null, _iteratorOptions);

            while (i.HasNext())
            {
                var prop = (IXmpPropertyInfo)i.Next();

                if (prop.Path != null)
                    valueCount++;
            }

            Set(TagXmpValueCount, valueCount);
        }
    }
}
