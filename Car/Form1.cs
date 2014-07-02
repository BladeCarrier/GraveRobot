using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
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
        GraphicsHelper gh,ghover; //графический помощник
        Physics ph;//физический помощник
        
        
        List<Car> cars = new List<Car>(); //машина (и физика и графика). АКТУАЛЬНЫЙ список хранится в environment, возможна рассинхронизация

        List<Agent> agents = new List<Agent>();// агент (управление) машины

        public Environment env;//описание среды
        public bool started; //сигнал начала/конца работы

        private void Form1_Load(object sender, EventArgs e)
        {
            initArena();


        }
       
        builders.Builder activeBuilder = new builders.Builder();

        public List<LicsDataWindow> dataWnds;
        private void button1_Click(object sender, EventArgs e)
        {
            
            if (button1.Text == "Старт")
            {
                if (env.robots.Count == 0)
                {
                    MessageBox.Show("add at least 1 robot");
                    return;
                }
                button1.Text = "Стоп";
                //создание агента и среды
                
                dataWnds = new List<LicsDataWindow>();

                agents = new List<Agent>();//dispose previous agents
                if (SwitchLearning.Checked) 
                {
                    for (int i = 0; i < env.robots.Count; i++)
                    {
                        var dataWnd = new LicsDataWindow();
                        dataWnds.Add(dataWnd);
                        dataWnd.InitializeComponent();

                        dataWnd.Show();

                        agents.Add(new LICStreeAgent(dataWnd, env.robots[i].pos, env.targets[i]));
                    }
                }
                else
                {
                    for(int i=0; i<env.robots.Count;i++)
                        agents.Add(new dreamerAgent(env.robots[i].pos, env.targets[i]));
                }

                saveToFile("last");

                /*
                //создание объектов среды
                //ВАЖНО!!! -- в каком порядке объекты создаются, в таком они и отрисовываются, поэтому в первую очередь создаются объекты заднего плана.
                //занятно...-- НЕсамообучающийся по картинке агент ооочень туго переваривает динамические препятствия. Хотя, всё же, переваривает.

                env.CreateRectZone(new Pose { xc = 32f, yc = 25f, angle_rad = 45 * k_PI }, new Vec2(15f, 25f), 10f);
            
                env.CreateWall(new Pose { xc = 25f, yc = 17f, angle_rad = 20 * k_PI }, new Vec2(2f, 10f));
              
                env.CreateColumn(new Pose { xc = 55f, yc = 35f, angle_rad = 100 * k_PI }, 5);
                env.CreateCircle(new Pose { xc = 25f, yc = 20f, angle_rad = -10 * k_PI }, 3, 10);
                */
                env.CreateWall(new Pose { xc = pictureBox1.Width / 60f, yc = pictureBox1.Height / 10f, angle_rad = 90 * k_PI }, new Vec2(2f, 10000f), new Box2DX.Dynamics.Color(0, 0, 0));
                env.CreateWall(new Pose { xc = pictureBox1.Width / 60f, yc = 0 / 10f, angle_rad = 90 * k_PI }, new Vec2(1f, 10000f), new Box2DX.Dynamics.Color(0, 0, 0));
                env.CreateWall(new Pose { xc = pictureBox1.Width / 10f, yc = pictureBox1.Height / 60f, angle_rad = 0 * k_PI }, new Vec2(2f, 10000f), new Box2DX.Dynamics.Color(0, 0, 0));
                env.CreateWall(new Pose { xc = 0 / 10f, yc = pictureBox1.Height / 60f, angle_rad = 0 * k_PI }, new Vec2(1f, 10000f), new Box2DX.Dynamics.Color(0, 0, 0));
                
                started = true;
                activeBuilder.Close();

                f = this;
                gh_static = gh;

            }
            else
            {
                button1.Text = "Старт";
                //инциализируем отрисовку и физику

                initArena(true);

                started = false;
                foreach(LicsDataWindow dataWnd in dataWnds) dataWnd.Close();
            }
           
        }
        Pose defaultPose1 = new Pose() { xc = 75, yc = 6, angle_rad = -30 * Helper.angle_to_rad },
             defaultPose2 = new Pose() { xc = 65, yc = 6, angle_rad = -30 * Helper.angle_to_rad };
        
        Vec2 defaultTarget1 = new Vec2(15F, 15f),
            defaultTarget2 = new Vec2(15F, 10f);
        public Car createCar(Pose pose)
        {
            // рожаем машину
            Car car;
            var p = new CarParams();
            p.InitDefault();
            p.h = 2.5f;
            p.h_base = 2f;
            car = new Car(p, 0);

            car.Create(ph, pose);
            return car;

        }
        static public Car createCar(Pose pose, Physics ph)
        {
            // рожаем машину
            Car car;
            var p = new CarParams();
            p.InitDefault();
            p.h = 2.5f;
            p.h_base = 2f;
            car = new Car(p, 0);

            car.Create(ph, pose);
            return car;
        }
        public void initArena(bool noCars = false)
        {

            //инциализируем отрисовку и физику

            gh = new GraphicsHelper(pictureBox1);
            ghover = new GraphicsHelper(pictureBoxOverlay);
            ph = new Physics();
            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            cars = new List<Car>();
            List<Vec2> targets = new List<Vec2>();
            if (!noCars)
            {
                Car car1= createCar(defaultPose1);
                cars.Add(car1);
                targets.Add(defaultTarget1);
            }
            env = new Environment(ph,cars, targets, this);//
            env.draw(gh);
            pictureBoxOverlay.Image = (Image)pictureBox1.Image.Clone();
        }
        private void pictureBoxOverlay_MouseClick(object sender, MouseEventArgs e)
        {
            if (started) return;

            activeBuilder.reportClick(e.X, e.Y);
            env.draw(gh);
            pictureBoxOverlay.Image = (Image)pictureBox1.Image.Clone();
            
        }
        private void pictureBoxOverlay_MouseMove(object sender, MouseEventArgs e)
        {
            if (started) return;
            activeBuilder.reportMove(e.X, e.Y);
            env.draw(gh);
            pictureBoxOverlay.Image = (Image)pictureBox1.Image.Clone();            
        }

        public Image getLocalViewImage(Car car)
        {
            return GraphicsHelper.GetLocalViewImage((Bitmap)pictureBox1.Image/*картинка с общего вида*/,
                                         pictureBox2.Width, pictureBox2.Height, car.body.GetPosition() * gh.Scale, car.body.GetAngle(), dL, scale);//запилить в один бинарный файл картинки
        }
        float t = 0;
        // несколько раз в секунду рисуем новый кадр анимации
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            if (!started) return;

            float dt = timer1.Interval / 1000f;
            t += dt;

            
            for(int i =0;i<agents.Count;i++)
            {
                var p=env.percept(i);
                var agent = agents[i];
                env.act(agent.react(p, t),i); // вот он цикл работы агента
            }
