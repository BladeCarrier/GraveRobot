using System;
using System.Collections.Generic;
using System.Text;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;
namespace Car
{
    //структура, описывающая положение 2d-тела
    public struct Pose
    {
        public float xc, yc, angle_rad;
    }
    //область. НЕпрепятствие, которое может влиять на передвижение.
    public class Area
    {
        public Pose p;
        public Vec2 sz;
        public bool isCircle; // если истинно, работает как эллипс, ложно - работает, как прямоугольник
        public BodyBehaviour b;
        public Vec2 C;//центр
        public float r;//радиус
        
        public Area(Pose p_, Vec2 sz_, BodyBehaviour b_, bool isCircle_)
        {
            p = p_;
            sz = sz_;
            b = b_;
            isCircle = isCircle_;
            r = (float)(System.Math.Sqrt((sz.X * sz.X + sz.Y * sz.Y) / 4)); //расстояние от центра до угла
            C = new Vec2(p.xc, p.yc);
            if (isCircle)
            {
                r = System.Math.Max(sz.X, sz.Y) / 2;//радиус
            }
        }


        //Пересекается ли с отдельно взятой машиной. ВАЖНО!!! - сейчас это работает только при пересечением с ЦЕНТРОМ машины.
        public bool intersects(Pose p2, Vec2 sz2)
        {
            Vec2 vc2 = new Vec2 (p2.xc,p2.yc); //Записываем координаты центра, не учитываем угол поворота
            if (((C - vc2).Length() > r)) return false;
            if (isCircle) return true; //если центр внутри радиуса у круга, это интерсект.
            vc2.X -= p.xc;
            vc2.Y -= p.yc;//зачем: повернуть точку в базис, где наш прямоугольник не повёрнут и стоит в начале координат, после чего смотреть, не вылезает ли она за половины ширины или длины.
            Vec2 vc2prime = new Vec2();
            vc2prime.X = (float)(vc2.X*System.Math.Cos(p.angle_rad) + vc2.Y*System.Math.Sin(p.angle_rad));//check me!
            vc2prime.Y = (float)(vc2.Y*System.Math.Cos(p.angle_rad) - vc2.X*System.Math.Sin(p.angle_rad));
            vc2 = vc2prime;
            if (((System.Math.Abs(vc2.X) > sz.X / 2) || System.Math.Abs(vc2.Y) > sz.Y / 2)) return false;
            return true;
        }
    }

    //класс, упрощающий работу с физикой
    public class Physics
    {
        //переменная для управления объектами моделируемого мира
        public World world;

        public Physics()
        {
            var worldAABB = new AABB();
            float sz = 200; //warning - mn
            worldAABB.LowerBound.Set(-sz, -sz);
            worldAABB.UpperBound.Set(sz, sz);
            bool doSleep = true;
            world = new World(worldAABB, new Vec2(0, 0), doSleep);
        }
        
        //шаг рассчета физической модели мира в Box2D
        public void Step(float dt)
        {
            world.Step(dt, 1, 1);
        }

        //создаёт пряпятствие с указанными параметрами
        public Body CreateBox(Pose p, Vec2 sz,  BodyBehaviour b)
        {
            if (b.isPassable) throw new Exception("НЕпрепятствия создаются через createArea");
            var bodyDef = new BodyDef();
            bodyDef.Position.Set(p.xc, p.yc);
            bodyDef.Angle = p.angle_rad;
            
            var body = world.CreateBody(bodyDef);
            
            // описание формы физического объекта
            var shapeDef = new PolygonDef();
            shapeDef.SetAsBox(sz.X / 2, sz.Y / 2);

            if (b.isDynamic)
            {
                // вычисление плотности коробки по ее массе
                shapeDef.Density = b.k / (sz.X * sz.Y);
            }

            // в общем случае физическое тело может состоять из нескольких геом. объетов.
            // здесь он только один - shapeDef
            body.CreateFixture(shapeDef);

            if (b.isDynamic) body.SetMassFromShapes();

            // устанавливаем замедление скорости тела со временем (для иммитации трения о поверхность)
            body.SetLinearDamping(0.3f);
            body.SetAngularDamping(0.3f);
            return body;
            
        }

        //Предикат для getGround, да - елси есть пересечение, нет - если нет пересечения
        bool match(Area a){ return a.intersects(pcar,szcar); } Pose pcar; Vec2 szcar;

        //Список всех областей, с которыми есть пересечение
        public List<Area> getGround(Pose p,Vec2 sz)
        {
            pcar = p; szcar = sz;
            return areas.FindAll(new Predicate<Area>(match)); //Метод списка, которые возавращает все, что соответствует предикату
        }

        public void removeBody(Body b)
        {
            world.DestroyBody(b);
        }
        public void removeArea(Area a)
        {
            areas.Remove(a);
        }
        public Body CreateCircle(Pose p, float r,  BodyBehaviour b)
        {
            if (b.isPassable) throw new Exception("НЕпрепятствия создаются через createArea");
            var bodyDef = new BodyDef();
            bodyDef.Position.Set(p.xc, p.yc);
            bodyDef.Angle = p.angle_rad;
            
            var body = world.CreateBody(bodyDef);
            
            // описание формы физического объекта
            var shapeDef = new CircleDef();
            shapeDef.Radius = r;
            

            if (b.isDynamic)
            {
                // вычисление плотности коробки по ее массе
                shapeDef.Density = b.k / r*r;
            }

            // в общем случае физическое тело может состоять из нескольких геом. объетов.
            // здесь он только один - shapeDef
            body.CreateFixture(shapeDef);

            if (b.isDynamic) body.SetMassFromShapes();

            // устанавливаем замедление скорости тела со временем (для иммитации трения о поверхность)
            body.SetLinearDamping(0.3f);
            body.SetAngularDamping(0.3f);
            return body;
            
        }
        //создаёт неподвижное сочленение двух объектов
        public PrismaticJoint CreateFixedJoint(Body b1,Body b2) 
        {
            var jdef = new PrismaticJointDef();
            jdef.Initialize(b1, b2, b2.GetWorldCenter(), new Vec2(1, 0));
            jdef.EnableLimit = true;
            jdef.UpperTranslation = jdef.LowerTranslation = 0;
            return (PrismaticJoint) world.CreateJoint(jdef);
        }
        List <Area> areas = new List<Area>();//список всех областей.
        //создание области
        public Area CreateArea(Pose p, Vec2 sz, BodyBehaviour b,bool isEllipse = false)
        {
            if (!b.isPassable) throw new Exception("препятствия создаются через createObject");
            Area a = new Area(p, sz, b, isEllipse);
            areas.Add(a);
            return a;
            

        }

    }
}
