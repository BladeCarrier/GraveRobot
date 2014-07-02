using System;
using System.Collections.Generic;
using System.Text;
using Box2DX.Dynamics;
using Box2DX.Common;
using Box2DX.Collision;
using System.Drawing;
using System.Windows.Forms;

namespace Car
{
    //набор управляющих воздействий на автомобиль
    
    public struct CarControl
    {
        public float u_gas;
        public float u_steer;
        public float u_brake;
    }

    //параметры авто
    public struct CarParams
    {
        public float w, h; // м
        public float w_base, h_base; //  м
        public float damping; // 0.1
        public float max_speed; //  км/ч
        public float avg_acceleration; // м/с^2
        public float mass; //  кг
        public float max_steer_angle; // град.

        public void InitDefault() //Параметры машины по умолчанию
        {
            w = 1.7f; h = 4.0f;
            w_base = 1.5f; h_base = 2.5f;
            damping = 0.1f; max_speed = 150f;
            avg_acceleration = 2.3f;
            mass = 1520f;
            max_steer_angle = 45f;
        }
    }

    //модель автомобиля
    public class Car
    {
        public int guid;

        public CarParams p;

        public Body body, bodyFW1, bodyBW1, bumper;
        public RevoluteJoint jFW1;
        public PrismaticJoint jBW1;

        //Секу Д. : а вдруг у нас несколько..
        //4.06.2014 Таисия: ну пусть пока будет.
        public Dictionary<string, Lidar> lidars = new Dictionary<string, Lidar>();

        public Car() { }
        public Car(CarParams p, int guid)
        {
            this.p = p;
            this.guid = guid;
        }

        public float car_angle0 = (float) System.Math.PI/2; //угол между вектором направленности машины и локальной осью X машины (т.е. между y и x)

        float damping = 0.1f; //Трение