#warning temp 0th
            var pp = env.percept(0);

            env.step(dt);

            
            // рисуем
            env.draw(gh);
            Agent carAgent = agents[0];
            /*отображение геометрии и узлов поиска пути*/
            double ox = 80.0 / ((dreamerAgent)carAgent).pr.dx;
            double oy = 50.0 / ((dreamerAgent)carAgent).pr.dy;
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

            var car = env.robots[0];
            /*СОХРАНЕНИЕ ЛОГ КАРТИНОК*/

            pictureBox2.Image = getLocalViewImage(car);
            
            pictureBox2.Image.Save("img/" + String.Format("{0:000.000}", System.Math.Round(t, 3)) + ".bmp");//получаем картинку относительно робота
            cnt++;
            if (carAgent is NeuroAgent)
            {
                //<SD>
                int K = 5;
                for (int ky = 0; ky < K; ky++)
                {
                    for (int kx = 0; kx < K; kx++)
                    {
                        var p = NeuroAgent.VisionToWorld(kx, ky, K, K, pp);
                        int x, y;
                        var da = ((dreamerAgent)carAgent);
                        NeuroAgent.WorldToTile(p.X, p.Y, out x, out y, da, da.pr);

                        ghover.FillRectangle((float)(xs + ox * x), (float)(ys + oy * y), 0.3F, 0.3F, 0, System.Drawing.Brushes.Black, true);
                        //ghover.FillRectangle(p.X, p.Y, 0.3F, 0.3F, 0, System.Drawing.Brushes.LightGray, true);
                    }
                }
                //</SD>

            }
        }

        public const float dL = 50;
        public const float scale = 0.5f;
        public static GraphicsHelper gh_static;
        public static Form1 f;

        int cnt = 0;

        public bool saveToFile(string name, bool reWrite = true)
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(@"saves\" + name);

            
            if (dir.Exists)
            {
                if (reWrite)
                    try { dir.Delete(true); }
                    catch (Exception e) { return false; }
                else
                    return false;
            }
            dir.Create();

            
            System.Xml.Serialization.XmlSerializer roboser = new System.Xml.Serialization.XmlSerializer(typeof(carCfg));
            System.IO.StreamWriter writer;
            int digitsCount = env.robots.Count.ToString().Length;
            for (int id = 0; id < env.robots.Count; id++)
            {
                var ids =  id.ToString();
                while (ids.Length < digitsCount) ids = "0" + ids;
                writer = System.IO.File.CreateText(@"saves\" + name + @"\robot"+ids+".cfg");
                
                roboser.Serialize(writer, new carCfg { p = env.robots[id].pos, target = env.targets[id] });
                writer.Close();
            }



            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(objInfo));
            int i = 0;
            digitsCount = env.objInfos.Count.ToString().Length;
            foreach (objInfo obj in env.objInfos)
            {
                var ids = i.ToString();
                while (ids.Length < digitsCount) ids = "0" + ids;
                i++;
                writer = System.IO.File.CreateText(@"saves\" + name + @"\object" + ids + ".inf");
                ser.Serialize(writer, obj);
                writer.Close();

            }
            return true;

        }
        public bool loadFromFile(string name,bool forceSort = false)
        {
            try
            {

                System.Xml.Serialization.XmlSerializer roboser = new System.Xml.Serialization.XmlSerializer(typeof(carCfg));
                var catalog = new System.IO.DirectoryInfo(@"saves\" + name);
                System.IO.FileStream reader;
                env.robots = new List<Car>();
                env.targets = new List<Vec2>();
                
                List<FileInfo> fileList = new List<FileInfo>(catalog.GetFiles("robot*.cfg"));
                if (forceSort)
                {//forceSort is for old or custom saves <robot/object IDs spelled without '0's in front of em>
                    SortedList<int, FileInfo> fileSL = new SortedList<int, FileInfo>();
                    foreach (FileInfo f in fileList)
                    {
                        int id = -1;
                        var s = f.Name.Substring(5, f.Name.IndexOf('.') - 5);
                        if (s.Length != 0) id = int.Parse(s);
                        fileSL[id] = f;
                    }
                    fileList = new List<FileInfo>(fileSL.Values);
                }

                foreach (System.IO.FileInfo file in fileList)
                {
                    reader = file.OpenRead();
                    carCfg cfg = (carCfg)(roboser.Deserialize(reader));
                    reader.Close();
                    env.addRobot(createCar(cfg.p),cfg.target);

                }



                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(@"saves\" + name);
                System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(objInfo));

                fileList = new List<FileInfo>(dir.EnumerateFiles("object*.inf"));
                if (forceSort)
                {//forceSort is for old or custom saves <robot/object IDs spelled without '0's in front of em>
                    SortedList<int, FileInfo> fileSL = new SortedList<int, FileInfo>();
                    foreach (FileInfo f in fileList)
                    {
                        int id = -1;
                        var s = f.Name.Substring(5, f.Name.IndexOf('.') - 5);
                        if (s.Length != 0) id = int.Parse(s);
                        fileSL[id] = f;
                    }
                    fileList = new List<FileInfo>(fileSL.Values);
                }

                foreach (System.IO.FileInfo f in fileList)
                {
                    reader = f.OpenRead();
                    objInfo oi = (objInfo)ser.Deserialize(reader);
                    env.CreateFromInfo(oi);
                    reader.Close();
                    
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Agent carAgent in agents)
            {
                var x = carAgent as reflexAgent;
                if (x != null) x.ClearResources();
            }
        }

        //Создание колонны
        private void ColBuilderRadio_Click(object sender, EventArgs e)
        {
            activeBuilder.Close();
            activeBuilder = new builders.ColumnBuilder(env);
            activeBuilder.Show();

        }

        //Создание квадратной области
        private void ObstBuilderRadio_MouseClick(object sender, MouseEventArgs e)
        {
            activeBuilder.Close();
            activeBuilder = new builders.ObstacleBuilder(env);
            activeBuilder.Show();

        }

        
        private void rposeRadio_Click(object sender, EventArgs e)
        {

            activeBuilder.Close();
            activeBuilder = new builders.RobotPoser(this);
            activeBuilder.Show();            
        }

        private void saveloadRadio_Click(object sender, EventArgs e)
        {
            activeBuilder.Close();
            activeBuilder = new builders.SaveLoadBuilder(this);
            activeBuilder.Show();   
        }

        private void RemRadio_Click(object sender, EventArgs e)
        {
            activeBuilder.Close();
            activeBuilder = new builders.RemoverBuilder(env);
            activeBuilder.Show();  
        }




        

    }
}
