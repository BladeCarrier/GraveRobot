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
    public partial class Builder : Form
    {
        public Builder()
        {
            InitializeComponent();
        }
        virtual public void reportClick(float x, float y)
        {
        }
        virtual public void reportMove(float x, float y)
        {
        }
    }
}
