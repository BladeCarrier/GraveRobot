using System;
using System.Collections.Generic;
using System.Text;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;


namespace Car
{
    //структура, описывающая физические параметры тела
    public struct BodyBehaviour
    {
        public bool isDynamic;
        public bool isPassable;
        public float k;
    }

    //параметры тела как объекта среды. На их основе формируется перцепт.
    public struct InnerParams
    {//внутренние параметры объекта
        public float glow;
        public float heat;
        public Color color;
        public int features_angle;
        public int features_line;
        public int features_gleam;
    }
    //структура, содеражщая воспринимаемые роботом параметры
    public struct PerceptParams
    {
        public float lidar_cm;
        public float lidar_d;
        public float dist;
        public float targ_angle;
        public float light;
        public Color color;
        public int features_angle;
        public int features_line;
        public int features_gleam;
        public double kaif, ksurf;
        public Vec2 nose;
        public Pose p;
        public double velocity;
        public System.Drawing.Image cameraView;
    }

    public class envObject
    {//реализация ЛЮБОГО объекта окружающей среды.
        /* обоснуй = интерфейс для взаимодействия агента и среды*/
        public BodyBehaviour beh;
        public InnerParams i;
        public Body body;

        public envObject() { }
        public envObject(Body body_, BodyBehaviour bb_, InnerParams i_)
        {
            beh = bb_;
            i = i_;
            body = body_;
        }

        public virtual void draw(GraphicsHelper gh) //Для каждого наследуемого класса для envObject будет соответственно отрисовывать
        {
        }
    }

    //прямоугольный объект-препятствие
    public class envObject_rect: envObject
    {
        public Vec2 size;
        
