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
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label TimeLabel;
        public System.Windows.Forms.Label PathDistanceLabel;
        public System.Windows.Forms.Label NumCollisionsLabel;
        public System.Windows.Forms.Label AvgPassabilityLabel;
        public System.Windows.Forms.Label MaxSpeedLabel;
        public System.Windows.Forms.Label AvgSpeedLabel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        public System.Windows.Forms.ListView ListSamples;
        public void InitializeComponent()
        {
            this.ListSamples = new System.Windows.Forms.ListView();
            this.treeVisualisationLabel = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.encbox = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.TimeLabel = new System.Windows.Forms.Label();
            this.PathDistanceLabel = new System.Windows.Forms.Label();
            this.NumCollisionsLabel = new System.Windows.Forms.Label();
            this.AvgPassabilityLabel = new System.Windows.Forms.Label();
            this.MaxSpeedLabel = new System.Windows.Forms.Label();
            this.AvgSpeedLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 387);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Время в пути:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 410);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Пройденный путь:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 433);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Столкновения:";
            // 
            // TimeLabel
            // 
            this.TimeLabel.AutoSize = true;
            this.TimeLabel.Location = new System.Drawing.Point(151, 387);
            this.TimeLabel.Name = "TimeLabel";
            this.TimeLabel.Size = new System.Drawing.Size(13, 13);
            this.TimeLabel.TabIndex = 9;
            this.TimeLabel.Text = "0";
            // 
            // PathDistanceLabel
            // 
            this.PathDistanceLabel.AutoSize = true;
            this.PathDistanceLabel.Location = new System.Drawing.Point(151, 410);
            this.PathDistanceLabel.Name = "PathDistanceLabel";
            this.PathDistanceLabel.Size = new System.Drawing.Size(13, 13);
            this.PathDistanceLabel.TabIndex = 10;
            this.PathDistanceLabel.Text = "0";
            // 
            // NumCollisionsLabel
            // 
            this.NumCollisionsLabel.AutoSize = true;
            this.NumCollisionsLabel.Location = new System.Drawing.Point(151, 433);
            this.NumCollisionsLabel.Name = "NumCollisionsLabel";
            this.NumCollisionsLabel.Size = new System.Drawing.Size(13, 13);
            this.NumCollisionsLabel.TabIndex = 11;
            this.NumCollisionsLabel.Text = "0";
            // 
            // AvgPassabilityLabel
            // 
            this.AvgPassabilityLabel.AutoSize = true;
            this.AvgPassabilityLabel.Location = new System.Drawing.Point(370, 433);
            this.AvgPassabilityLabel.Name = "AvgPassabilityLabel";
            this.AvgPassabilityLabel.Size = new System.Drawing.Size(13, 13);
            this.AvgPassabilityLabel.TabIndex = 17;
            this.AvgPassabilityLabel.Text = "0";
            // 
            // MaxSpeedLabel
            // 
            this.MaxSpeedLabel.AutoSize = true;
            this.MaxSpeedLabel.Location = new System.Drawing.Point(370, 410);
            this.MaxSpeedLabel.Name = "MaxSpeedLabel";
            this.MaxSpeedLabel.Size = new System.Drawing.Size(13, 13);
            this.MaxSpeedLabel.TabIndex = 16;
            this.MaxSpeedLabel.Text = "0";
            // 
            // AvgSpeedLabel
            // 
            this.AvgSpeedLabel.AutoSize = true;
            this.AvgSpeedLabel.Location = new System.Drawing.Point(370, 387);
            this.AvgSpeedLabel.Name = "AvgSpeedLabel";
            this.AvgSpeedLabel.Size = new System.Drawing.Size(13, 13);
            this.AvgSpeedLabel.TabIndex = 15;
            this.AvgSpeedLabel.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(231, 433);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(128, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "Средняя проходимость:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(231, 410);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(137, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Максимальная скорость:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(231, 387);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(103, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "Средняя скорость:";
            // 
            // LicsDataWindow
            // 
            this.ClientSize = new System.Drawing.Size(446, 464);
            this.ControlBox = false;
            this.Controls.Add(this.AvgPassabilityLabel);
            this.Controls.Add(this.MaxSpeedLabel);
            this.Controls.Add(this.AvgSpeedLabel);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.NumCollisionsLabel);
            this.Controls.Add(this.PathDistanceLabel);
            this.Controls.Add(this.TimeLabel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
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
