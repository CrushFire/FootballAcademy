using System.Collections.Generic;

namespace Core.Models.Config
{
    /// <summary>
    /// Загружается ОДИН РАЗ при старте бэка из appsettings.Coefficients.json.
    /// Для применения изменений нужен рестарт бэка.
    /// </summary>
    public class CoefficientsConfig
    {
        public AgeCoefficientsBlock AgeCoefficients { get; set; } = new();
        public AbsoluteStandardsBlock AbsoluteStandards { get; set; } = new();
        public MedicalThresholdsBlock MedicalThresholds { get; set; } = new();
        public PentagonNormalizationBlock PentagonNormalization { get; set; } = new();
        public ProfileThresholdsBlock ProfileThresholds { get; set; } = new();
    }

    public class AgeCoefficientsBlock
    {
        public List<AgeCoefficientEntry> Male { get; set; } = new();
        public AgeCoefficientEntry MaleDefault { get; set; } = new();
        public List<AgeCoefficientEntry> Female { get; set; } = new();
        public AgeCoefficientEntry FemaleDefault { get; set; } = new();
    }

    public class AgeCoefficientEntry
    {
        public string? AgeGroup { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double SpeedFactor { get; set; }
        public double PowerFactor { get; set; }
        public double EnduranceFactor { get; set; }
        public double ExplosiveFactor { get; set; }
        public int MaxHeartRateBase { get; set; }
    }

    public class AbsoluteStandardsBlock
    {
        public List<AbsoluteStandardEntry> ByAge { get; set; } = new();
        public FemaleMultipliersBlock FemaleMultipliers { get; set; } = new();
        public AbsoluteStandardEntry Default { get; set; } = new();
    }

    public class AbsoluteStandardEntry
    {
        public string? AgeGroup { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double MaxSpeedStandard { get; set; }
        public double MaxDistancePerMinuteStandard { get; set; }
        public double MaxPlayerLoadStandard { get; set; }
        public double SprintRatioStandard { get; set; }
        public double ExplosiveContributionStandard { get; set; }
    }

    public class FemaleMultipliersBlock
    {
        public double MaxSpeed { get; set; } = 1.0;
        public double MaxDistancePerMinute { get; set; } = 1.0;
        public double MaxPlayerLoad { get; set; } = 1.0;
        public double SprintRatio { get; set; } = 1.0;
        public double ExplosiveContribution { get; set; } = 1.0;
    }

    public class MedicalThresholdsBlock
    {
        public double LoadCapBase { get; set; } = 500;
        public double InjuryLoadCapBase { get; set; } = 450;
        public double AccDecCapBase { get; set; } = 200;
        public double RedZoneCapBase { get; set; } = 600;
        public double ConsistencyCapBase { get; set; } = 20;
        public int AverageHeartRateMax { get; set; } = 180;
        public int MinRecoveryTimeSeconds { get; set; } = 300;
        public double AcuteChronicWarn { get; set; } = 1.3;
        public double AcuteChronicDanger { get; set; } = 1.5;
        public double FatigueIndexMax { get; set; } = 1.2;
    }

    public class PentagonNormalizationBlock
    {
        public int HighGroupCount { get; set; } = 30;
        public int MediumGroupCount { get; set; } = 15;
        public int LowGroupCount { get; set; } = 5;
        public NormalizationWeights HighWeights { get; set; } = new() { Relative = 0.8, Absolute = 0.2 };
        public NormalizationWeights MediumWeights { get; set; } = new() { Relative = 0.7, Absolute = 0.3 };
        public NormalizationWeights LowWeights { get; set; } = new() { Relative = 0.5, Absolute = 0.5 };
        public NormalizationWeights FallbackWeights { get; set; } = new() { Relative = 0.2, Absolute = 0.8 };
        public double EndurancePenaltyPerRedZonePercent { get; set; } = 0.10;
    }

    public class NormalizationWeights
    {
        public double Relative { get; set; }
        public double Absolute { get; set; }
    }

