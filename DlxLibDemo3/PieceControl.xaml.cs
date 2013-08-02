using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DlxLibDemo3.Model;
using Orientation = DlxLibDemo3.Model.Orientation;

namespace DlxLibDemo3
{
    public partial class PieceControl
    {
        private readonly RotatedPiece _rotatedPiece;
        private readonly double _squareSize;
        private const double BorderWidth = 4;
        private const double HalfBorderWidth = BorderWidth / 2;
        private const double Epsilon = 0.1;

        public PieceControl(RotatedPiece rotatedPiece, double squareSize)
        {
            _rotatedPiece = rotatedPiece;
            _squareSize = squareSize;
            InitializeComponent();

            Width = squareSize * rotatedPiece.Width;
            Height = squareSize * rotatedPiece.Height;

            for (var px = 0; px < rotatedPiece.Width; px++)
            {
                for (var py = 0; py < rotatedPiece.Height; py++)
                {
                    var square = rotatedPiece.SquareAt(px, py);
                    if (square != null)
                    {
                        var rectangle = new Rectangle { Width = squareSize, Height = squareSize };
                        Canvas.SetLeft(rectangle, px * squareSize);
                        Canvas.SetBottom(rectangle, py * squareSize);
                        rectangle.Fill = new SolidColorBrush(square.Colour == Colour.Black ? Colors.Black : Colors.White);
                        PieceCanvas.Children.Add(rectangle);
                    }
                }
            }

            var piece = _rotatedPiece.Piece;
            var unrotatedPieceWidth = piece.Width;
            var unrotatedPieceHeight = piece.Height;

            var outsideEdges = new List<Point>();

            for (var x = 0; x < unrotatedPieceWidth; x++)
            {
                for (var y = 0; y < unrotatedPieceHeight; y++)
                {
                    if (piece.SquareAt(x, y) != null)
                    {
                        DetermineOutsideEdges(outsideEdges, x, y);
                    }
                }
            }

            var combinedOutsideEdges = CombineOutsideEdges(outsideEdges);
            var outsideEdgeLinePoints = CalculateEdgeLinePoints(combinedOutsideEdges);

            // TODO (MASSIVE): the above will calculate the line points for a piece with an orientation of North!
            // - need to adjust these line points for other orientations!
            TransformOutsideEdgeLinePoints(outsideEdgeLinePoints);

            var polyLineSegment = new PolyLineSegment(outsideEdgeLinePoints, true);
            var pathFigure = new PathFigure {StartPoint = outsideEdgeLinePoints.First()};
            pathFigure.Segments.Add(polyLineSegment);
            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            var path = new Path
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(0x00, 0x66, 0xCC)),
                    StrokeThickness = BorderWidth,
                    StrokeEndLineCap = PenLineCap.Square,
                    Data = pathGeometry
                };
            PieceCanvas.Children.Add(path);
        }

        private void TransformOutsideEdgeLinePoints(IList<Point> outsideEdgeLinePoints)
        {
            var piece = _rotatedPiece.Piece;
            var unrotatedPieceWidth = piece.Width;
            var unrotatedPieceHeight = piece.Height;

            for (var i = 0; i < outsideEdgeLinePoints.Count; i++)
            {
                var oldPt = outsideEdgeLinePoints[i];
                var newPt = new Point();

                switch (_rotatedPiece.Orientation)
                {
                    case Orientation.North:
                        newPt.X = oldPt.X;
                        newPt.Y = oldPt.Y;
                        break;

                    case Orientation.South:
                        newPt.X = (unrotatedPieceWidth * _squareSize) - oldPt.X;
                        newPt.Y = (unrotatedPieceHeight * _squareSize) - oldPt.Y;
                        break;

                    case Orientation.East:
                        newPt.X = (unrotatedPieceHeight * _squareSize) - oldPt.Y;
                        newPt.Y = oldPt.X;
                        break;

                    case Orientation.West:
                        newPt.X = oldPt.Y;
                        newPt.Y = (unrotatedPieceWidth * _squareSize) - oldPt.X;
                        break;
                }

                outsideEdgeLinePoints[i] = newPt;
            }
        }

        private enum Side
        {
            Top,
            Bottom,
            Left,
            Right
        };

        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        };

        private void DetermineOutsideEdges(ICollection<Point> outsideEdges, int x, int y)
        {
            var piece = _rotatedPiece.Piece;
            var unrotatedPieceWidth = piece.Width;
            var unrotatedPieceHeight = piece.Height;

            foreach (var side in Enum.GetValues(typeof (Side)).Cast<Side>())
            {
                var isOutsideEdge = false;

                switch (side)
                {
                    case Side.Top:
                        if (y + 1 >= unrotatedPieceHeight) {
                            isOutsideEdge = true;
                        }
                        else {
                            if (piece.SquareAt(x, y + 1) == null) {
                                isOutsideEdge = true;
                            }
                        }
                        if (isOutsideEdge) {
                            outsideEdges.Add(new Point(x, y + 1));
                            outsideEdges.Add(new Point(x + 1, y + 1));
                        }
                        break;

                    case Side.Right:
                        if (x + 1 >= unrotatedPieceWidth) {
                            isOutsideEdge = true;
                        }
                        else {
                            if (piece.SquareAt(x + 1, y) == null) {
                                isOutsideEdge = true;
                            }
                        }
                        if (isOutsideEdge) {
                            outsideEdges.Add(new Point(x + 1, y + 1));
                            outsideEdges.Add(new Point(x + 1, y));
                        }
                        break;

                    case Side.Bottom:
                        if (y == 0) {
                            isOutsideEdge = true;
                        }
                        else {
                            if (piece.SquareAt(x, y - 1) == null) {
                                isOutsideEdge = true;
                            }
                        }
                        if (isOutsideEdge) {
                            outsideEdges.Add(new Point(x + 1, y));
                            outsideEdges.Add(new Point(x, y));
                        }
                        break;

                    case Side.Left:
                        if (x == 0) {
                            isOutsideEdge = true;
                        }
                        else {
                            if (piece.SquareAt(x - 1, y) == null) {
                                isOutsideEdge = true;
                            }
                        }
                        if (isOutsideEdge) {
                            outsideEdges.Add(new Point(x, y));
                            outsideEdges.Add(new Point(x, y + 1));
                        }
                        break;
                }
            }
        }

        private static IList<Point> CombineOutsideEdges(IList<Point> outsideEdges)
        {
            var combinedOutsideEdges = new List<Point>();

            var firstLineStartPoint = outsideEdges[0];
            var firstLineEndPoint = outsideEdges[1];

            combinedOutsideEdges.Add(firstLineStartPoint);

            var currentLineEndPoint = firstLineEndPoint;

            for (; ; )
            {
                Point nextLineStartPoint;
                Point nextLineEndPoint;

                FindNextLine(outsideEdges, currentLineEndPoint, out nextLineStartPoint, out nextLineEndPoint);

                combinedOutsideEdges.Add(nextLineStartPoint);
                currentLineEndPoint = nextLineEndPoint;

                if (Math.Abs(nextLineEndPoint.X - firstLineStartPoint.X) < Epsilon && Math.Abs(nextLineEndPoint.Y - firstLineStartPoint.Y) < Epsilon) {
                    break;
                }
            }

            combinedOutsideEdges.Add(firstLineStartPoint);

            return combinedOutsideEdges;
        }

        private static void FindNextLine(IList<Point> outsideEdges, Point currentLineEndPoint, out Point nextLineStartPoint, out Point nextLineEndPoint)
        {
            nextLineStartPoint = new Point();
            nextLineEndPoint = new Point();

            var numLines = outsideEdges.Count / 2;

            for (var i = 0; i < numLines; i++)
            {
                var pt1 = outsideEdges[i * 2];
                var pt2 = outsideEdges[i * 2 + 1];

                if (Math.Abs(pt1.X - currentLineEndPoint.X) < Epsilon && Math.Abs(pt1.Y - currentLineEndPoint.Y) < Epsilon)
                {
                    nextLineStartPoint = pt1;
                    nextLineEndPoint = pt2;
                    return;
                }
            }

            throw new InvalidOperationException("FindNextLine failed to find the next line!");
        }

        private IList<Point> CalculateEdgeLinePoints(IList<Point> combinedOutsideEdges)
        {
            var outsideEdgeLinePoints = new List<Point>();

            var numCoords = combinedOutsideEdges.Count;

            var firstCoords = combinedOutsideEdges.First();
    
            var lastXCoord = firstCoords.X;
            var lastYCoord = firstCoords.Y;
    
            var temporaryFirstPoint = new Point();
            outsideEdgeLinePoints.Add(temporaryFirstPoint);

            for (var i = 1; i < numCoords; i++)
            {
                var isLastCoord = (i == numCoords - 1);

                var thisCoords = combinedOutsideEdges[i];
                var nextCoords = (!isLastCoord) ? combinedOutsideEdges[i + 1] : combinedOutsideEdges[1];

                var thisXCoord = thisCoords.X;
                var thisYCoord = thisCoords.Y;

                var nextXCoord = nextCoords.X;
                var nextYCoord = nextCoords.Y;

                var thisDirection = DetermineDirection(lastXCoord, lastYCoord, thisXCoord, thisYCoord);
                var nextDirection = DetermineDirection(thisXCoord, thisYCoord, nextXCoord, nextYCoord);

                var unrotatedPieceHeight = _rotatedPiece.Piece.Height;
                var pt = new Point {X = thisXCoord * _squareSize, Y = (unrotatedPieceHeight - thisYCoord) * _squareSize};

                switch (thisDirection)
                {
                    case Direction.Up:
                        pt.X += HalfBorderWidth;
                        switch (nextDirection) {
                            case Direction.Left:
                                pt.Y -= HalfBorderWidth;
                                break;
                            case Direction.Right:
                                pt.Y += HalfBorderWidth;
                                break;
                        }
                    break;

                    case Direction.Down:
                        pt.X -= HalfBorderWidth;
                        switch (nextDirection) {
                            case Direction.Left:
                                pt.Y -= HalfBorderWidth;
                                break;
                            case Direction.Right:
                                pt.Y += HalfBorderWidth;
                                break;
                        }
                        break;

                    case Direction.Left:
                        pt.Y -= HalfBorderWidth;
                        switch (nextDirection) {
                            case Direction.Up:
                                pt.X += HalfBorderWidth;
                                break;
                            case Direction.Down:
                                pt.X -= HalfBorderWidth;
                                break;
                        }
                        break;

                    case Direction.Right:
                        pt.Y += HalfBorderWidth;
                        switch (nextDirection) {
                            case Direction.Up:
                                pt.X += HalfBorderWidth;
                                break;
                            case Direction.Down:
                                pt.X -= HalfBorderWidth;
                                break;
                        }
                        break;
                }
                
                outsideEdgeLinePoints.Add(pt);
        
                lastXCoord = thisXCoord;
                lastYCoord = thisYCoord;
            }

            outsideEdgeLinePoints[0] = outsideEdgeLinePoints.Last();

            return outsideEdgeLinePoints;
        }

        private static Direction DetermineDirection(double x1, double y1, double x2, double y2)
        {
            if (Math.Abs(x1 - x2) < Epsilon)
            {
                return y2 > y1 ? Direction.Up : Direction.Down;
            }

            if (Math.Abs(y1 - y2) < Epsilon)
            {
                return x2 > x1 ? Direction.Right : Direction.Left;
            }

            throw new InvalidOperationException(string.Format("DetermineDirection - cannot determine direction given ({0},{1}) & ({2},{3})", x1, y1, x2, y2));
        }
    }
}
