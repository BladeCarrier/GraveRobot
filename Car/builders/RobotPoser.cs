    using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Car.builders
{
    partial class RobotPoser : Builder
    {
        Environment env;
        Physics p;
        Form1 form;
        public RobotPoser(Form1 form)
        {
            InitializeComponent();
            env = form.env;
            p = form.env.physics;
            this.form = form;
        }
        public override void reportClick(float x, float y)//перемещает робота в место клика
        {
            try
            {
                int id = int.Parse(robotListBox.Text);
                if (id >= env.robots.Count)
                {
                    id = env.robots.Count;
                    env.addRobot(Form1.createCar( new Pose { xc = 10, yc = 10, angle_rad = 0 }, env.physics),
                        new Box2DX.Common.Vec2(15, 10));
                }
                if (radioButton1.Checked)
                {
                    //раньше это был трай-кеч. Если до 13.06.2014 не вылетит ни разу, удалите коммент
                    env.robots[id].Move(new Pose { xc = x / 10, yc = y / 10, angle_rad = Convert.ToSingle(textBox1.Text) });
                }
                else
                {
                    env.targets[id] = new Box2DX.Common.Vec2 { X = x / 10, Y = y / 10 };
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to commit action\n"+e.Message);
            }
            
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            try
            {
                env.removeRobot(int.Parse(robotListBox.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to commit action\n" + ex.Message);
            }
            
        }



    }
}
