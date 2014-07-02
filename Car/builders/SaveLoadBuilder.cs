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
    partial class SaveLoadBuilder : Builder
    {
        Form1 form;
        public SaveLoadBuilder(Form1 form)
        {
            InitializeComponent();
            this.form = form;
            
        }
        public override void reportClick(float x, float y)
        {
          
        }

        private void saveBTN_Click(object sender, EventArgs e)
        {
            if (!form.saveToFile(nameBox.Text))
                MessageBox.Show("Failed to save");
        }

        private void loadBTN_Click(object sender, EventArgs e)
        {
            form.initArena(true);
            if (!form.loadFromFile(nameBox.Text))
                MessageBox.Show("Failed to load");  
        }
    }
}
