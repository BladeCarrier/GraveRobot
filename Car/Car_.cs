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

        public void InitDefault()
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
        public CarParams p;
        public Body body, bodyFW1, bodyBW1;
        public RevoluteJoint jFW1;
        public PrismaticJoint jBW1;

        const float damping = 0.1f;

        //public RCS rcs;

        public Lidar lidar;

        public Vec2 forwardVec = new Vec2(0, 1);

        public Car(Physics ph,CarParams p,Pose pose)
        {
            this.p = p;

            car_angle0 = Helper.GetRelAngle(new Vec2(1, 0), forwardVec);

            body = ph.CreateBox(new Pose { xc = pose.xc, yc = pose.yc }, new Box2DX.Common.Vec2 { X = p.w, Y = p.h }, new BodyBehaviour { isDynamic = true, k = 0.98f * p.mass });

            bodyFW1 = ph.CreateBox(new Pose { xc = pose.xc, yc = pose.yc + p.h_base / 2 }, new Box2DX.Common.Vec2 { X = p.w / 10, Y = p.h / 10 }, new BodyBehaviour { isDynamic = true, k = 0.01f * p.mass });
            bodyBW1 = ph.CreateBox(new Pose { xc = pose.xc, yc = pose.yc + -p.h_base / 2 }, new Box2DX.Common.Vec2 { X = p.w / 10, Y = p.h / 10 }, new BodyBehaviour { isDynamic = true, k = 0.01f * p.mass });

            var jFW1def = new RevoluteJointDef();
            jFW1def.Initialize(body, bodyFW1, bodyFW1.GetWorldCenter());
            jFW1def.EnableMotor = true;
            jFW1def.MaxMotorTorque = 1000;
            jFW1def.EnableLimit = true;
            jFW1def.LowerAngle = -p.max_steer_angle * Helper.angle_to_rad;
            jFW1def.UpperAngle = p.max_steer_angle * Helper.angle_to_rad;

            jFW1 = (RevoluteJoint)ph.world.CreateJoint(jFW1def);

            var jBW1def = new PrismaticJointDef();
            jBW1def.Initialize(body, bodyBW1, bodyBW1.GetWorldCenter(), new Vec2(1, 0));
            jBW1def.EnableLimit = true;
            jBW1def.UpperTranslation = jBW1def.LowerTranslation = 0;
            jBW1 = (PrismaticJoint)ph.world.CreateJoint(jBW1def);

            //LidarParams lp = new LidarParams() { dir_deg = 0, d0 = 2.2f, dist = 20, fov_deg = 60, x0 = 0, y0 = 0, num_rays = 20 };
            var p1 = new LidarParams();
            p1.InitDefault();
            p1.d0 = p.h / 2 + 0.2f;
            InitLidar(p1);


            body.SetAngle(pose.angle_rad);

            //rcs = new RCS(this);
        }

        public void InitLidar(LidarParams lp)
        {
            lidar = new Lidar(lp, body);
        }

        void killWheelOrthogonalVelocity(Body hull, Body wheel)
        {
            var p_hull = hull.GetPosition();
            var p_wheel = wheel.GetPosition();
            var velocity = wheel.GetLinearVelocityFromLocalPoint(Vec2.Zero);

            var vec1 = wheel.GetXForm().R.Col1;
            var projection = Vec2.Dot(velocity, vec1);
            vec1 *= projection;

            var k = vec1.Length() / 0.001f; // превышение боковой скорости 1 мм / с
            if (k < 0.1f) k = 0.1f; if (k > 10f) k = 10f;
            float force = -k * 3000; //warning - mn

            vec1.Normalize();

            hull.ApplyForce(force * vec1, p_wheel);
        }

        float limitVelocity(Body hull, float max)
        {
            Vec2 velocity = hull.GetLinearVelocityFromLocalPoint(Box2DX.Common.Vec2.Zero);

            var vec1 = hull.GetXForm().R.Col2;
            var projection = Vec2.Dot(velocity, vec1);
            vec1 *= projection;

            var A = vec1.Length();
            if (A > max)
            {
                var k = max / A;
                hull.SetLinearVelocity(vec1 * k);
            }
            return A;
        }

        public float speed;

        public void OnBeforeSubStep(int iter, float dt)
        {
            if (iter == 0)
            {
                const float k1 = 1;
                var d0 = (1 - cc.u_gas) * damping;
                body.SetLinearDamping(d0 + cc.u_brake * k1);

                if (lidar != null) lidar.calculated_lidar = false; //для кэширования лидара
            }

            killWheelOrthogonalVelocity(body, bodyFW1);
            killWheelOrthogonalVelocity(body, bodyBW1);

            float max_speed = p.max_speed / Helper.ms_to_kmh;
            speed = limitVelocity(body, max_speed);
            
#warning car magic numbers
            //не трогай, оно и не пахнет
            float k = (max_speed - speed) / max_speed;
            float k0 = (max_speed - 100 / Helper.ms_to_kmh) / max_speed;
            var acc_max = 2 * p.avg_acceleration / (1 + k0);
            float acc = k * acc_max;
            //Driving
            Vec2 ldirection = bodyFW1.GetXForm().R.Col2;
            ldirection *= acc * p.mass * cc.u_gas;
            bodyFW1.ApplyForce(ldirection, bodyFW1.GetPosition());
            const float k2 = 2;
            //Steering
            jFW1.MotorSpeed = 2 * cc.u_steer - (1 - cc.u_steer) * k2 * jFW1.JointAngle;
        }

        public CarControl cc;

        public void OnAfterStep(float dt)
        {
            if (lidar != null) lidar.Simulate();
            //rcs.Simulate(dt);
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



        float car_angle0;
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
            var a2 = Helper.Get_Point_Angle_Relative_to_X_Radians(x, y);
            return Helper.GetRelAngle(a1, a2, 2 * (float)System.Math.PI);
        }
    }


}
