using System;
using Core.Models.Config;

namespace Application.Services.Coefficientes
{
    public static class CoefficientsConfigProvider
    {
        private static CoefficientsConfig? _config;

        public static void Initialize(CoefficientsConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public static CoefficientsConfig Current
        {
            get
            {
                if (_config == null)
                    throw new InvalidOperationException(
                        "CoefficientsConfigProvider не инициализирован. Вызовите CoefficientsConfigProvider.Initialize(...) в Program.cs.");
                return _config;
            }
        }
    }
}
