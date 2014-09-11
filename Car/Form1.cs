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



        public Dictionary<string, Agent> agents = new Dictionary<string, Agent>(); // агенты по именам

        public Environment env;//описание среды
        public bool started; //сигнал начала/конца работы

        private void Form1_Load(object sender, EventArgs e)
        {
            initArena();
            
        }
       
        builders.Builder activeBuilder = new builders.Builder();

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (button1.Text == "Старт")
            {
                if (env.robotsDict.Count == 0)
                {
                    MessageBox.Show("add at least 1 robot");
                    return;
                }
                button1.Text = "Стоп";
                t = 0;

                foreach (string agentname in agents.Keys)
                {
                    robotListBox.Items.Add(agentname);
                }


                saveToFile("last");


                //agent init

                //update targets and positions(positions - not to cause first pathGone unit at initial position)
                foreach (var pair in env.targetsDict)
                    (pair.Key as dreamerAgent).target = pair.Value;
                foreach (var pair in env.robotsDict)
                    (pair.Key as dreamerAgent).initial = pair.Value.pos;

                //init group leaders
                Dictionary<string, List<EvoForestUnitAgent>> groupsDict = new Dictionary<string, List<EvoForestUnitAgent>>();
                foreach (Agent agent in agents.Values)
                {
                    if (typeOfAgent(agent) == "LICS forest car agent with transfer")
                    {
                        EvoForestUnitAgent unitAgent = agent as EvoForestUnitAgent;
                        if (unitAgent.groupID == "") continue;
                        if (!groupsDict.ContainsKey(unitAgent.groupID))
                            groupsDict[unitAgent.groupID] = new List<EvoForestUnitAgent>();
                        groupsDict[unitAgent.groupID].Add(unitAgent);
                    }
                }
                foreach (var pair in groupsDict)
                    agents.Add("coordinator " + pair.Key, new EvoForestTransferAgent(pair.Value));

                //initialise data windows
                foreach (var wnd in dataWnds)
                {
                    wnd.InitializeComponent();
                }
                //initialise agents
                foreach (Agent agent in agents.Values)
                    agent.initAgent();//why: init is safe only after all preparations are done
                //agent init end
                

                

                


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

                initArena();

                started = false;
                //deinit

                //end of deinit
                loadFromFile("last");

            }
           
        }
        Pose defaultPose1 = new Pose() { xc = 75, yc = 6, angle_rad = -30 * Helper.angle_to_rad },
             defaultPose2 = new Pose() { xc = 65, yc = 6, angle_rad = -30 * Helper.angle_to_rad };
        
        Vec2 defaultTarget1 = new Vec2(15F, 15f),
            defaultTarget2 = new Vec2(15F, 10f);
        public Car createCar(Pose pose)
        {
            var pars = new CarParams();
            pars.InitDefault();
            pars.h = 2.5f;
            pars.h_base = 2f;
            return createCar(pose, pars);
        }
        public Car createCar(Pose pose, CarParams pars)
        {
            // рожаем машину
            Car car = new Car(pars, 0);

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
        public void initArena()
        {

            //инциализируем отрисовку и физику

            gh = new GraphicsHelper(pictureBox1);
            ghover = new GraphicsHelper(pictureBoxOverlay);
            ph = new Physics();
            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            env = new Environment(ph, this);//
            
            env.draw(gh);
            pictureBoxOverlay.Image = (Image)pictureBox1.Image.Clone();
            foreach (LicsDataWindow dataWnd in dataWnds) dataWnd.Close();
            dataWnds = new List<LicsDataWindow>();
            while (robotListBox.Items.Count != 0)
                robotListBox.Items.RemoveAt(0);
            agents = new Dictionary<string, Agent>();//dispose previous agents
            



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

            
            foreach(Agent agent in agents.Values)
            {
                var p=env.percept(agent);
                env.act(agent.react(p, t),agent); // вот он цикл работы агента
            }

            string observedName = robotListBox.SelectedItem as string;
            if (observedName == null)
                observedName = robotListBox.Items[0] as string;
            Agent carAgent = agents[observedName];
            var pp = env.percept(carAgent);

            env.step(dt);

            
            // рисуем
            env.draw(gh);
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
            var car = env.robotsDict[carAgent];

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
            if (name == "")
                return false;
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
            
            int digitsCount = agents.Count.ToString().Length;
            int id = 0;
            foreach(var keyValue in agents)
            {
                var agent = keyValue.Value;
                var ids = id.ToString();
                while (ids.Length < digitsCount) ids = "0" + ids;
                writer = System.IO.File.CreateText(@"saves\" + name + @"\robot"+ids+".cfg");


                Pose pos = new Pose { };
                Vec2 target = new Vec2();
                CarParams cparams = new CarParams();
                if (env.robotsDict.ContainsKey(agent))
                {
                    cparams = env.robotsDict[agent].p;
                    pos = env.robotsDict[agent].pos;
                    target = env.targetsDict[agent];
                }

                string groupID = "";
                if (typeOfAgent(agent) == "LICS forest car agent with transfer")
                    groupID = (agent as EvoForestUnitAgent).groupID;
                roboser.Serialize(writer, new carCfg { p = pos, target = target, agentName = keyValue.Key, agentType =typeOfAgent(agent), agentGroup =groupID, cpars = cparams });

                writer.Close();
                id++;
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
                env.robotsDict = new  Dictionary<Agent,Car>();
                env.targetsDict = new Dictionary<Agent, Vec2>();
                
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
                    Car car = createCar(cfg.p, cfg.cpars);
                    
                        
                    var agent = agentByType(cfg.agentType, car,cfg.target);
                    if (typeOfAgent(agent) == "LICS forest car agent with transfer")
                        (agent as EvoForestUnitAgent).groupID = cfg.agentGroup;

                    env.addRobot(agent,car,cfg.target);
                    agents[cfg.agentName] = agent;

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
                //draw
                pictureBox1.Invalidate();
                env.draw(gh);
                pictureBoxOverlay.Image = (Image)pictureBox1.Image.Clone(); 
                return true;
            }
            catch (Exception e)
            {
                
                return false;
            }
        }
        //version-specific
        public List<LicsDataWindow> dataWnds = new List<LicsDataWindow>();
        //end of version-specific
        public Agent addNewAgent(string name, string type, string groupID = "")
        {
            if (name == null)
                return null;
            if (agents.ContainsKey(name))
                return null;

            Agent agent;

            if (type != "LICS forest coordinator")
            {

                Car car = Form1.createCar(new Pose { xc = 10, yc = 10, angle_rad = 0 }, env.physics);
                Vec2 target = new Vec2(15, 10);
                agent = agentByType(type, car, target,groupID);
                env.addRobot(agent, car, target);

            }
            else
                agent = agentByType(type);

            agents[name] = agent;

            return agent;
        }
        public Agent agentByType(string type,Car robot = null, Vec2 targ = new Vec2(),string group = "")
        {//there must have been a better way to do this
            LicsDataWindow wnd;//for simplicity of naming wnd variables
            switch (type)
            {
                case "reflex agent":
                    return new reflexAgent();
                case "dreamer agent":
                    return new dreamerAgent(robot.pos,targ);
                case "HTM agent":
                    return new NeuroAgent(robot.pos,targ);
                case "LICS tree agent":
                    wnd = new LicsDataWindow();
                    dataWnds.Add(wnd);
                    return new LICSTreeAgent(wnd, robot.pos, targ);
                case "planning agent":
                    wnd = new LicsDataWindow();
                    dataWnds.Add(wnd);
                    return new NotLearningAgent( wnd,robot.pos,targ);
                
                case "LICS forest car agent":
                    wnd = new LicsDataWindow();
                    dataWnds.Add(wnd);
                    return new LICSForestAgent(wnd,robot.pos,targ);
                
                case "LICS forest car agent with transfer":
                    wnd = new LicsDataWindow();
                    dataWnds.Add(wnd);
                    return new EvoForestUnitAgent(wnd,robot.pos,targ,group);
                    
                case "LICS forest coordinator":
                    return new EvoForestTransferAgent(new List<EvoForestUnitAgent>());
            }
                    return null;
        }
        public string typeOfAgent(Agent agent)
        {//there must have been a better way to do this
            Type type =  agent.GetType();
            
            switch (type.Name)
            {
                case "reflexAgent": return "reflex agent";
                case "dreamerAgent": return "dreamer agent";
                case "NeuroAgent": return "HTM agent";
                case "LICSTreeAgent": return "LICS tree agent";
                case "EvoForestUnitAgent": return "LICS forest car agent with transfer";
                case "NotLearningAgent": return "planning agent";
                case "LICSForestAgent": return "LICS forest car agent";
                case "EvoForestTransferAgent": return "LICS forest coordinator";
            }
            return null;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Agent carAgent in agents.Values)
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
        Agent observedAgent;
        }
    




        

    }

