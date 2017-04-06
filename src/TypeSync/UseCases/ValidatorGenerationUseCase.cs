﻿using log4net;
using TypeSync.Models;

namespace TypeSync.UseCases
{
    public class ValidatorGenerationUseCase : IUseCase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ValidatorGenerationUseCase));

        public string Id { get; } = "GenerateValidators";

        public string Name { get; } = "Generate TypeScript validators based on server side implemented .NET DTO validation.";

        public ValidatorGenerationUseCase()
        {
        }

        public Result Handle()
        {
            return Result.CreateSuccess();
        }
    }
}
