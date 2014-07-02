using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Box2DX.Common;

namespace Car
{
    //содержит вспомогательные функции для прорисовки различных объектов
    public class GraphicsHelper
    {
        public Graphics G;//переменная доступа к низкоуровневым графическим функциям в C#
        PictureBox pb;//элемент формы, на который будет выводится графика

        public float Scale = 10;//показывает сколько пикселей приходится на 1 метр реального мира
        public static Box2DX.Dynamics.Color ConvertColor (System.Drawing.Color SysObjColor )
        {
            return new Box2DX.Dynamics.Color(SysObjColor.R, SysObjColor.G, SysObjColor.B);
        }
        //конструктор
        public GraphicsHelper(PictureBox pb_)
        {
            pb = pb_;
            pb.Image = new Bitmap(pb.Size.Width, pb.Size.Height);
            G = Graphics.FromImage(pb.Image);

            G.ScaleTransform(Scale, Scale);
        }
        public void reInit()
        {
            G = Graphics.FromImage(pb.Image);
            G.ScaleTransform(Scale, Scale);
        }
        //возвращает графическое перо заданного цвета
        public Pen GetPen(Color c)
        {
            return new Pen(c, 1 / Scale);
        }

        //отображает нарисованную графику
        public void UpdateGraphics()
        {
            pb.Invalidate();
        }

        //очищает область прорисовки
        public void Clear()
        {
            G.Clear(System.Drawing.Color.White);
        }

        //рисует прямоугольник с геометрическим центром в точке {xc, yc}, но повернутый вокруг него на угол alpha против часовой стрелки
        public void DrawRectangle(float xc, float yc, float width, float height, float alpha_d, Pen p, bool reset_transform)
        {
            if (reset_transform) Push();

            var dx = width / 2;
            var dy = height / 2;

            G.TranslateTransform(xc, yc);
            G.RotateTransform(alpha_d);

            G.DrawRectangle(p, -dx, -dy, width, height);

            if (reset_transform) Pop();
        }

        //рисует прямоугольник с геометрическим центром в точке {xc, yc}, но повернутый вокруг него на угол alpha против часовой стрелки
        public void FillRectangle(float xc, float yc, float width, float height, float alpha_d, Brush p, bool reset_transform)
        {
            if (reset_transform) Push();

            var dx = width / 2;
            var dy = height / 2;

            G.TranslateTransform(xc, yc);
            G.RotateTransform(alpha_d);

            G.FillRectangle(p, -dx, -dy, width, height);

            if (reset_transform) Pop();
        }

        //рисует машину
        public void DrawCar(float xc, float yc, float width, float length, float alpha_d, float wheelbase, float track, float theta_d)
        {
            Push();

            var pen1 = GetPen(Color.Blue);
            var pen2 = GetPen(Color.Black);

            DrawRectangle(xc, yc, width, length, alpha_d, pen1, false);//корпус

            Push();
            DrawRectangle(track / 2, wheelbase / 2, width / 10, length / 6, theta_d, pen2, false);
            Pop();

            Push();
            DrawRectangle(-track / 2, wheelbase / 2, width / 10, length / 6, theta_d, pen2, false);
            Pop();

            Push();
            DrawRectangle(0, wheelbase / 2 + 0.5f, width / 2, length / 5, 0, pen2, false);
            Pop();

            Push();
            DrawRectangle(track / 2, -wheelbase / 2, width / 10, length / 6, 0, pen2, false);
            Pop();

            Push();
            DrawRectangle(-track / 2, -wheelbase / 2, width / 10, length / 6, 0, pen2, false);
            Pop();

            Push();
            DrawRectangle(0, length / 2.1f, width / 2, width / 20, 0, pen1, false);
            Pop();

            Pop();
        }

        Stack<System.Drawing.Drawing2D.Matrix> transformations = new Stack<System.Drawing.Drawing2D.Matrix>();
        //сохраняет текущюю трансформацию графики
        public void Push()
        {
            transformations.Push(G.Transform);
        }

        //восстанавливает сохраненную ранее трансформацию графики
        public void Pop()
        {
            G.Transform = transformations.Pop();
        }



        public static Bitmap GetLocalViewImage(Bitmap src, int w, int h, Vec2 pos, float alpha, float shift_forward, float scale)
        {
            return GetRegion(src, new Rectangle((int)pos.X  - w / 2, (int)pos.Y  - h / 2, w, h), alpha + (float)System.Math.PI, shift_forward, scale);
        }

        public static Bitmap GetRegion(Bitmap src, Rectangle r, float alpha, float dL, float scale)//чем меньше масштаб, тем "выше" камера
        {
            // Создание картинки и изменение ориентации относительно робота

            var alpha1 = alpha + System.Math.PI / 2;
            var cos = (float)(System.Math.Cos(alpha1));
            var sin = -(float)(System.Math.Sin(alpha1));
            float L = (float)System.Math.Sqrt(r.Width * r.Width + r.Height * r.Height) / scale;
            float dx = (L - r.Width) / 2;
            float dy = (L - r.Height) / 2;

            Bitmap bmp = new Bitmap(r.Width, r.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.TranslateTransform(L / 2 - dx, L / 2 - dy);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(1/Helper.angle_to_rad * alpha*(-1));
            g.TranslateTransform(-L / 2, -L / 2);

            var r1 = new Rectangle(r.X - (int)(dL/scale * cos + dx), r.Y - (int)(- dL/scale * sin + dy), (int)L, (int)L);
           
            // отрисовывает определенную область из картинки
            g.Clear(System.Drawing.Color.White);
            g.DrawImage(src, 0, 0, r1, GraphicsUnit.Pixel);

            // Очистка
            g.Dispose();

            // Возвращает картинку
            return bmp;
        }
    }
}
