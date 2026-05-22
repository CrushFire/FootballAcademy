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
        public SpeedCompositionBlock SpeedComposition { get; set; } = new();
        public PowerCompositionBlock PowerComposition { get; set; } = new();
        public SprintsCompositionBlock SprintsComposition { get; set; } = new();
        public EnduranceCompositionBlock EnduranceComposition { get; set; } = new();
        public ExplosiveCompositionBlock ExplosiveComposition { get; set; } = new();
    }

    /// <summary>
    /// Веса компонентов 5 осей пятиугольника. Сумма весов в каждой композиции = 1.0.
    /// Все обоснования: Обоснование_коэффициентов/ФормулыПятиугольника_Обоснование.txt
    /// </summary>
    public class SpeedCompositionBlock
    {
        // Speed = w1*norm(MaxSpeed) + w2*norm(AvgSpeed) + w3*norm(HighSpeedRatio).
        // 3 компонента (AvgTop3 убран т.к. дублирует MaxSpeed и упирается в норматив).
        // Литература: Buchheit (peak speed), Bradley (avg/m-min), Bush (HSR distance).
        // PCA Turkish 1st div (PMC 12641997): три независимых компонента физики —
        // (1) explosiveness/acc, (2) high-speed running, (5) average running velocity.
        public double MaxSpeedWeight { get; set; }
        public double AvgSpeedWeight { get; set; }
        public double HighSpeedRatioWeight { get; set; }
    }

    public class PowerCompositionBlock
    {
        // Power = w1*norm(PlayerLoad) + w2*norm(Energy) + w3*norm(MetabolicPower).
        // Литература: PMC 6815086 (PlayerLoad & MetabolicPower r=0.918), Osgnach (metabolic power).
        public double PlayerLoadWeight { get; set; }
        public double EnergyWeight { get; set; }
        public double MetabolicPowerWeight { get; set; }
    }

    public class SprintsCompositionBlock
    {
        // Sprints = w1*norm(SprintRatio) + w2*norm(SprintEffortsPerMin) + w3*norm(MaxSpeed_in_sprint).
        // Литература: Bradley & Ade (integrative approach — volume+frequency+quality).
        public double SprintRatioWeight { get; set; }
        public double SprintEffortsPerMinWeight { get; set; }
        public double SprintMaxSpeedWeight { get; set; }
    }

    public class EnduranceCompositionBlock
    {
        // Endurance = w1*norm(DPM) + w2*norm(AerobicLoad) + w3*norm(HRStability) + w4*norm(LowIntensityRatio) - penalty*HRRedPercent.
        // Литература: Bangsbo SSE 125, Mohr 2014 (HR stability), TopSportsLab (intensity distribution).
        public double DistancePerMinuteWeight { get; set; }
        public double AerobicLoadWeight { get; set; }
        public double HRStabilityWeight { get; set; }
        public double LowIntensityRatioWeight { get; set; }
    }

    public class ExplosiveCompositionBlock
    {
        // Explosive = w1*norm(ExplosiveEffortsPerMin) + w2*norm(MaxAcceleration) + w3*norm(AccelDecelPerSec).
        // Литература: Buchheit 2014 (4 m/s² peak threshold), PMC 6851047 (dec ≥ acc demands).
        public double ExplosiveEffortsPerMinWeight { get; set; }
        public double MaxAccelerationWeight { get; set; }
        public double AccelDecelPerSecWeight { get; set; }
        // Стандарты для нормализации (не зависят от возраста — универсальные пороги).
        public double MaxAccelerationStandard { get; set; }   // Buchheit 2014: 4 m/s² peak threshold
        public double AccelDecelPerSecStandard { get; set; }  // элита 0.025-0.032/сек, целевой пик 0.030
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
        // Средняя скорость за тренировку (км/ч). Литература: 5-9 км/ч для юношеских тренировок.
        public double AvgSpeedStandard { get; set; }
        // Стандарт «стабильного пика» — среднее топ-3 максимальных скоростей. Обычно немного ниже MaxSpeed.
        public double AvgTop3SpeedStandard { get; set; }
        // Доля high-speed дистанции (>19.8 км/ч) от total. Литература: 0.06-0.12 для юношей.
        public double HighSpeedRatioStandard { get; set; }
        public double MaxDistancePerMinuteStandard { get; set; }
        public double MaxPlayerLoadStandard { get; set; }
        // Расход энергии за тренировку (kJ). Литература: Osgnach 2010 — за матч 4000-6000 kJ,
        // тренировка ~50-80% от матча → 300-800 kJ для youth-академии.
        public double EnergyStandard { get; set; }
        // Средняя метаболическая мощность (W/kg). Литература: Osgnach 2010 (профи 10-11),
        // PMC 6394943 (youth/sub-elite 7-9), Astrand (общая физиология).
        public double MetabolicPowerStandard { get; set; }
        public double SprintRatioStandard { get; set; }
        // Спринт-эпизодов в минуту. Литература: youth 9-12/трен ≈ 0.12-0.20/мин (PMC 9977053),
        // элита 23-44/матч ≈ 0.25-0.50/мин.
        public double SprintEffortsPerMinStandard { get; set; }
        public double ExplosiveContributionStandard { get; set; }
    }

    public class FemaleMultipliersBlock
    {
        public double MaxSpeed { get; set; } = 1.0;
        public double AvgSpeed { get; set; } = 1.0;
        public double AvgTop3Speed { get; set; } = 1.0;
        public double HighSpeedRatio { get; set; } = 1.0;
        public double MaxDistancePerMinute { get; set; } = 1.0;
        public double MaxPlayerLoad { get; set; } = 1.0;
        public double Energy { get; set; } = 1.0;
        public double MetabolicPower { get; set; } = 1.0;
        public double SprintRatio { get; set; } = 1.0;
        public double SprintEffortsPerMin { get; set; } = 1.0;
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
