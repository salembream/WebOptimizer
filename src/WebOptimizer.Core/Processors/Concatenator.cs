﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebOptimizer
{
    internal class Concatenator : Processor
    {
        public override Task ExecuteAsync(IAssetContext context)
        {
            context.Content = new Dictionary<string, byte[]>
            {
                { Guid.NewGuid().ToString(), context.Content.Values.SelectMany(x => x).ToArray() }
            };


            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class PipelineExtensions
    {
        /// <summary>
        /// Adds the string content of all source files to the pipeline.
        /// </summary>
        public static IAsset Concatenate(this IAsset asset)
        {
            var reader = new Concatenator();
            asset.Processors.Add(reader);

            return asset;
        }

        /// <summary>
        /// Adds the string content of all source files to the pipeline.
        /// </summary>
        public static IEnumerable<IAsset> Concatenate(this IEnumerable<IAsset> assets)
        {
            return assets.AddProcessor(asset => asset.Concatenate());
        }
    }
}
