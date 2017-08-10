﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders.Physical;
using Moq;
using Xunit;

namespace WebOptimizer.Test.Processors
{
    public class RelativePathAdjusterTest
    {
        [Theory2]
        [InlineData("url(/img/foo.png)", "url(/img/foo.png)")]
        [InlineData("url(/img/foo.png?1=1)", "url(/img/foo.png?1=1)")]
        [InlineData("url(img/foo.png)", "url(../css/img/foo.png)")]
        [InlineData("url(http://foo.png)", "url(http://foo.png)")]
        [InlineData("url('img/foo.png')", "url('../css/img/foo.png')")]
        [InlineData("url(\"img/foo.png\")", "url(\"../css/img/foo.png\")")]
        public async Task AdjustRelativePaths_Success(string url, string newUrl)
        {
            var adjuster = new RelativePathAdjuster();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var pipeline = new Mock<IAssetPipeline>().SetupAllProperties();
            var inputFile = new PhysicalFileInfo(new FileInfo(@"c:\source\css\site.css"));
            var outputFile = new PhysicalFileInfo(new FileInfo(@"c:\source\dist\all.css"));
            var options = new Mock<WebOptimizerOptions>().SetupAllProperties();

            context.SetupGet(s => s.Asset.Route)
                   .Returns("/my/route.css");

            context.SetupGet(s => s.Options)
                   .Returns(options.Object);

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IAssetPipeline)))
                   .Returns(pipeline.Object);

            options.SetupSequence(s => s.FileProvider.GetFileInfo(It.IsAny<string>()))
                   .Returns(inputFile)
                   .Returns(outputFile);

            context.Object.Content = new Dictionary<string, byte[]> { { "css/site.css", url.AsByteArray() } };

            await adjuster.ExecuteAsync(context.Object);

            Assert.Equal(newUrl, context.Object.Content.First().Value.AsString());
            Assert.Equal("", adjuster.CacheKey(new DefaultHttpContext()));
        }
    }
}
