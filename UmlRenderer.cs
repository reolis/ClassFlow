using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static ClassFlow.UmlData;

namespace ClassFlow
{
    internal class UmlRenderer
    {
        private UmlDiagram diagram;
        private int classWidth = 160;
        private int classHeightBase = 60;
        private int verticalSpacing = 100;
        private int horizontalSpacing = 200;
        private Font font = SystemFonts.DefaultFont;

        public UmlRenderer(UmlDiagram diagram)
        {
            this.diagram = diagram;
        }

        public Bitmap Render(int canvasWidth, int canvasHeight)
        {
            var layout = CalculateLayout();
            var positions = layout.Positions;
            int virtualWidth = layout.Width;
            int virtualHeight = layout.Height;

            float scaleX = (float)canvasWidth / virtualWidth;
            float scaleY = (float)canvasHeight / virtualHeight;
            float scale = Math.Min(scaleX, scaleY);
            if (scale > 1f) scale = 1f;

            float scaledWidth = virtualWidth * scale;
            float scaledHeight = virtualHeight * scale;

            float offsetX = (canvasWidth - scaledWidth) / 2f;
            float offsetY = (canvasHeight - scaledHeight) / 2f;

            Bitmap bmp = new Bitmap(canvasWidth, canvasHeight);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                foreach (var umlClass in diagram.Classes)
                {
                    if (positions.TryGetValue(umlClass.Name, out var rect))
                    {
                        var scaledRect = ScaleRect(rect, scale);
                        scaledRect.Offset((int)offsetX, (int)offsetY);
                        DrawClassBox(g, umlClass, scaledRect, scale);
                    }
                }

                foreach (var rel in diagram.Relations)
                {
                    if (positions.TryGetValue(rel.From, out var from) &&
                        positions.TryGetValue(rel.To, out var to))
                    {
                        var start = ScalePoint(new PointF(from.Left + from.Width / 2f, from.Bottom), scale);
                        var end = ScalePoint(new PointF(to.Left + to.Width / 2f, to.Top), scale);
                        start.X += offsetX;
                        start.Y += offsetY;
                        end.X += offsetX;
                        end.Y += offsetY;

                        DrawArrow(g, start, end, rel.Type);
                    }
                }
            }

            return bmp;
        }

        private void DrawClassBox(Graphics g, UmlClass umlClass, RectangleF rect, float scale)
        {
            float padding = 5f * scale;
            float titleFontSize = 10f * scale;
            float memberFontSize = 9f * scale;
            float titleHeight = 20f * scale;
            float lineSpacing = 15f * scale;
            
            var fontTitle = new Font(font.FontFamily, titleFontSize, FontStyle.Bold);
            var fontMember = new Font(font.FontFamily, memberFontSize);
            
            var pen = new Pen(Color.Black, 1f * scale);
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

            float currentY = rect.Y + padding;

            g.DrawString(umlClass.Name, fontTitle, Brushes.Black, rect.X + padding, currentY);
            currentY += titleHeight;

            g.DrawLine(pen, rect.X + padding, currentY, rect.Right - padding, currentY);
            currentY += padding;

            foreach (var member in umlClass.Members)
            {
                string prefix;
                if (member.Visibility == "+")
                {
                    prefix = "public ";
                }
                else if (member.Visibility == "-")
                {
                    prefix = "private ";
                }
                else if (member.Visibility == "#")
                {
                    prefix = "protected ";
                }
                else
                {
                    prefix = "";
                }

                string text = prefix + member.Name + (member.IsMethod ? "()" : "") + " : " + member.Type;

                g.DrawString(text, fontMember, Brushes.DarkBlue, rect.X + padding, currentY);
                currentY += lineSpacing;
            }
        }

        private void DrawArrow(Graphics g, PointF from, PointF to, string type)
        {
            var pen = new Pen(Color.Black, 2);
            pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 6);
            g.DrawLine(pen, from, to);
        }

        private RectangleF ScaleRect(Rectangle rect, float scale)
        {
            return new RectangleF(
                rect.X * scale,
                rect.Y * scale,
                rect.Width * scale,
                rect.Height * scale
            );
        }

        private PointF ScalePoint(PointF point, float scale)
        {
            return new PointF(point.X * scale, point.Y * scale);
        }

        private (Dictionary<string, Rectangle> Positions, int Width, int Height) CalculateLayout()
        {
            const int maxColumns = 4;
            var result = new Dictionary<string, Rectangle>();
            var placed = new HashSet<string>();
            int xStart = 50, yStart = 50;

            int classW = classWidth;
            int classH = classHeightBase;

            var inherited = new HashSet<string>(
                diagram.Relations
                .Where(r => r.Type == "<|--")
                .Select(r => r.To)
            );

            var baseClasses = diagram.Classes
                .Where(c => !inherited.Contains(c.Name))
                .ToList();

            int currentX = xStart;
            int currentY = yStart;
            int maxX = 0;
            int maxY = 0;

            foreach (var baseClass in baseClasses)
            {
                int baseHeight = classHeightBase + baseClass.Members.Count * 20;
                Rectangle baseRect = new Rectangle(currentX, currentY, classW, baseHeight);
                result[baseClass.Name] = baseRect;
                placed.Add(baseClass.Name);

                maxX = Math.Max(maxX, currentX + classW);
                maxY = Math.Max(maxY, currentY + baseHeight);

                var children = diagram.Relations
                    .Where(r => r.Type == "<|--" && r.From == baseClass.Name)
                    .Select(r => r.To)
                    .ToList();

                int childX = currentX - (children.Count - 1) * (classW + 40) / 2;
                int childY = currentY + baseHeight + verticalSpacing;

                foreach (var childName in children)
                {
                    var childClass = diagram.Classes.FirstOrDefault(c => c.Name == childName);
                    if (childClass == null) continue;

                    int h = classHeightBase + childClass.Members.Count * 20;
                    Rectangle childRect = new Rectangle(childX, childY, classW, h);
                    result[childName] = childRect;
                    placed.Add(childName);

                    maxX = Math.Max(maxX, childX + classW);
                    maxY = Math.Max(maxY, childY + h);

                    childX += classW + 40;
                }

                currentX += classW + horizontalSpacing;
            }

            foreach (var umlClass in diagram.Classes)
            {
                if (placed.Contains(umlClass.Name)) continue;

                int h = classHeightBase + umlClass.Members.Count * 20;
                Rectangle rect = new Rectangle(currentX, currentY, classW, h);
                result[umlClass.Name] = rect;

                maxX = Math.Max(maxX, currentX + classW);
                maxY = Math.Max(maxY, currentY + h);

                currentX += classW + horizontalSpacing;

                if (currentX + classW > maxColumns * (classW + horizontalSpacing))
                {
                    currentX = xStart;
                    currentY += h + verticalSpacing;
                }
            }

            return (result, maxX + 50, maxY + 50);
        }
    }
}