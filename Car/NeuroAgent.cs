using System;
using System.Collections.Generic;
using System.Text;
using Box2DX.Common;
using System.Drawing;
using System.Drawing.Imaging;
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
    class NeuroAgent : dreamerAgent
    {
        FFNN nn;
        public NeuroAgent(Pose initial, Vec2 target, int xtiles = 80, int ytiles = 50)
            : base(initial, target, xtiles, ytiles)
        {
            /* Pose initial - начальное положение машины в системе Box2d с перевёрнутым углом (0 - слева, + против часовой),
             * Vec2 target - положение цели в системе Box2d,
             * int xtiles = 80, int ytiles = 50 - ширина и высота массива тайлов для поиска пути. Глубина определяется количеством ориентаций(8).
             */
            nn = new FFNN("10000, 25", true);

            this.xtiles = xtiles; this.ytiles = ytiles;
        }

        int xtiles, ytiles;

        /* Содержание dreamerAgent. Работать с ними условимся только в СВОИХ классах, ибо перемены в DreamAgent для 1 наследника могут вывести из строя другой.
         * public Prbl pr  - описание проблемы
         * public DStar ds - система динамического поиска пути.
         * double angle_last = 0, tLast =0 - угол и время на прошлом кадре - для дифференциалов
         * ox = 80.0 / [число тайлов у проблемы в ширину] - расстояние между центрами тайлов по горизонтали, ширина тайла
         * oy = 50.0 / число тайлов у проблемы в высоту - расстояние между центрами тайлов по вертикали, высота тайла
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

        //<SD>

        public override CarControl react(PerceptParams pp, float t)
        {
            //ВАЖНО!!! для чистоты эксперимента необходимо, чтобы ОТРАБОТКА траектории, а значит, и
            //параметры регулятора и механизм вывода CarControl, были одинаковые или выбирались на равных
            //условиях во всех 7(план максимум) сериях экспериментов: контрольная, сети по базе, деревья по базе,
            //сети с ходу, деревья с ходу, комбинация по базе, комбинация с ходу.

            var res=base.react(pp, t);
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
            //Form1.f.label1.Text = "" + pp.kaif;


            float Y = 0.4f;
            if (double.IsPositiveInfinity(pp.kaif)) Y = 1;

                var inps = BitmapToFloats(pp.cameraView as Bitmap, -1);

                var outs = nn.Calculate(inps);

                var douts = new float[outs.Length];
                for (int i = 0; i < douts.Length; i++)
                {
                    douts[i] = Y;
                }

                var klearns = new float[outs.Length];
                for (int ky = 0, i = 0; ky < K; ky++)
                {
                    for (int kx = 0; kx < K; kx++)
                    {
#warning Position of camera may change
                        float miu = get_miu(kx, ky, K);
                        klearns[i++] = miu;
                    }
                }

            var miu1=0.03f;
            if (Y < 1) { klearns = null; miu1 *= 3; }
                nn.Learn(douts, klearns, miu1, null);

            return res;

        }

        float get_miu(int kx, int ky, int K)
        {
            float dx = kx - (K - 1) / 2f, dy = ky - (K - 1);
            float d = (float)System.Math.Sqrt(dx * dx + dy * dy);
            var m= System.Math.Max(0, 1 - d / K);
            return m * m;
        }

        int K = 5;
        override public void intelligentSystemCalculateWeights(PerceptParams pp, float t)
        {
            //вызывается внутри реакта в тот момент, когда можно безопасно поменять веса.
            //именно здесь (по скромному ежиному мнению) проще всего сделать обновление весов силами нейронки.

            var inps = BitmapToFloats(pp.cameraView as Bitmap, -1);

            var outs = nn.Calculate(inps);

            for (int ky = 0; ky < K; ky++)
            {
                for (int kx = 0; kx < K; kx++)
                {
                    var p = NeuroAgent.VisionToWorld(kx, ky, K, K, pp);
                    int x, y;
                    NeuroAgent.WorldToTile(p.X, p.Y, out x, out y, this, pr);

                    var exapmle_cost = outs[kx+ky*K];

                    bool upd = rnd.NextDouble() < get_miu(kx, ky, K);

                    if (exapmle_cost > 0.6 && upd)
                    {
                        //obst(x, y, 1, true);
                    }
                    if (exapmle_cost < 0.6f)
                    {
                      // obst(x, y, 1, false);
                    }

                }
            }

          

            //алгоритм перевода координат Box2d в координаты тайла

#warning TODO - нейросеть: вход - изображение, выход наличие препятствий по всем тайлам в зоне видимости. геом преобразование.
#warning TODO - запоминать те тайлы в которых побывали и по ним обучать нейросеть

            //pr.setCost(xn, yn, exapmle_cost);

            //throw new NotImplementedException("Интеллектуальная система не реализована");
        }

        Random rnd = new Random();

        void obst(int x, int y, int r, bool add_or_remove)
        {
            for (int ky = -r; ky <= r; ky++)
            {
                int y_ = y + ky;

                if (y_ < 0 || y_ >= ytiles) continue;

                for (int kx = -r; kx <= r; kx++)
                {
                    int x_ = x + kx;

                    if (x_ < 0 || x_ >= xtiles) continue;

                    if (add_or_remove)
                    {
                        pr.setCost(x_, y_, float.PositiveInfinity);
#warning вместо infs юзать obsts
                        infs.Add(pr.states[orientation.down][x_, y_]); //???
                    }
                    else
                    {
                        pr.setCost(x, y, 0);

                        for (int i = 0; i < infs.Count; i++)
                        {
                            var z = infs[i];
                            if (z.x == x && z.y == y)
                            {
                                infs.RemoveAt(i);
                                break;
                            }
                        }
                    }


                }
            }
        }

        public static void WorldToTile(float x_, float y_, out int x, out int y, dreamerAgent da, Prbl pr)
        {
            x = Convert.ToInt32(System.Math.Round((x_ - da.xs) / da.ox));
            if (x < 0) x = 0;
            if (x > pr.dx - 1) x = pr.dx - 1;
            y = Convert.ToInt32(System.Math.Round((y_ - da.ys) / da.oy));
            if (y < 0) y = 0;
            if (y > pr.dy - 1) y = pr.dy - 1;
        }

        public static Vec2 VisionToWorld(int kx, int ky, int numx, int numy, PerceptParams pp)
        {
            float w = pp.cameraView.Width, h = pp.cameraView.Height;
            float dw = w / numx;
            float dh = h / numy;

            float x = dw * (kx + 0.5f);
            float y = dh * (ky + 0.5f);

            return VisionToWorld(x, y, w, h, Form1.scale * Form1.gh_static.Scale, Form1.dL, pp);
        }

        public static Vec2 VisionToWorld(float px_x, float px_y, float px_w, float px_h, float scale, float dL_px, PerceptParams pp)
        {
            var p0 = new Vec2(px_w / 2, px_h / 2 + dL_px);
            var p1 = new Vec2(px_x, px_y);

            var dp = p1 - p0;

            var r = dp.Length();
            var a = -Helper.GetRelAngle(0, -1, dp.X, dp.Y, Helper.angle_to_rad * 360) + pp.p.angle_rad;
            a *= -1;

            var r1 = r / scale;

            double x = pp.p.xc;
            double y = pp.p.yc;

            x += r1 * System.Math.Cos(a);
            y += r1 * System.Math.Sin(a);

            return new Vec2((float)x, (float)y);
        }

        public static float[] BitmapToFloats(Bitmap b, int channel)
        {
            int w = b.Width, h = b.Height;
            var res = new float[w * h];
            var imgData = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            /*
            unsafe
            {
                byte* imgPtr0 = (byte*)imgData.Scan0;

                for (int i = 0, y = 0; y < h; y++)
                {
                    var imgPtr1 = imgPtr0 + y * imgData.Stride;
                    for (int x = 0; x < w; x++, i++)
                    {
                        var imgPtr = imgPtr1 + 3 * x;

                        float a;
                        if (channel == -1) a = (imgPtr[0] + imgPtr[1] + imgPtr[2]) / 3f / 255f;
                        else a = imgPtr[channel] / 255f;
                        res[i] = a;
                    }
                }
             
            }*/
            b.UnlockBits(imgData);
            return res;
        }

        //</SD>


        public class Sample
        {

        }
    }

}
