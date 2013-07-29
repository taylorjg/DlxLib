namespace DlxLibDemo3.Model
{
    public enum Colour
    {
        Black,
        White
    }

    public static class ColourExtensions
    {
        public static Colour OppositeColour(this Colour colour)
        {
            return (colour == Colour.Black) ? Colour.White : Colour.Black;
        }

        public static Colour RelativeColour(this Colour colour, int x, int y)
        {
            var sumOfCoordinates = x + y;
            var sumOfCoordinatesIsEven = sumOfCoordinates % 2 == 0;
            return (sumOfCoordinatesIsEven) ? colour : colour.OppositeColour();
        }
    }
}
