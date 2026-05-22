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
                    { PositionGroup.Goalkeeper, new PositionCoefficientData { SpeedWeight = 0.30, SprintWeight = 0.20, PowerWeight = 0.90, EnduranceWeight = 0.75, ExplosiveWeight = 1.25 } },
                    { PositionGroup.Defender,   new PositionCoefficientData { SpeedWeight = 0.75, SprintWeight = 0.45, PowerWeight = 1.25, EnduranceWeight = 1.00, ExplosiveWeight = 0.95 } },
                    { PositionGroup.Midfielder, new PositionCoefficientData { SpeedWeight = 0.60, SprintWeight = 0.55, PowerWeight = 1.00, EnduranceWeight = 1.30, ExplosiveWeight = 1.00 } },
                    { PositionGroup.Forward,    new PositionCoefficientData { SpeedWeight = 0.65, SprintWeight = 0.75, PowerWeight = 1.00, EnduranceWeight = 0.85, ExplosiveWeight = 1.20 } },
                    { PositionGroup.Winger,     new PositionCoefficientData { SpeedWeight = 0.90, SprintWeight = 0.85, PowerWeight = 0.70, EnduranceWeight = 1.05, ExplosiveWeight = 1.05 } }
                });

        // Принимает конкретную позицию, затем маппит в группу, затем возвращает коэффициент
        public static PositionCoefficientData GetByPosition(Position? position)
        {
            if (position == null)
                return GetDefault();

            var group = PositionMapper.ToGroup(position.Value);
            return Coefficients.TryGetValue(group, out var coef) ? coef : GetDefault();
        }

        public static PositionCoefficientData GetDefault()
        {
            return new PositionCoefficientData
            {
                SpeedWeight = 0.6,
                SprintWeight = 0.4,
                PowerWeight = 1.0,
                EnduranceWeight = 1.0,
                ExplosiveWeight = 1.0
            };
        }
    }
}
