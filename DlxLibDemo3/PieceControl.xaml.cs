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
        private const double BorderWidth = 8; // half of this width will be clipped away

        public PieceControl(RotatedPiece rotatedPiece, double squareSize)
        {
            _rotatedPiece = rotatedPiece;
            _squareSize = squareSize;
            InitializeComponent();

            Width = squareSize * rotatedPiece.Width;
            Height = squareSize * rotatedPiece.Height;

            var clipGeometryGroup = new GeometryGroup();

            for (var px = 0; px < rotatedPiece.Width; px++)
            {
                for (var py = 0; py < rotatedPiece.Height; py++)
                {
                    var square = rotatedPiece.SquareAt(px, py);
                    if (square != null)
                    {
                        var rect = new Rect(px * squareSize, (rotatedPiece.Height - py - 1) * squareSize, squareSize, squareSize);
                        var rectangle = new Rectangle { Width = rect.Width, Height = rect.Height };
                        Canvas.SetLeft(rectangle, rect.Left);
                        Canvas.SetTop(rectangle, rect.Top);
                        rectangle.Fill = new SolidColorBrush(square.Colour == Colour.Black ? Colors.Black : Colors.White);
                        PieceCanvas.Children.Add(rectangle);
                        var clipRectangleGeometry = new RectangleGeometry(rect);
                        clipGeometryGroup.Children.Add(clipRectangleGeometry);
                    }
                }
            }

            var piece = _rotatedPiece.Piece;
            var unrotatedPieceWidth = piece.Width;
            var unrotatedPieceHeight = piece.Height;

            var outsideEdges = new List<Coords>();

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
            PieceCanvas.Clip = clipGeometryGroup;
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

        private void DetermineOutsideEdges(ICollection<Coords> outsideEdges, int x, int y)
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
                            outsideEdges.Add(new Coords(x, y + 1));
                            outsideEdges.Add(new Coords(x + 1, y + 1));
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
                            outsideEdges.Add(new Coords(x + 1, y + 1));
                            outsideEdges.Add(new Coords(x + 1, y));
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
                            outsideEdges.Add(new Coords(x + 1, y));
                            outsideEdges.Add(new Coords(x, y));
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
                            outsideEdges.Add(new Coords(x, y));
                            outsideEdges.Add(new Coords(x, y + 1));
                        }
                        break;
                }
            }
        }

        private static IEnumerable<Coords> CombineOutsideEdges(IList<Coords> outsideEdges)
        {
            var combinedOutsideEdges = new List<Coords>();

            var firstLineStartCoords = outsideEdges[0];
            var firstLineEndCoords = outsideEdges[1];

            combinedOutsideEdges.Add(firstLineStartCoords);

            var currentLineEndCoords = firstLineEndCoords;

            for (; ; )
            {
                Coords nextLineStartCoords;
                Coords nextLineEndCoords;

                FindNextLine(outsideEdges, currentLineEndCoords, out nextLineStartCoords, out nextLineEndCoords);

                combinedOutsideEdges.Add(nextLineStartCoords);
                currentLineEndCoords = nextLineEndCoords;

                if (nextLineEndCoords.X == firstLineStartCoords.X && nextLineEndCoords.Y == firstLineStartCoords.Y) {
                    break;
                }
            }

            combinedOutsideEdges.Add(firstLineStartCoords);

            return combinedOutsideEdges;
        }

        private static void FindNextLine(IList<Coords> outsideEdges, Coords currentLineEndCoords, out Coords nextLineStartCoords, out Coords nextLineEndCoords)
        {
            var numLines = outsideEdges.Count / 2;

            for (var i = 0; i < numLines; i++)
            {
                var pt1 = outsideEdges[i * 2];
                var pt2 = outsideEdges[i * 2 + 1];

                if (pt1.X == currentLineEndCoords.X && pt1.Y == currentLineEndCoords.Y)
                {
                    nextLineStartCoords = pt1;
                    nextLineEndCoords = pt2;
                    return;
                }
            }

            throw new InvalidOperationException("FindNextLine failed to find the next line!");
        }

        private IList<Point> CalculateEdgeLinePoints(IEnumerable<Coords> combinedOutsideEdges)
        {
            return combinedOutsideEdges.Select(coords => new Point
                {
                    X = coords.X * _squareSize,
                    Y = (_rotatedPiece.Piece.Height - coords.Y) * _squareSize
                }).ToList();
        }
    }
}
