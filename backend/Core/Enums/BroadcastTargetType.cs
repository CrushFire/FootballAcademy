namespace Core.Enums
{
    public enum BroadcastTargetType
    {
        All,        // все спортсмены
        Personal,   // все сотрудники (personal: trainer + medical)
        Team,       // конкретная команда
        Group,      // конкретная группа
        Individual, // конкретный пользователь
        Trainers,   // только тренеры (personal.Type = Trainer)
        Medical     // только мед. персонал (personal.Type = Medical)
    }
}
