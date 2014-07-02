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
    partial class ObstacleBuilder : Builder
    {
        Environment env;
        public System.Drawing.Color SysObjColor;
        public bool Pass, Dyn;
        Box2DX.Dynamics.Color Box2dObjColor = new Box2DX.Dynamics.Color(0, 0, 0);
        public ObstacleBuilder(Environment e)
        {
            InitializeComponent();
            env = e;
            Pass = radioPassYes.Checked;
            Dyn = radioDynYes.Checked;
        }
        public override void reportClick(float x, float y)
        {
            try
            {
                if (Dyn == true)
                {
                    env.CreateBox(new Pose { xc = x / 10, yc = y / 10, angle_rad = 0 }, new Box2DX.Common.Vec2(Convert.ToSingle(MassValue.Text), Convert.ToSingle(SizeBValue.Text)), Convert.ToInt32(MassValue.Text), Box2dObjColor);
                }
                else
                {
                    if (Pass == true)
                    {
                        env.CreateRectZone(new Pose { xc = x / 10, yc = y / 10, angle_rad = 0 }, new Box2DX.Common.Vec2(Convert.ToSingle(SizeAValue.Text), Convert.ToSingle(SizeBValue.Text)), Convert.ToInt32(PassValue.Text), Box2dObjColor);
                    }
                    else
                    {
                        env.CreateWall(new Pose { xc = x / 10, yc = y / 10, angle_rad = 0 }, new Box2DX.Common.Vec2(Convert.ToSingle(SizeAValue.Text), Convert.ToSingle(SizeBValue.Text)), Box2dObjColor);
                    }

                }                
            }
            catch(Exception e)
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                SysObjColor = colorDialog1.Color;
                //Конвертация цветов видовс в цвета бокс2д
                Box2dObjColor = GraphicsHelper.ConvertColor(SysObjColor);
            }
        }

        private void radioDynYes_CheckedChanged(object sender, EventArgs e)
        {
            //Если движимо, то проходимость выключена
            if (radioDynYes.Checked == true)
            {
                radioDynNo.Checked = false;
                Dyn = true;
                GroupPass.Enabled = false;
                MassValue.Enabled = true;

            }
            else
            {
                radioDynNo.Checked = true;
                Dyn = false;
                GroupPass.Enabled = true;
                MassValue.Enabled = false;
            }
        }

        private void radioPassYes_CheckedChanged(object sender, EventArgs e)
        {
            if (radioPassYes.Checked == true)
            {
                radioPassNo.Checked = false;
                Pass = true;
                PassValue.Enabled = true;
            }
            else
            {
                radioPassNo.Checked = true;
                Pass = false;
                PassValue.Enabled = false;
            }
        }




    }
}
