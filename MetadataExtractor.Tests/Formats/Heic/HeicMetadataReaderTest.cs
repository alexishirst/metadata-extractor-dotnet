using System.Collections.Generic;
using System.Linq;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using Xunit;

namespace MetadataExtractor.Tests.Formats.Heic
{
    public class HeicMetadataReaderTest
    {
        [Fact]
        public void ExtractMetadataUsingPath()
        {
            Validate(ImageMetadataReader.ReadMetadata(@"Data/Sample.HEIC"));
        }

        private static void Validate(IEnumerable<Directory> metadata)
        {
            var directory = metadata.OfType<QuickTimeFileTypeDirectory>().FirstOrDefault();

            Assert.NotNull(directory);
            Assert.Equal("heic", directory.GetString(QuickTimeFileTypeDirectory.TagMajorBrand));
        }
    }
}