    /// <summary>
    /// Пороги активации профилей в ProfileService.
    /// Все значения берутся из appsettings.Coefficients.json — секция ProfileThresholds.
    /// Подобраны как ~среднее БД × 1.25-1.30 для каждой метрики (см. ПорогиПрофилейИгрока_Обоснование.txt).
    /// </summary>
    public class ProfileThresholdsBlock
    {
        // Логика: метрика > Hard → active. метрика > Soft (но не > Hard) → potential.
        // По умолчанию Soft = Hard × PotentialMultiplier (0.7). Значения Soft в JSON предварительно
        // рассчитаны под этот множитель, но КАЖДЫЙ Soft можно тонко настроить отдельно (исключение для профиля).
        // PotentialMultiplier — справочный множитель: храним для документации и пересчёта при изменении Hard.

        public double PotentialMultiplier { get; set; }

        // Простые правила (Hard / Soft)
        public double EnduranceRunner_AerobicLoad { get; set; }
        public double EnduranceRunner_AerobicLoad_Soft { get; set; }
        public double PowerPlayer_PlayerLoad { get; set; }
        public double PowerPlayer_PlayerLoad_Soft { get; set; }
        public double ExplosivePlayer_ExplosiveIndex { get; set; }
        public double ExplosivePlayer_ExplosiveIndex_Soft { get; set; }
        public double DynamicPlayer_AccelPerSecond { get; set; }
        public double DynamicPlayer_AccelPerSecond_Soft { get; set; }
        public double StaticPlayer_PlayerLoadPerDistance { get; set; }
        public double StaticPlayer_PlayerLoadPerDistance_Soft { get; set; }

        // Sprinter
        public double Sprinter_SprintRatio { get; set; }
        public double Sprinter_SprintRatio_Soft { get; set; }
        public double Sprinter_AccelPerSecond { get; set; }
        public double Sprinter_AccelPerSecond_Soft { get; set; }

        // FlankPlayer
        public double FlankPlayer_SprintRatio { get; set; }
        public double FlankPlayer_SprintRatio_Soft { get; set; }
        public double FlankPlayer_AccelPerSecond { get; set; }
        public double FlankPlayer_AccelPerSecond_Soft { get; set; }
        public double FlankPlayer_DistancePerMinute { get; set; }
        public double FlankPlayer_DistancePerMinute_Soft { get; set; }
        public double FlankPlayer_SpeedWeightMin { get; set; }

        // DefenderType
        public double DefenderType_Index { get; set; }
        public double DefenderType_Index_Soft { get; set; }
        public double DefenderType_PowerWeightMin { get; set; }

        // CentralMidfielder
        public double CentralMidfielder_Index { get; set; }
        public double CentralMidfielder_Index_Soft { get; set; }
        public double CentralMidfielder_EnduranceWeightMin { get; set; }

        // DefensiveMidfielder
        public double DefensiveMidfielder_Index { get; set; }
        public double DefensiveMidfielder_Index_Soft { get; set; }

        // AttackingMidfielder (окно — без soft, окно само мягкое)
        public double AttackingMidfielder_SprintRatioMin { get; set; }
        public double AttackingMidfielder_SprintRatioMax { get; set; }

        // Forward
        public double Forward_SprintRatio { get; set; }
        public double Forward_SprintRatio_Soft { get; set; }
        public double Forward_SprintWeightMin { get; set; }

        // Offensive
        public double Offensive_HighIntensityRatio { get; set; }
        public double Offensive_HighIntensityRatio_Soft { get; set; }
        public int Offensive_SprintEfforts { get; set; }
        public int Offensive_SprintEfforts_Soft { get; set; }

        // Defensive
        public double Defensive_Index { get; set; }
        public double Defensive_Index_Soft { get; set; }

        // Universal — добавляется если active.Count >= UniversalMinActive
        public int Universal_MinActiveCount { get; set; }
    }
}
