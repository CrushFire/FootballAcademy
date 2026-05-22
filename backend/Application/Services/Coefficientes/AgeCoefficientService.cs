using Core.Models.MetricModel.Coefficientes;

namespace Application.Services.Coefficientes
{
    // Условно мидлвеар - выбирает мужской или женский набор коэффициентов по полу спортсмена.
    // gender: M — мужской, F — женский. Любое другое значение - мужской по умолчанию.
    public static class AgeCoefficientService
    {
        public static AgeCoefficientData GetByAgeAndGender(int age, char gender)
        {
            if (gender == 'F')
                return FemaleAgeCoefficientService.GetByAge(age) ?? FemaleAgeCoefficientService.GetDefault();

            return MaleAgeCoefficientService.GetByAge(age) ?? MaleAgeCoefficientService.GetDefault();
        }

        public static AgeCoefficientData GetByAge(int age) =>
            MaleAgeCoefficientService.GetByAge(age) ?? MaleAgeCoefficientService.GetDefault();

        public static AgeCoefficientData GetDefault() =>
            MaleAgeCoefficientService.GetDefault();
    }
}
