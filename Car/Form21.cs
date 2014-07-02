using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Box2DX.Dynamics;
using Box2DX.Common;

namespace Car
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //!!!!!!!!!!!!создание конечной точки
        static float k_PI = (float)System.Math.PI / 180; //57.296f
        GraphicsHelper gh, ghover; //графические помощники для отрисовки среды+робота и для траектории+весов узлов
        Physics ph; //физический помощник
        Car car; //машина (и физика и графика)
        Agent carAgent; // агент (управление) машины
        Environment env; //описание среды
       
        private void Form1_Load(object sender, EventArgs e)
        {
            //инциализируем отрисовку и физику
            gh = new GraphicsHelper(pictureBox1);
            ghover = new GraphicsHelper(pictureBoxOverlay);
            ph = new Physics();
            Vec2 target = new Vec2(75F, 6f); //Координаты конечной точки

            // рожаем машину
            var p = new CarParams();
            p.InitDefault(); //Задает машине параметры по умолчанию
            p.h = 2.5f; //Поправляем форму машинки
            p.h_base = 2f;
            var ps = new Pose() { xc = 5, yc = 15, angle_rad = 30 * Helper.angle_to_rad }; //Задаем начальное положение машины
            car = new Car(p, 0);
            car.Create(ph, ps);

            //создание агента и среды
            carAgent = new LICStreeAgent(ps,target);
            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            env = new Environment(ph, car, target,pictureBox2);

            //создание объектов среды
            //ВАЖНО!!! -- в каком порядке объекты создаются, в таком они и отрисовываются, поэтому в первую очередь создаются объекты заднего плана.
            //занятно...-- НЕсамообучающийся по картинке агент ооочень туго переваривает динамические препятствия. Хотя, всё же, переваривает.
            
            env.CreateRectZone(new Pose { xc = 30f, yc = 23f, angle_rad = 45 * k_PI }, new Vec2(15f, 25f), 2f);
            env.CreateRectZone(new Pose { xc = 70f, yc = 20f, angle_rad = 45 * k_PI }, new Vec2(10f, 5f), 3f);
            env.CreateWall( new Pose { xc = 25f, yc = 17f, angle_rad = 20 * k_PI },new Vec2(2f, 10f));
            env.CreateWall(new Pose { xc = 20f, yc = 35f, angle_rad = 35 * k_PI }, new Vec2(10f, 10f));
            env.CreateWall(new Pose { xc = 40f, yc = 10f, angle_rad = 35 * k_PI }, new Vec2(40f, 1f));
            env.CreateWall( new Pose { xc = 55f, yc = 30f, angle_rad = 100 * k_PI },new Vec2(2f, 10f));
            env.CreateWall(new Pose { xc = 65f, yc = 45f, angle_rad = 0 * k_PI }, new Vec2(15f, 15f));
            env.CreateWall(new Pose { xc = 25f, yc = 25f, angle_rad = -195 * k_PI }, new Vec2(20f, 1f));
            env.CreateWall(new Pose { xc = 50f, yc = 20f, angle_rad = 105 * k_PI }, new Vec2(2f, 7f));
            env.CreateWall(new Pose { xc = 58f, yc = 18f, angle_rad = 45 * k_PI }, new Vec2(10f, 5f));

            env.CreateWall(new Pose { xc = pictureBox1.Width/20f, yc = pictureBox1.Height/10f, angle_rad = 90 * k_PI }, new Vec2(1f, 10000f));
            env.CreateWall(new Pose { xc = pictureBox1.Width / 20f, yc = 0/10f, angle_rad = 90 * k_PI }, new Vec2(1f, 10000f));
            env.CreateWall(new Pose { xc = pictureBox1.Width / 10f, yc = pictureBox1.Height / 20f, angle_rad = 0 * k_PI }, new Vec2(1f, 10000f));
            env.CreateWall(new Pose { xc = 0/10f, yc = pictureBox1.Height / 20f, angle_rad = 0 * k_PI }, new Vec2(1f, 10000f));
            
            //env.CreateCircle(new Pose { xc = 25f, yc = 20f, angle_rad = -10 * k_PI }, 3, 10);
            //env.CreateFire(new Pose { xc = 0f, yc = 0f, angle_rad = -10 * k_PI },5);
            
        }


        
        float t = 0;
        // несколько раз в секунду рисуем новый кадр анимации
        private void timer1_Tick(object sender, EventArgs e)
        {
            float dt = timer1.Interval / 1000f;
            t += dt;

            env.act(carAgent.react(env.percept(), t)); // вот он цикл работы агента
            env.step(dt);
            // рисуем
            env.draw(gh);



            /*отображение геометрии и узлов поиска пути*/
            double ox = 96.0 / ((dreamerAgent)carAgent).pr.dx;
            double oy = 52.0 / ((dreamerAgent)carAgent).pr.dy;
            double xs = 1;
            double ys = 1;
            if (showGeometryBox.Checked)
                pictureBoxOverlay.Image = (Image)pictureBox1.Image.Clone();
            else
                pictureBoxOverlay.Image = new Bitmap(pictureBoxOverlay.Width,pictureBoxOverlay.Height);
            ghover.reInit();
            if (showChangedWeightsBox.Checked)
            {
                foreach (state sp in ((dreamerAgent)carAgent).infs)
                    ghover.FillRectangle((float)(xs + ox * sp.x), (float)(ys + oy * sp.y), 0.5F, 0.5F, 0, System.Drawing.Brushes.Red, true);
                foreach (state sp in ((dreamerAgent)carAgent).obsts.Keys)
                    ghover.FillRectangle((float)(xs + ox * sp.x), (float)(ys + oy * sp.y), 0.5F, 0.5F, 0, new System.Drawing.SolidBrush(((dreamerAgent)carAgent).obsts[sp]), true);
            }
            if (showTrajectoryBox.Checked)
            {
                ghover.FillRectangle((float)(xs + ox * ((dreamerAgent)carAgent).x), (float)(ys + oy * ((dreamerAgent)carAgent).y), 0.7F, 0.7F, 0, System.Drawing.Brushes.Blue, true);
                foreach (state sp in ((dreamerAgent)carAgent).pathGone)
                    ghover.FillRectangle((float)(xs + ox * sp.x), (float)(ys + oy * sp.y), 0.3F, 0.3F, 0, System.Drawing.Brushes.DarkGreen, true);
                foreach (state sp in ((dreamerAgent)carAgent).path)
                    ghover.FillRectangle((float)(xs + ox * sp.x), (float)(ys + oy * sp.y), 0.3F, 0.3F, 0, System.Drawing.Brushes.Blue, true);
             
            }


            /*Конец отображения геометрии и узлов поиска пути*/

            /*СОХРАНЕНИЕ ЛОГ КАРТИНОК*/
            PictureBox pictureB = pictureBox1;
            if (camDebugBox.Checked) pictureB = pictureBoxOverlay;

            pictureBox2.Image = GraphicsHelper.GetLocalViewImage((Bitmap)pictureB.Image/*картинка с общего вида*/, 
                                         pictureBox2.Width, pictureBox2.Height, car.body.GetPosition()*gh.Scale, car.body.GetAngle(), 50f, 0.5f);//запилить в один бинарный файл картинки
           
           pictureBox2.Image.Save("img/" + String.Format("{0:000.000}", System.Math.Round(t, 3)) + ".bmp");//получаем картинку относительно робота
            cnt++;
           
        }

        int cnt = 0;

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((reflexAgent)carAgent).ClearResources();
        }

    }
}
