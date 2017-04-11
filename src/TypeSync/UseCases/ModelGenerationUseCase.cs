﻿using System.IO;
using System.Linq;
using log4net;
using TypeSync.Common.Constants;
using TypeSync.Core.Features.ModelAnalysis;
using TypeSync.Models;
using TypeSync.Output.Converters;
using TypeSync.Output.Emitters;
using TypeSync.Output.Generators;
using TypeSync.Providers;

namespace TypeSync.UseCases
{
    public class ModelGenerationUseCase : IUseCase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ModelGenerationUseCase));

        private readonly IConfigurationProvider _configurationProvider;

        private Configuration _configuration;

        public string Id { get; } = "GenerateModels";

        public string Name { get; } = "Generate TypeScript model classes from C# DTO objects.";

        public ModelGenerationUseCase(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public Result Handle()
        {
            _configuration = _configurationProvider.GetConfiguration();

            var supportedExtensions = DotNetFileExtension.All;
            var extension = Path.GetExtension(_configuration.Path);

            if (!supportedExtensions.Contains(extension))
            {
                return Result.CreateError(string.Format("Unsupported path extension - {0}.  Supported extensions: {1}", extension, string.Join(", ", supportedExtensions)));
            }

            var analyzer = new ModelAnalyzer();
            var converter = new ModelConverter();
            var generator = new ModelGenerator();
            var emitter = new TypeScriptEmitter();

            var analysisResult = analyzer.Analyze(_configuration.Path);

            if (!analysisResult.Success)
            {
                return Result.CreateError($"Source analysis error: {analysisResult.ErrorMessage}");
            }

            log.Debug("Source analyzed");

            var tsClassModels = converter.ConvertClasses(analysisResult.Value.Classes);
            var tsEnumModels = converter.ConvertEnums(analysisResult.Value.Enums);

            log.Debug("Models converted");

            foreach (var tsModel in tsClassModels)
            {
                var contents = generator.GenerateClass(tsModel);

                emitter.Emit(_configuration.OutputPath, tsModel.Name, EmittedFileType.Model, contents);

                log.Debug($"Class {tsModel.Name} emitted");
            }

            foreach (var tsModel in tsEnumModels)
            {
                var contents = generator.GenerateEnum(tsModel);

                emitter.Emit(_configuration.OutputPath, tsModel.Name, EmittedFileType.Enum, contents);

                log.Debug($"Enum {tsModel.Name} emitted");
            }

            return Result.CreateSuccess();
        }
    }
}
