using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Car
{

    public class LicsDataWindow : System.Windows.Forms.Form
    {
        public System.Windows.Forms.RichTextBox treeVisualisationLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.RichTextBox encbox;
        public System.Windows.Forms.ListView ListSamples;
        public void InitializeComponent()
        {
            this.ListSamples = new System.Windows.Forms.ListView();
            this.treeVisualisationLabel = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.encbox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // ListSamples
            // 
            this.ListSamples.Location = new System.Drawing.Point(12, 225);
            this.ListSamples.Name = "ListSamples";
            this.ListSamples.Size = new System.Drawing.Size(430, 150);
            this.ListSamples.TabIndex = 1;
            this.ListSamples.UseCompatibleStateImageBehavior = false;
            // 
            // treeVisualisationLabel
            // 
            this.treeVisualisationLabel.Location = new System.Drawing.Point(12, 16);
            this.treeVisualisationLabel.Name = "treeVisualisationLabel";
            this.treeVisualisationLabel.Size = new System.Drawing.Size(430, 190);
            this.treeVisualisationLabel.TabIndex = 2;
            this.treeVisualisationLabel.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Текущее состояние дерева";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 218);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Выборка примеров";
            // 
            // encbox
            // 
            this.encbox.Location = new System.Drawing.Point(448, 16);
            this.encbox.Name = "encbox";
            this.encbox.Size = new System.Drawing.Size(174, 359);
            this.encbox.TabIndex = 5;
            this.encbox.Text = "";
            // 
            // LicsDataWindow
            // 
            this.ClientSize = new System.Drawing.Size(446, 384);
            this.ControlBox = false;
            this.Controls.Add(this.encbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeVisualisationLabel);
            this.Controls.Add(this.ListSamples);
            this.Name = "LicsDataWindow";
            this.Load += new System.EventHandler(this.LicsDebugWIndow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public void LicsDebugWIndow_Load(object sender, EventArgs e)
        {
            this.ListSamples.LargeImageList = new System.Windows.Forms.ImageList();

        }
    }
}
