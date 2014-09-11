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
    
    class LICSForestAgent : dreamerAgent
    {
        public LicsDataWindow dataWnd;
        public dynamic getPythonWrapper(string pyFile, Hashtable parameters)
        {//creates a wrapper using a specified python file
            if (!File.Exists(pyFile))
                throw new Exception("No such file");
            
            var ipy = Python.CreateRuntime();
                        
            dynamic wrap = ipy.UseFile(pyFile);
            return wrap;
        }
        public Vec2 rotate(Vec2 vec, double angle_rad)
        {
            return new Vec2((float)System.Math.Cos(angle_rad) * vec.X - (float)System.Math.Sin(angle_rad) * vec.Y, (float)System.Math.Cos(angle_rad) * vec.Y + (float)System.Math.Sin(angle_rad)*vec.X);
        }
        public Vec2 cameraToBox2d(Vec2 vec)
        {
            Vec2 v = new Vec2(vec.X, vec.Y);
            v -= new Vec2(50, 100); //в систему координат робота
            v *= 0.2f; //в масштаб оригинала
            
            v = rotate(v, +1.57-pos.angle_rad);//доворот угла до угла глобальной системы координат( с ординатой вверх).
            v += new Vec2(pos.xc, pos.yc); //сдвиг в систему координат Box2d

            return v;
        }
        public Vec2 box2dToCamera(Vec2 vec)
        {
            Vec2 v = new Vec2(vec.X, vec.Y);
            v -= new Vec2(pos.xc, pos.yc); //сдвиг в систему координат робота
            v = rotate(v, pos.angle_rad-1.57);//доворот угла до угла робота.
            v *= 5f; //в масштаб оригинала
            v += new Vec2(50, 100); //в систему координат картинки
            return v;
        }

        public Image local = new Bitmap(1, 1);
        public Bitmap getTileImage(Bitmap source, int Tx, int Ty)
        {
            Vec2 tileInGlobal = getTileCoords(Tx, Ty);
            Vec2 pp_dflt = box2dToCamera(tileInGlobal);
            local =GraphicsHelper.GetLocalViewImage(source, Convert.ToInt32(wrapper.width), Convert.ToInt32(wrapper.height), pp_dflt , pos.angle_rad+1.57f, 0f, 1f);
            return (Bitmap)local;
        }
        Thread pythoner;
        bool pythonerLaunched = false;

        public virtual void threadMethod()
        {
            //be warn, young padawan! everything you write here might get executed long after threadstart and is not determined in time,
            //so something might go wrong if you use some data used elsewhere in the program.
            system._injectedEncoder.updateSystem(system);
            var tsamples = newSamples;
            newSamples = new IronPython.Runtime.SetCollection(); //this is done so that new samples cannot affect the current loop (and cause it to crash, most likely)
            system.fullLoop(tsamples);
                
        }
        public void launchComposeThread()
        {
            
            ThreadStart ts = new ThreadStart(threadMethod);
                                              
            pythoner = new Thread(ts);
            pythoner.Start();
            pythonerLaunched = true;

        }
        public LICSForestAgent(LicsDataWindow dataW,Pose initial, Vec2 target)
            : base(initial, target)
        {
            /* Pose initial - начальное положение машины в системе Box2d с перевёрнутым углом (0 - слева, + против часовой),
             * Vec2 target - положение цели в системе Box2d,
             * int xtiles = 80, int ytiles = 50 - ширина и высота массива тайлов для поиска пути. Глубина определяется количеством ориентаций(8).
             */

            //initialising data window
            dataWnd = dataW;

          

        }
        public override void initAgent()
        {
            //initializing LICS FROM A WRAPPER
            base.initAgent();
            wrapper = getPythonWrapper("..\\..\\..\\PyLICS\\wrapper.py", new Hashtable());

            system = wrapper.getSystem();
            enc = system._injectedEncoder;
            launchComposeThread();
            pos = initial;

            // initialisation end
            dataWnd.Show();

        }
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
            //07072014 Ёж: ну как, три пункта мы выполнили :P

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
        public coordpair getTileCoords( Vec2 v)
        {
            int x, y;
            x = Convert.ToInt32(System.Math.Round((v.X - xs) / ox));
            if (x < 0) x = 0;
            if (x > pr.dx - 1) x = pr.dx - 1;
            y = Convert.ToInt32(System.Math.Round((v.Y - ys) / oy));
            if (y < 0) y = 0;
            if (y > pr.dy - 1) y = pr.dy - 1;
            return new coordpair { x = x, y = y };
        }
        public struct coordpair { public int x, y;};
        public Vec2 getTileCoords( int x, int y)
        {
            return new Vec2((float)(x * ox + xs), (float)(y * oy + ys));
        }
        bool visible(coordpair pair)
        {
            Vec2 vec = box2dToCamera( getTileCoords(pair.x, pair.y));
            return (vec.X > 2*ox *5) && (vec.X < 100 - 2*ox *5) && (vec.Y > 2*oy *5) && (vec.Y < 100 - 2*oy *5) ;
        }
        List<coordpair> getActualTiles()
        {//visible or within clasification range
            var tiles= getVisibleTiles();
            /*
#warning causes minor freezes
            //mb just take random indices at photos.keys collection?
            for ( int indX = x-classificationRange; indX <= x+classificationRange; indX++)
                for (int indY = y- classificationRange; indY <= y + classificationRange; indY++)
                {
                    coordpair tile = new coordpair{x=indX,y=indY};
                    if (visible(tile))continue;
                    if (distanceFromRobot(tile) < classificationRange)
                        if (photos.ContainsKey(tile))//not checking consistency of coords 'cuz there can't be photos of inconsistent ones
                          tiles.Add(tile);
                }
            */
            return tiles;

            

        }
        List<coordpair> getVisibleTiles()
        {
            //maybe optimize?
            List<coordpair> tiles = new List<coordpair>();
            coordpair farLeft= getTileCoords(cameraToBox2d(new Vec2((float)ox / 2, (float)oy / 2))),
                      farRight= getTileCoords(cameraToBox2d(new Vec2(100f - (float)ox / 2, (float)oy / 2))),
                      nearRight= getTileCoords(cameraToBox2d(new Vec2(100f - (float)ox / 2, 100-(float)oy / 2))),
                      nearLeft = getTileCoords(cameraToBox2d(new Vec2((float)ox / 2, 100f - (float)oy / 2)));
            //наркоманский способ это сделать - идти, например, слева направо по столбцам: найти вершину столбца, после чего опускаться, пока не вышел из камеры, после чего - следующий столбик.
            int leftMost = System.Math.Min(System.Math.Min(farLeft.x, farRight.x), System.Math.Min(nearLeft.x, nearRight.x)),
                rightMost = System.Math.Max(System.Math.Max(farLeft.x, farRight.x), System.Math.Max(nearLeft.x, nearRight.x)),
                downMost = System.Math.Max(System.Math.Max(farLeft.y, farRight.y), System.Math.Max(nearLeft.y, nearRight.y)),
                upMost = System.Math.Min(System.Math.Min(farLeft.y, farRight.y), System.Math.Min(nearLeft.y, nearRight.y));

            for ( int i = leftMost; i<= rightMost; i++)
            {
                for (int j = upMost; j <= downMost; j++)
                {
                    coordpair pair = new coordpair{x = i, y = j};
                    if (visible(pair))
                    {
                        tiles.Add(pair);
                        if (!photos.ContainsKey(pair))
                            photos[pair] =getTileImage(new Bitmap(pplast.cameraView), pair.x, pair.y);
                    }
                }
            }

            return tiles;
        }
        float tlast;
        float dt = 0.5f;
        //pythonies
        public dynamic wrapper ;
        public dynamic enc;
        public dynamic system;
        //end of pythonies
        public PerceptParams pplast = new PerceptParams{ };
        List<coordpair> tiles= new List<coordpair>();
        
        Dictionary<coordpair, Bitmap> photos = new Dictionary<coordpair, Bitmap>();


        //Ёжик: механизм обработки семплов офигительнейше избыточен в памяти,
        Dictionary<coordpair,dynamic> samplesDict = new Dictionary<coordpair,dynamic>();//all samples
        protected IronPython.Runtime.SetCollection newSamples = new IronPython.Runtime.SetCollection();// new portion for each update


        Dictionary<coordpair, System.Windows.Forms.ListViewItem> images = new Dictionary<coordpair, System.Windows.Forms.ListViewItem>();
        
        void feedSample(Bitmap tile,coordpair coords, double measuredcost)
        {
            measuredcost = System.Math.Round(measuredcost, 1);
            dynamic sample = wrapper.sampleFromBitmap(tile);
            enc.encode(sample,measuredcost);

            if (samplesDict.ContainsKey(coords))
            {
                if (measuredcost == enc.decodeSample(samplesDict[coords]))
                    return;


                samplesDict[coords] = sample;

                newSamples.add(sample);
                images[coords].Name = measuredcost.ToString();

            }
            else
            {
                dataWnd.ListSamples.LargeImageList.Images.Add(tile);
                System.Windows.Forms.ListViewItem listViewItem = new System.Windows.Forms.ListViewItem(measuredcost.ToString(),dataWnd.ListSamples.LargeImageList.Images.Count-1);

                samplesDict[coords] = sample;

                newSamples.add(sample);
                images[coords] = listViewItem;
                dataWnd.ListSamples.Items.AddRange(new System.Windows.Forms.ListViewItem[] { listViewItem });
            }
        }
        double distanceFromRobot(coordpair tile)
        {
            return System.Math.Sqrt((tile.x - x) * (tile.x - x) + (tile.y - y) * (tile.y - y));
        }
        public Dictionary<state, double> measureHistory = new Dictionary<state, double>();//список рутовых измерений(за счёт kaif и ksurf) за всё время. сами Measures обнуляются.
        public int classificationRange = 7;
        public int classificationsPerCall = 150;//number of random classifications within field of view per call of IntelligentSystemCalculateWeights.
        public int blindZone = 3;
        override public void intelligentSystemCalculateWeights(PerceptParams pp, float t)
        {
            
            //ФАЗА СОЗДАНИЯ ПРИМЕРОВ
            foreach ( KeyValuePair<state,double> measure in measures)
            {
                coordpair tile = new coordpair{ x = measure.Key.x, y = measure.Key.y};
                measureHistory[measure.Key] = measure.Value;
                if (photos.ContainsKey(tile))
                {
                    feedSample(photos[tile],tile, measure.Value);
                }
            }
            measures = new Dictionary<state, double>();//да-да, они обнуляются
            if (pythonerLaunched)//flag for the first single round
            {
                if (!pythoner.IsAlive)
                {
                    dataWnd.treeVisualisationLabel.Text = system.logWriter.readString();
                    dataWnd.encbox.Text = enc.visualise();
                    launchComposeThread();
                }
            }
            //конец фазы создания примеров
            
            //именно здесь происходит обновление весов силами ССИУ
            if (t - tlast > dt)
                tlast = t;
            else return;
            pplast = pp;
            Random picker = new Random();


            for (int itr = 0; itr < classificationsPerCall; itr++)
            {
                if (tiles.Count == 0)
                {
                    tiles = getActualTiles();
                    continue;//in case there are none
                }

                int ind = picker.Next(tiles.Count);
                coordpair tile = tiles[ind];


                tiles.RemoveAt(ind);

                double dist = distanceFromRobot(tile);
                //don't classify too close,    too far                  , or directly measured
                if ((dist < blindZone) || (dist > classificationRange && !visible(tile)) || measureHistory.ContainsKey(pr.states[orientation.down][tile.x, tile.y])) continue;

                {
                    Bitmap bmp = photos[tile];

                    dynamic sample = wrapper.sampleFromBitmap(bmp);
                    try
                    {//why try: there was chance that some trees got polled while being composed or pruned. If you wanna adjust the learning routine so as it won't cause this error, you may try.
                        dynamic rslt = system.classify(sample, 1, 10);
#warning adjust
                        double cost = enc.decodeResult(rslt);

                        pr.setCost(tile.x, tile.y, cost);
                        if (cost == double.PositiveInfinity)
                            obsts[pr.states[orientation.down][tile.x, tile.y]] = System.Drawing.Color.Crimson;
                        else if (cost > 2)
                            obsts[pr.states[orientation.down][tile.x, tile.y]] = System.Drawing.Color.DarkGray;
                        else
                        {
                            if (obsts.ContainsKey(pr.states[orientation.down][tile.x, tile.y]))
                                obsts.Remove(pr.states[orientation.down][tile.x, tile.y]);
                            if (infs.Contains(pr.states[orientation.down][tile.x, tile.y]))
                                infs.Remove(pr.states[orientation.down][tile.x, tile.y]);

                        }
                    }
                    catch (Exception e) { }
                    //{ System.Windows.Forms.MessageBox.Show(e.Message); };


                }
            }

        }
    }
    
}