        public Pose pos;
        public void Move(Pose pose)
        {
            pos = pose;
            body.SetPosition(new Vec2(pose.xc, pose.yc));
            body.SetAngle(pose.angle_rad);
            bumper.SetPosition(new Vec2(pose.xc, pose.yc + p.h_base / 2 + 1f));
        }
        public void Create(Physics ph, Pose pose)
        {
            pos = pose;
            body = ph.CreateBox(   new Pose { xc = pose.xc, yc = pose.yc }, 
                                   new Box2DX.Common.Vec2 { X = p.w, Y = p.h }, 
                                   new BodyBehaviour { isDynamic = true, k = 0.98f * p.mass }); //Создает корпус робота

            bumper = ph.CreateBox( new Pose { xc = pose.xc, yc = pose.yc + p.h_base / 2 +1f}, 
                                   new Box2DX.Common.Vec2 { X = p.w * 0.5f, Y = p.h / 10 }, 
                                   new BodyBehaviour { isDynamic = true, k = 0.01f * p.mass }); //Создает бампер
           
            bodyFW1 = ph.CreateBox(new Pose { xc=pose.xc, yc = pose.yc+p.h_base / 2 }, 
                                   new Box2DX.Common.Vec2 { X = p.w/10, Y = p.h/10 }, 
                                   new BodyBehaviour { isDynamic = true, k = 0.01f * p.mass }); //Создает переднее колесо
            
            bodyBW1 = ph.CreateBox(new Pose { xc=pose.xc, yc = pose.yc+-p.h_base / 2 }, 
                                   new Box2DX.Common.Vec2 { X = p.w / 10, Y = p.h / 10 }, 
                                   new BodyBehaviour { isDynamic = true, k = 0.01f * p.mass }); //Создает заднее колесо
           
            //Связь переднего колеса и корпуса (поворотная)
            var jFW1def = new RevoluteJointDef(); //Создает "сустав"
            jFW1def.Initialize(body, bodyFW1, bodyFW1.GetWorldCenter()); //Инициализирует "сустав" между корпусом и колесом
            jFW1def.EnableMotor = true; //Есть мотор
            jFW1def.MaxMotorTorque = 1000; //Максимальный момент
            jFW1def.EnableLimit = true; //Есть ограничения
            jFW1def.LowerAngle = -p.max_steer_angle * Helper.angle_to_rad; //Минимальный угол поворота колеса в рад
            jFW1def.UpperAngle = p.max_steer_angle * Helper.angle_to_rad; //Минимальный угол поворота колеса в рад
            jFW1 = (RevoluteJoint)ph.world.CreateJoint(jFW1def); //Создает переднее колесо

            //Связь заднего колеса и корпуса (статичная)
            var jBW1def = new PrismaticJointDef(); //Создает неповоротный сустав
            jBW1def.Initialize(body, bodyBW1, bodyBW1.GetWorldCenter(), new Vec2(1, 0));
            jBW1def.EnableLimit = true;
            jBW1def.UpperTranslation = jBW1def.LowerTranslation= 0; //Пределы поворота от 0 до 0, поворачивать не может
            jBW1 = (PrismaticJoint)ph.world.CreateJoint(jBW1def);

            //Связь бампера и корпуса (статичная)
            var jBumpdef = new PrismaticJointDef(); 
            jBumpdef.Initialize(body, bumper, bumper.GetWorldCenter(), new Vec2(1, 0));
            jBumpdef.EnableLimit = true;
            jBumpdef.UpperTranslation = jBumpdef.LowerTranslation = 0;
            var jBump = (PrismaticJoint)ph.world.CreateJoint(jBumpdef);

            LidarParams lp=new LidarParams(){dir_deg=0, d0=2.2f, dist=20, fov_deg=60, x0=0, y0=0, num_rays=20}; //Лидар, пока не нужен

            body.SetAngle(pose.angle_rad);
            body.SetLinearDamping(damping);
            body.SetAngularDamping(0.2f);

            forces[body] = new Vec2();
            forces[bodyFW1] = new Vec2();
            forces[bodyBW1] = new Vec2();

            var p1 = new LidarParams();
            p1.InitDefault();
            p1.d0 = p.h / 2 + 0.2f;
            InitLidar(p1, "FWD");
        }
        public void disposeBodies(Physics ph)
        {
            ph.world.DestroyBody(body);
            ph.world.DestroyBody(bumper);
            ph.world.DestroyBody(bodyBW1);
            ph.world.DestroyBody(bodyFW1);
        }

        public void InitLidar(LidarParams p, string ind)
        {
            lidars[ind] = new Lidar(p, this);
        }

        Dictionary<Body, Vec2> forces = new Dictionary<Body, Vec2>();

        void killWheelOrthogonalVelocity(Body body, Body wheel, float dt)
        {
            var p_body = body.GetPosition();
            var p_wheel = wheel.GetPosition();
            var velocity = wheel.GetLinearVelocityFromLocalPoint(Vec2.Zero); //Альтернатва vec = new Vec2(0, 0)

            var vec1 = wheel.GetXForm().R.Col1; //Вектор, направленный вдоль колеса
            var projection = Vec2.Dot(velocity, vec1); //Численное значение продольной скорости
            vec1 *= projection; // Вектор скорости

            var V1 = vec1.Length();
            var k = V1 / 0.001f; // превышение боковой скорости 1 мм / с
            if (k < 1) k = 0;

#warning magic
            else k = (float)System.Math.Pow(k, 0.4);

            if (k > 100) k = 100;

            float force = -k * p.mass; //warning - mn           

            if(V1>0)
            {
                var force_vec = force/V1 * vec1;
                wheel.ApplyForce(force_vec, p_wheel);
            }
        }

        float limitVelocity(Body hull, float max)
        {
            var velocity = hull.GetLinearVelocityFromLocalPoint(Box2DX.Common.Vec2.Zero);

            var V = velocity.Length();
            if (V > max)
            {
                var k = max / V;
                hull.SetLinearVelocity(velocity * k);
            }
            if (V <0.01f)
            {
                hull.SetLinearVelocity(velocity * 0);
            }
            return V;
        }

