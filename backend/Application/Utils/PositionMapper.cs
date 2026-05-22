using Core.Enums;

namespace Application.Utils
{
    public static class PositionMapper
    {
        public static Position? TryParse(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return Enum.TryParse<Position>(value, true, out var pos) ? pos : null;
        }

        public static PositionGroup ToGroup(Position position)
        {
            return position switch
            {
                Position.GK => PositionGroup.Goalkeeper,

                Position.CB or Position.LB or Position.RB or
                Position.LWB or Position.RWB => PositionGroup.Defender,

                Position.CM or Position.CDM or Position.CAM => PositionGroup.Midfielder,

                Position.LW or Position.RW => PositionGroup.Winger,

                Position.ST or Position.CF or Position.SS => PositionGroup.Forward,

                _ => PositionGroup.Midfielder // дефолт
            };
        }
    }
}
