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
            FillProfileSoftDefaults(_config.ProfileThresholds);
        }

        // Если в JSON для какого-то профиля Soft-порог не задан (равен 0), подставляем
        // Hard × PotentialMultiplier. Если Soft задан явно — оставляем как есть (тонкая настройка).
        private static void FillProfileSoftDefaults(ProfileThresholdsBlock t)
        {
            double k = t.PotentialMultiplier > 0 ? t.PotentialMultiplier : 0.7;

            double Soft(double soft, double hard) => soft > 0 ? soft : hard * k;
            int    SoftI(int soft, int hard)      => soft > 0 ? soft : (int)Math.Round(hard * k);

            t.EnduranceRunner_AerobicLoad_Soft        = Soft(t.EnduranceRunner_AerobicLoad_Soft,        t.EnduranceRunner_AerobicLoad);
            t.PowerPlayer_PlayerLoad_Soft             = Soft(t.PowerPlayer_PlayerLoad_Soft,             t.PowerPlayer_PlayerLoad);
            t.ExplosivePlayer_ExplosiveIndex_Soft     = Soft(t.ExplosivePlayer_ExplosiveIndex_Soft,     t.ExplosivePlayer_ExplosiveIndex);
            t.DynamicPlayer_AccelPerSecond_Soft       = Soft(t.DynamicPlayer_AccelPerSecond_Soft,       t.DynamicPlayer_AccelPerSecond);
            t.StaticPlayer_PlayerLoadPerDistance_Soft = Soft(t.StaticPlayer_PlayerLoadPerDistance_Soft, t.StaticPlayer_PlayerLoadPerDistance);

            t.Sprinter_SprintRatio_Soft    = Soft(t.Sprinter_SprintRatio_Soft,    t.Sprinter_SprintRatio);
            t.Sprinter_AccelPerSecond_Soft = Soft(t.Sprinter_AccelPerSecond_Soft, t.Sprinter_AccelPerSecond);

            t.FlankPlayer_SprintRatio_Soft       = Soft(t.FlankPlayer_SprintRatio_Soft,       t.FlankPlayer_SprintRatio);
            t.FlankPlayer_AccelPerSecond_Soft    = Soft(t.FlankPlayer_AccelPerSecond_Soft,    t.FlankPlayer_AccelPerSecond);
            t.FlankPlayer_DistancePerMinute_Soft = Soft(t.FlankPlayer_DistancePerMinute_Soft, t.FlankPlayer_DistancePerMinute);

            t.DefenderType_Index_Soft        = Soft(t.DefenderType_Index_Soft,        t.DefenderType_Index);
            t.CentralMidfielder_Index_Soft   = Soft(t.CentralMidfielder_Index_Soft,   t.CentralMidfielder_Index);
            t.DefensiveMidfielder_Index_Soft = Soft(t.DefensiveMidfielder_Index_Soft, t.DefensiveMidfielder_Index);
            t.Forward_SprintRatio_Soft       = Soft(t.Forward_SprintRatio_Soft,       t.Forward_SprintRatio);

            t.Offensive_HighIntensityRatio_Soft = Soft(t.Offensive_HighIntensityRatio_Soft, t.Offensive_HighIntensityRatio);
            t.Offensive_SprintEfforts_Soft      = SoftI(t.Offensive_SprintEfforts_Soft,     t.Offensive_SprintEfforts);

            t.Defensive_Index_Soft = Soft(t.Defensive_Index_Soft, t.Defensive_Index);
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
