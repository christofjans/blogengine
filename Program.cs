﻿using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var serviceProvider = CreateServiceProvider();
var processor = serviceProvider.GetService<IProcessor>();

processor!.Process(args[0], args[1]);




IServiceProvider CreateServiceProvider()
{
    var serviceCollection = new ServiceCollection();

    serviceCollection.AddSingleton<IProcessor, Processor>();
    serviceCollection.AddSingleton<IFileSystem, FileSystem>();
    serviceCollection.AddSingleton<IFileProcessor, DispatchFileProcessor>();
    serviceCollection.AddSingleton<NoopFileProcessor>();
    serviceCollection.AddSingleton<CopyFileProcessor>();
    serviceCollection.AddSingleton<PostFileProcessor>();
    serviceCollection.AddSingleton<NoPostFileProcessor>();
    serviceCollection.AddSingleton<IMarkdownToHtmlConverter, MarkDigConverter>();
    serviceCollection.AddSingleton<ITemplateEngine, HandlebarsTemplateEngine>();
    serviceCollection.AddSingleton<IPostFinder, PostFinder>();
    serviceCollection.AddLogging(lb=>lb.AddConsole());
    
    return serviceCollection.BuildServiceProvider();
}
