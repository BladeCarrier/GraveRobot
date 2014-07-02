using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Box2DX.Dynamics;

namespace Car.builders
{
    partial class RemoverBuilder : Builder
    {
        Environment env;
        public System.Drawing.Color SysObjColor;
        Box2DX.Dynamics.Color Box2dObjColor = new Box2DX.Dynamics.Color (0, 0, 0);

        public RemoverBuilder(Environment e)
        {
            InitializeComponent();
            env = e;
            
        }
        public override void reportClick(float x, float y)
        {

        }



        private void RemLastBtn_Click(object sender, EventArgs e)
        {
            if (env.objects.Count>0)
                env.removeLastObject();
        }
    }
}
