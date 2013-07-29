namespace DlxLibDemo3.Model
{
    public class Square
    {
        public Square(int x, int y, Colour colour)
        {
            X = x;
            Y = y;
            Colour = colour;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public Colour Colour { get; private set; }
    }
}