        public envObject_rect(Body body, BodyBehaviour b_, InnerParams i_, Vec2 size_)
            : base(body, b_, i_)
        {
            size = size_;
        }
        public override void draw(GraphicsHelper gh)
        {

            Vec2 p = body.GetPosition();
            float a = body.GetAngle();
            gh.FillRectangle(
            p.X, p.Y,
            size.X, size.Y,
            a / ((float)System.Math.PI / 180),
            new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb((int)i.color.R,(int)i.color.G,(int)i.color.B)),
            true);
        }
    }

    //прямоугольная область-место
    public class envObject_rectArea : envObject_rect
    {
        public Area area;
        public envObject_rectArea(Area area_, BodyBehaviour b_, InnerParams i_, Vec2 size_)
            : base(null, b_, i_, size_)
        {
            area = area_ ;
        }
        public override void draw(GraphicsHelper gh)
        {

            Vec2 p = new Vec2( area.p.xc,area.p.yc);
            float a = area.p.angle_rad;
            gh.FillRectangle(
            p.X, p.Y,
            size.X, size.Y,
            a / ((float)System.Math.PI / 180),
            new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb((int)i.color.R, (int)i.color.G, (int)i.color.B)),
            true);
            
            
        }
    }

    //круглый объект-препятствие
    public class envObject_crcl : envObject
    {
        public float r;
        public envObject_crcl(Body body, BodyBehaviour b_, InnerParams i_, float r_)
            : base(body, b_, i_)
        {
            r = r_;
        }
        public override void draw(GraphicsHelper gh)
        {

            Vec2 p = body.GetPosition();
            float a = body.GetAngle();
            gh.G.FillEllipse(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb((int)i.color.R, (int)i.color.G, (int)i.color.B)),
            p.X - r, p.Y - r,
            2 * r, 2 * r);
        }
    }

    //круглая область-место
    public class envObject_crclArea : envObject_crcl
    {
        public Area area;
        public envObject_crclArea(Area area_, BodyBehaviour b_, InnerParams i_, float r_)
            : base(null, b_, i_, r_)
        {
            area = area_ ;
        }
        public override void draw(GraphicsHelper gh)
        {

            Vec2 p = new Vec2( area.p.xc,area.p.yc);
            float a = area.p.angle_rad;
            gh.G.FillEllipse(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb((int)i.color.R, (int)i.color.G, (int)i.color.B)),
            p.X - r, p.Y - r,
            2 * r, 2 * r);
        }
    }

    //источник огня.
    public class envObject_fire : envObject_crcl
    {
        float rar;
        public envObject_fire(Body body, BodyBehaviour b_, InnerParams i_, float r_) : base(body, b_, i_, r_) { rar = r_; }
        public override void draw(GraphicsHelper gh)
        {
            Vec2 p = body.GetPosition();
            float rr = 2 * rar*i.heat;
            float ro = 3 * rar * i.heat;

            gh.G.FillEllipse(new System.Drawing.SolidBrush(System.Drawing.Color.Orange),
            p.X - ro, p.Y - ro,
            2 * ro, 2 * ro);
            gh.G.FillEllipse(new System.Drawing.SolidBrush(System.Drawing.Color.OrangeRed),
            p.X - rr, p.Y - rr,
            2 * rr, 2*rr);
            base.draw(gh);
        }
    }

    //сериализаторы
    public struct objInfo
    {
        public string type;
        public Pose p;
        public Vec2 size;
        public float radius, weight;
        public Box2DX.Dynamics.Color color;


    }
    public struct carCfg
    {
        public Pose p;
        public Vec2 target;
        public CarParams cpars;

    }
    //сама среда
    public class Environment
    {
        public Physics physics;
        public List<Car> robots;
        public Environment() { }
        class reporter : ContactListener
        {
            public List<Body> guarded = new List<Body>();
            public Dictionary<Body, envObject> allObjs = new Dictionary<Body, envObject>(); //Словарь соответствия Body и envObject для всех объектов
            
            public Dictionary<Body, List<envObject>> collided = new Dictionary<Body, List<envObject>>(); //Словарь соответствия Body и envObject с которыми произошло столкновение
            
            public reporter( List<Body> bodies, List<envObject> allObjects) //Создает библиотеку препятствий
            {
                guarded = bodies;
                foreach (Body overseen in bodies)
                {
                    collided[overseen] = new List<envObject>();
                }
                foreach (envObject obj in allObjects)
                {
                    if (obj.body == null) continue; //Отсеиваем все области, оставляем только препятствия
                    allObjs.Add(obj.body, obj); //Добавление в словарь найденного препятствия
                }
            }

            public void BeginContact(Contact contact) //Добавляет тело, с которым произошел контакт, в словарь
            {
                Body f;
                foreach (Body overseen in guarded)
                {
                    if (contact._fixtureA.Body == overseen) f = contact._fixtureB.Body; //Если столкновение произошло именно с нашим бампером
                    else if (contact._fixtureB.Body == overseen) f = contact._fixtureA.Body;
                    else continue;
                    if (!allObjs.ContainsKey(f)) continue;
                    collided[overseen].Add( allObjs[f]); //Найденный объект, с которым столкнулся наш бампер записываем в словарь collided
                }
            }

            public double getK(Body forBody)//Та масса, которую агент сдвигает, сверх собственной. аргумент - тело, с которым считались столкновения
            {
                double k = 0;
                foreach (envObject eobj in collided[forBody]) //Смотрим только значения (envObject), а есть еще collided.Keys (Body)
                    k+=eobj.beh.k;
                return k;
            }
            public void addGuarded(Body body)
            {
                guarded.Add(body);
                collided[body] = new List<envObject>();
            }
            public void removeGuarded(Body body)
            {
                guarded.Remove(body);
                collided.Remove(body);
            }

            public void EndContact(Contact contact) //Удаляет тело, с которым контакт закончился, из словаря
            {
                Body f;
                foreach (Body overseen in guarded)
                {
                    if (contact._fixtureA.Body == overseen) f = contact._fixtureB.Body; //Если столкновение произошло именно с нашим бампером
                    else if (contact._fixtureB.Body == overseen) f = contact._fixtureA.Body;
                    else continue;
                    if (!allObjs.ContainsKey(f)) continue;
                    collided[overseen].Remove(allObjs[f]); //Найденный объект, с которым столкнулся наш бампер записываем в словарь collided
                }
            }

            public void PostSolve(Contact contact, ContactImpulse impulse) //Упоминаем так как прописана в интерфейсе ContactListener
            {
            }

            public void PreSolve(Contact contact, Manifold oldManifold) //Упоминаем так как прописана в интерфейсе ContactListener
            {
            }
        }

        reporter guardianAngel; //Следит за всеми столкновениями бампера
        Form1 form;
        public Environment(Physics px, List<Car> cars ,List<Vec2> targ, Form1 mainForm)
        {
            form = mainForm;
            targets = targ;
            physics = px;

            List<Body> noses = new List<Body>();
            foreach(Car car in cars)
            {
                noses.Add(car.bumper);
            }
            guardianAngel = new reporter(noses,objects);
            physics.world.SetContactListener(guardianAngel);

            robots = cars;
        }
        public void addRobot(Car car, Vec2 target)
        {
            targets.Add(target);
            robots.Add(car);

            guardianAngel.addGuarded(car.bumper);
        }
        public void removeRobot(int ind)
        {

            var robot = robots[ind];
            guardianAngel.removeGuarded(robot.bumper);
            robot.disposeBodies(physics);
            robots.RemoveAt(ind);
            targets.RemoveAt(ind);
        }

        /*ИНТЕРФЕЙС АГЕНТА*/
        public void act(CarControl cc,int id)
        {   
            robots[id].cc = cc;
        }

        public PerceptParams percept(int id)
        {
            var robot = robots[id];
            var target = targets[id];
            var ang_target = robot.Get_Angle_of_Point(target.X, target.Y);//конечная точка
            
            float cm, d;
            robot.lidar.Calculate(out cm, out d);
            Pose p;
            Vec2 ps = robot.body.GetPosition();
            p.xc = ps.X;
            p.yc = ps.Y;
            p.angle_rad =  - robot.Get_Forward_to_X_Angle();//переход в видимую систему координат.
            var kaif = 1+guardianAngel.getK(robot.bumper)/1520;
            
            var ksurf = kaif * (1+robot.body.GetLinearDamping());//важно: дефолтный вес равен 1!
            PerceptParams pp = new PerceptParams { lidar_cm = cm, lidar_d = d, targ_angle = ang_target, dist = (target - robot.body.GetPosition()).Length(), nose= robot.bumper.GetPosition() ,
                p = p,kaif=kaif,ksurf=ksurf, cameraView = form.getLocalViewImage(robot), velocity = robot.body.GetLinearVelocity().Length()};
            return pp;
        }
        /*КОНЕЦ ИНТЕРФЕЙСА АГЕНТА*/


        /*СПСИСОК ВСЕХ ОБЪЕКТОВ В СРЕДЕ*/
        public List<envObject> objects = new List<envObject>();
        public List<objInfo> objInfos = new List<objInfo>();
        public List<Vec2> targets = new List<Vec2>();

        //создание в указанном месте прямоугольного объекта-препятствия, движимого
        public envObject CreateFromInfo(objInfo info)
        {
            switch (info.type)
            {
                case "box":
                    return CreateBox(info.p, info.size, info.weight, info.color);
                case "crcl":
                    return CreateCircle(info.p, info.radius, info.weight, info.color);
                case "wall":
                    return CreateWall(info.p,info.size,info.color);
                case "col":
                    return CreateColumn(info.p,info.radius,info.color);
                case "czone":
                    return CreateCirZone(info.p, info.radius, info.weight, info.color);
                case "rzone":
                    return CreateRectZone(info.p, info.size, info.weight, info.color);
            }
            return null;
        }
        public envObject CreateBox(Pose pose, Vec2 size, float mass, Color obcolor)
        {
            objInfos.Add(new objInfo { type = "box", p = pose, size = size, weight = mass, color = obcolor });
            BodyBehaviour b = new BodyBehaviour { isDynamic = true, isPassable = false, k = mass };
            InnerParams i = new InnerParams { features_angle = 6, features_line = 7, color = obcolor, features_gleam = 0, glow = 0, heat = 0 };
            envObject obj = new envObject_rect(physics.CreateBox(pose,size, b),b,i,size);
            objects.Add(obj);
            guardianAngel.allObjs.Add(obj.body, obj);
            return obj;
        }

        //создание в указанном месте круглого объекта-препятствия, движимого
        public envObject CreateCircle(Pose pose, float radius, float mass, Color obcolor)
        {
            objInfos.Add(new objInfo { type = "crcl", p = pose, radius = radius, weight = mass, color = obcolor });
            BodyBehaviour b = new BodyBehaviour { isDynamic = true, isPassable = false, k = mass };
            InnerParams i = new InnerParams { features_angle = 0, features_line = 2, color = obcolor, features_gleam = 0, glow = 0, heat = 0 };
            envObject obj = new envObject_crcl(physics.CreateCircle(pose, radius, b), b, i, radius);
            objects.Add(obj);
            guardianAngel.allObjs.Add(obj.body, obj);
            return obj;
        }

        //создание в указанном месте прямоугольного объекта-препятствия, статичного (стена)
        public envObject CreateWall(Pose pose, Vec2 size, Color obcolor)
        {

            objInfos.Add(new objInfo { type = "wall", p = pose, size = size, color = obcolor });
            BodyBehaviour b = new BodyBehaviour { isDynamic = false, isPassable = false, k =float.PositiveInfinity};
            InnerParams i = new InnerParams { features_angle = 4, features_line = 6, color = obcolor, features_gleam = 0, glow = 0, heat = 0 };
            envObject obj = new envObject_rect(physics.CreateBox(pose,size, b),b,i,size);
            objects.Add(obj);
            guardianAngel.allObjs.Add(obj.body, obj);
            return obj;
        }

        //создание в указанном месте круглого объекта-препятствия, статичного (колонна)
        public envObject CreateColumn(Pose pose, float radius, Color obcolor)
        {
            objInfos.Add(new objInfo { type = "col", p = pose, radius = radius, color = obcolor });
            
            BodyBehaviour b = new BodyBehaviour { isDynamic = false, isPassable = false, k = float.PositiveInfinity };
            InnerParams i = new InnerParams { features_angle = 0, features_line = 2, color = obcolor, features_gleam = 0, glow = 0, heat = 0 };
            envObject obj = new envObject_crcl(physics.CreateCircle(pose, radius, b), b, i, radius);
            objects.Add(obj);
            guardianAngel.allObjs.Add(obj.body, obj);
            return obj;
        }

        //создание в указанном месте прямоугольной области-места, проходимого (болото)
        public envObject CreateRectZone(Pose pose, Vec2 size, float k_, Color obcolor)
        {
            objInfos.Add(new objInfo { type = "rzone", p = pose, size = size, weight = k_, color = obcolor });
            
            BodyBehaviour b = new BodyBehaviour { isDynamic = false, isPassable = true, k = k_ };
            InnerParams i = new InnerParams { features_angle = 0, features_line = 0, color = obcolor, features_gleam = 2, glow = 0, heat = 0 };
            envObject obj = new envObject_rectArea( physics.CreateArea(pose,size,b), b, i, size);
            objects.Add(obj);
            return obj;
        }
        //создание в указанном месте круглой области-места, проходимого (лужа)
        public envObject CreateCirZone(Pose pose, float radius, float k_, Color obcolor)
        {
            objInfos.Add(new objInfo { type = "czone", p = pose, radius = radius, weight = k_, color = obcolor });

            BodyBehaviour b = new BodyBehaviour { isDynamic = false, isPassable = true, k = k_ };
            InnerParams i = new InnerParams { features_angle = 0, features_line = 2, color = obcolor, features_gleam = 0, glow = 0, heat = 0 };
            envObject obj = new envObject_crclArea(physics.CreateArea(pose, new Vec2(radius, radius), b), b, i, radius);
            objects.Add(obj);
            return obj;
        }


        //Создание источника огня
        public envObject CreateFire(Pose pose, float heat)
        {
            BodyBehaviour b = new BodyBehaviour { isDynamic = false, isPassable = false, k = float.PositiveInfinity };
            InnerParams i = new InnerParams { features_angle = 8, features_line = 9, color = new Color(0, 0, 0), features_gleam = 4, glow = 4, heat = heat };
            envObject obj = new envObject_fire(physics.CreateCircle(pose, 0.25f, b), b, i, 0.25f);
            objects.Add(obj);
            guardianAngel.allObjs.Add(obj.body, obj);
            return obj;
        }

        public void removeLastObject()
        {
            envObject victim = objects[objects.Count - 1];
            if (victim.body != null)
            {
                physics.removeBody(victim.body);
                guardianAngel.allObjs.Remove(victim.body); //тело жертвы >.<
            }
            else
                physics.removeArea(((dynamic)victim).area);
            objInfos.RemoveAt(objInfos.Count - 1);
            objects.RemoveAt(objects.Count - 1);
        }

        double kLast=1; //Сумма всех коэфициентов трения, действующих на робота в данном месте

        public void step(float dt)
        {
            foreach (Car robot in robots)
            {
                robot.OnBeforeStep(0, dt); //
                //обсчёт замедления от грунта
                Vec2 pose = robot.body.GetPosition();
                float kaf = 0;
                //для всех областей на которые заехал робот, если они проходимые, то пусть затормаживают движение
                foreach (Area a in physics.getGround(new Pose { xc = pose.X, yc = pose.Y, angle_rad = robot.body.GetAngle() }, new Vec2(robot.p.w, robot.p.h)))
                {
                    if (a.b.isPassable) kaf += a.b.k;
                    else throw new Exception("Car intersects with an unpassable area. If that is only a minor intersection without any consequences, remove this exception, else - remove the area and make it a box2d body to handle collisions");
                }
                kLast = kaf + robot.body.GetLinearDamping();
                robot.body.SetLinearDamping(robot.body.GetLinearDamping() + kaf);
            }
                physics.Step(dt);
            //robot.OnAfterStep(dt);//обсчитывает лидар, комментить для выключения
        }

        /*ОТРИСОВКА*/
        public void draw(GraphicsHelper g)
        {
            g.Clear();
            foreach (envObject e in objects)
                e.draw(g);
            g.UpdateGraphics();
            foreach(Car robot in robots)
                robot.Draw(g);
            foreach(Vec2 target in targets)
                g.DrawRectangle(target.X, target.Y, 0.3f, 0.3f, 0f, g.GetPen(System.Drawing.Color.Pink), true);

        }
    }
}
