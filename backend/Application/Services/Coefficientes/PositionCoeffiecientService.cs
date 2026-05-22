using System.Collections.Generic;
using System.Collections.ObjectModel;
using Application.Utils;
using Core.Enums;
using Core.Models.MetricModel.Coefficientes;

namespace Application.Services.Coefficientes
{
    public static class PositionCoefficientService
    {
        private static readonly ReadOnlyDictionary<PositionGroup, PositionCoefficientData> Coefficients =
            new ReadOnlyDictionary<PositionGroup, PositionCoefficientData>(
                new Dictionary<PositionGroup, PositionCoefficientData>
                {
                    // Весы = ожидаемый уровень показателя позиции относительно базовой возрастной нормы (1.0 = эталон).
                    // Применяются в PentagonService.NormalizeHybrid: actual / (standard × weight).
                    // Чем выше weight — тем выше планка нормы для позиции (труднее достичь 100%).
                    { PositionGroup.Goalkeeper, new PositionCoefficientData { SpeedWeight = 0.65, SprintWeight = 0.45, PowerWeight = 0.75, EnduranceWeight = 1.00, ExplosiveWeight = 1.25 } },
                    { PositionGroup.Defender,   new PositionCoefficientData { SpeedWeight = 0.85, SprintWeight = 0.75, PowerWeight = 1.00, EnduranceWeight = 0.95, ExplosiveWeight = 0.95 } },
                    { PositionGroup.Midfielder, new PositionCoefficientData { SpeedWeight = 0.90, SprintWeight = 0.80, PowerWeight = 1.05, EnduranceWeight = 1.10, ExplosiveWeight = 1.00 } },
                    { PositionGroup.Forward,    new PositionCoefficientData { SpeedWeight = 0.95, SprintWeight = 0.95, PowerWeight = 0.95, EnduranceWeight = 0.90, ExplosiveWeight = 1.10 } },
                    { PositionGroup.Winger,     new PositionCoefficientData { SpeedWeight = 1.00, SprintWeight = 1.00, PowerWeight = 0.85, EnduranceWeight = 1.00, ExplosiveWeight = 1.05 } }
                });

        // Принимает конкретную позицию, затем маппит в группу, затем возвращает коэффициент
        public static PositionCoefficientData GetByPosition(Position? position)
        {
            if (position == null)
                return GetDefault();

            var group = PositionMapper.ToGroup(position.Value);
            return Coefficients.TryGetValue(group, out var coef) ? coef : GetDefault();
        }

        // Дефолт — для игрока без указанной позиции. Берём «средний полевой» (между защитником и полузащитником),
        // без преимуществ. Не используй слишком низкие веса — это даёт ложные 100% у всех без позиции.
        public static PositionCoefficientData GetDefault()
        {
            return new PositionCoefficientData
            {
                SpeedWeight = 0.90,
                SprintWeight = 0.80,
                PowerWeight = 1.00,
                EnduranceWeight = 1.00,
                ExplosiveWeight = 1.00
            };
        }
    }
}
