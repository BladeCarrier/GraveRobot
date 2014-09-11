using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Box2DX.Common;
using System.Drawing;
using System.Threading;
namespace Car
{
    /*
     * Что мы обучаем: 
     * в методе void intelligentSystemCalculateWeights(PerceptParams pp, float t) стоит задача на основании
     *  -PerceptParams, t
     *  -внутренних полей и методов базового класса
     *  -выбранной самообучающейся системы
     * через метод Problem.setCost(int x, int y, double cost) устанавливать и обновлять веса тайлов так, чтобы
     * минимизировать среднее время прохождения роботом от начальной позиции до конечной.
     * Для визуализации и в целях отладки может иметь смысл добавлять отмеченные тайлы в obsts, препятствия в infs,
     * используя автоматический вывод на их на экран.
     * 
     * Если вы также желаете оптимизировать скорость патфайндинга за счёт эвристики, её вы можете прописать через
     * метод Problem.setHeuristic(state sFrom, state sTo, double cost), либо более полно - создав собственный класс
     * наследник Prbl и прописав его в конструкторе SekouNeuroAgent
     */
    class NotLearningAgent : dreamerAgent
    {
        public LicsDataWindow dataWnd;
        public NotLearningAgent(LicsDataWindow dataW,Pose initial, Vec2 target)
            : base(initial, target)
        {
            /* Pose initial - начальное положение машины в системе Box2d с перевёрнутым углом (0 - слева, + против часовой),
             * Vec2 target - положение цели в системе Box2d,
             * int xtiles = 80, int ytiles = 50 - ширина и высота массива тайлов для поиска пути. Глубина определяется количеством ориентаций(8).
             */

            //initialising data window
            dataWnd = dataW;
            pos = initial;
          
            // initialisation end
        }
        public override void initAgent()
        {
            base.initAgent();
            dataWnd.Show();
            
        }
        /* Содержание dreamerAgent. Работать с ними условимся только в СВОИХ классах, ибо перемены в DreamAgent для 1 наследника могут вывести из строя другой.
         * public Prbl pr  - описание проблемы
         * public DStar ds - система динамического поиска пути.
         * double angle_last = 0, tLast =0 - угол и время на прошлом кадре - для дифференциалов
         * ox = 80.0 / [число тайлов у проблемы в ширину] - расстояние между центрами тайлов по горизонтали, ширина тайла
         * oy = 80.0 / число тайлов у проблемы в высоту - расстояние между центрами тайлов по вертикали, высота тайла
         * xs = 1.0 - абсцисса нулевого (левого) столбца тайлов, расстояние от последнего столбца до края картинки
         * ys = 1.0 - ордината нулевой (верхней) строки тайлов, расстояние от последней строки до края картинки
         * bool goingBack - если истинно, робот перемещается задом. P и D коэффициенты не работают в этом режиме, угол выбирается как сигмоида от расхождения в углах корпуса и направления на цель.
         * int x,y,orientation a - координаты текущего состояния робота в problem.states.
         * List<state> path - система помещает сюда актуальный путь от текущей до конечной точек следования робота. Регулятор работает именно на ЭТОТ путь.
         * List<state> infs - список всех тайлов, которые АГЕНТ СЧИТАЕТ НЕПРЕОДОЛИМЫМИ.
         * Dictionary<state,System.Drawing.Color> obsts - список всех тайлов, базовый вес которых агент определил как ОТЛИЧНЫЙ ОТ 1. Значения словаря - цвет отрисовки.
         * int xn, yn - координаты переднего (пока - единственного) "щупа" агента в системе координат поиска пути.
         * double P = 2, D = 0.3; - PD-регулятор угла поворота и скорости при движении вперёд.
         * orient orienter - хелпер для работы с orientation.
         * enum orientation - все возможные направления
         * Pvel = 0.02, Ivel = 0.01, Dvel = 0.001 - PID регулятор газа по скорости
         * double lastdVel, sumdVel,maxVel = 50,targVel =30; - параметры регуляции по скорости
         */
        Pose pos;
        //measurement data
        int numCollisions = 0,
            numMeasures = 0;
        double pathDistance = 0,
               maxSpeed = 0,
               sumPassability = 1;
        bool goingBack= false;
        bool finished = false;
        //end of measurements
        public override Control react(PerceptParams pp, float t)
        {
            //ВАЖНО!!! для чистоты эксперимента необходимо, чтобы ОТРАБОТКА траектории, а значит, и
            //параметры регулятора и механизм вывода CarControl, были одинаковые или выбирались на равных
            //условиях во всех 7(план максимум) сериях экспериментов: контрольная, сети по базе, деревья по базе,
            //сети с ходу, деревья с ходу, комбинация по базе, комбинация с ходу.

            //measures:
            if (!finished)
            {
                if (path.Count <= 1)
                    finished = true;

                numMeasures += 1;
                pathDistance += System.Math.Sqrt(System.Math.Pow(pos.xc - pp.p.xc, 2) + System.Math.Pow(pos.yc - pp.p.yc, 2));//distance of travel.
                sumPassability += System.Math.Min(pp.ksurf, 1000);
                if (pp.velocity > maxSpeed) maxSpeed = pp.velocity;
                if (t - tBack < goBackOnCollisionTimer)
                {
                    if (!goingBack)
                    {
                        goingBack = true;
                        numCollisions++;
                    }
                }
                else
                    goingBack = false;
                dataWnd.TimeLabel.Text = t.ToString("F3");
                dataWnd.PathDistanceLabel.Text = pathDistance.ToString("F3");
                dataWnd.NumCollisionsLabel.Text = numCollisions.ToString();
                dataWnd.AvgSpeedLabel.Text = (pathDistance / t).ToString("F3");
                dataWnd.MaxSpeedLabel.Text = (maxSpeed).ToString("F3");
                dataWnd.AvgPassabilityLabel.Text = (sumPassability / numMeasures).ToString("F3");
            }
            //end of measures

            pos = pp.p;
            return base.react(pp, t);
            //выполняется на каждом тике таймера.
            /* Состав PerceptParams:
             * float lidar_cm, lidar_d; - параметры лидара - включите, если хотите, но заставьте его игнорить щуп.
             * float dist, targ_angle; - расстояние и угол до цели.
             * float light, Color color, int features_angle,features_line,int features_gleam - НЕ(!)реализованые параметры изображения.
             * double kaif - как раз наоборот, мера общей ТРУДНОСТИ перемещения сейчас из-за ПРЕПЯТСТВИЙ.
             * double ksurf - мера трудности сдвижения из-за поверхности.
             * Vec2 nose - координаты щупа в Box2D
             * Pose p - координаты робота в Box2D
             * System.Drawing.Image cameraView - Оно самое - изображение из pictureBox2, точнее его КОПИЯ.
             * double velocity - скорость из Box2d
             * 
             * Состав CarControl
             * float u_gas - газ
             * float u_steer - поворот колёс относительно оси "вперёд" машины.
             * float u_brake - тормоза.
             */

        }
        override public void intelligentSystemCalculateWeights(PerceptParams pp, float t)
        {
            
            //ФАЗА СОЗДАНИЯ ПРИМЕРОВ

        }
    }
}