        void brake(Body hull, float decceleration, float dt)
        {
            if (decceleration < 0.0001f) return;
            var dv = decceleration * dt;
            var velocity = hull.GetLinearVelocityFromLocalPoint(Box2DX.Common.Vec2.Zero);
            var V = velocity.Length();
            var V1=System.Math.Max(0, V-dv);
            var k = V < 0.0001 ? 0 : V1 / V;
            hull.SetLinearVelocity(velocity * k);
        }

        public float speed;

        public void OnBeforeStep(int iter, float dt)
        {
            var d0 = (1 - cc.u_gas) * 0.5f / Helper.ms_to_kmh;
            var d1 = 10 / Helper.ms_to_kmh;
            var deceleration = d0 + cc.u_brake * d1;

            brake(body, deceleration, dt);

            if (iter == 0)
            {
                foreach (var x in lidars.Values)
                {
                    if (x != null) x.is_calculated = false; //для кэширования лидара
                }

                body.SetLinearDamping(damping * (1 - cc.u_gas));
            }

            killWheelOrthogonalVelocity(body, bodyFW1, dt);
            killWheelOrthogonalVelocity(body, bodyBW1, dt);

            float max_speed = p.max_speed / Helper.ms_to_kmh;
#warning magic
            max_speed *= 1.05f;

            speed=limitVelocity(body, max_speed);


            float k = (max_speed - speed) / max_speed;
            if (k < 0) k = 0;
#warning magic
            //k = (float)System.Math.Pow(k, 0.5);
            k *= 1.1f;

            float k0 = (max_speed - 100 / Helper.ms_to_kmh) / max_speed;
            var acc_max = 2 * p.avg_acceleration / (1 + k0);
            float acc = k * acc_max;

            //float acc = 2.8f;

            //Driving
            Vec2 ldirection = bodyFW1.GetXForm().R.Col2;
            ldirection *= acc * p.mass * cc.u_gas;

            bodyFW1.ApplyForce(ldirection, bodyFW1.GetPosition());
            //bodyBW1.ApplyForce(ldirection, bodyBW1.GetPosition());

            const float k2 = 2;
            //Steering
            float A = System.Math.Abs(cc.u_steer), s=System.Math.Sign(cc.u_steer);
            float B = 1 - A;
            jFW1.MotorSpeed = 3 *s* A - B * k2 * jFW1.JointAngle;
        }

        public CarControl cc;

        public Lidar lidar { get { return lidars["FWD"]; } }

        
        public void OnAfterStep(float dt)
        {
            if (lidar != null) lidar.Simulate();
        }


        public void Draw(GraphicsHelper gh)
        {
            var pos = body.GetPosition();
            float k = Helper.angle_to_rad;
            var a1 = body.GetAngle() / k;
            var a2 = bodyFW1.GetAngle() / k;
            var th = Helper.GetRelAngle(a1, a2, 360 * k);
            gh.DrawCar(pos.X, pos.Y, p.w, p.h, a1, p.h_base, p.w_base, th);
            if (lidar != null) lidar.Draw(gh);
        }
        public float Get_Forward_to_X_Angle()//угол вектора направления машины относительно оси X
        {
            var x_angle = body.GetAngle();
            var fwd_angle = x_angle + car_angle0;
            Helper.LimitAngle(ref fwd_angle, 2 * (float)System.Math.PI);
            return fwd_angle;
        }

        public float Get_Angle_of_Point(float x, float y)
        {
            var p = body.GetPosition();
            x -= p.X; y -= p.Y;
            var a1 = Get_Forward_to_X_Angle();
            var a2 = Helper.Get_Point_Angle_Relative_to_X(x, y,  2 * (float)System.Math.PI);
            return Helper.GetRelAngle(a1, a2, 2 * (float)System.Math.PI);
        }
    }

    }



