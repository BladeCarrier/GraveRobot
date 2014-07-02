namespace Car
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.showTrajectoryBox = new System.Windows.Forms.CheckBox();
            this.overlayGroup = new System.Windows.Forms.GroupBox();
            this.camDebugBox = new System.Windows.Forms.CheckBox();
            this.showGeometryBox = new System.Windows.Forms.CheckBox();
            this.showChangedWeightsBox = new System.Windows.Forms.CheckBox();
            this.pictureBoxOverlay = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.Builders = new System.Windows.Forms.GroupBox();
            this.saveloadRadio = new System.Windows.Forms.RadioButton();
            this.rposeRadio = new System.Windows.Forms.RadioButton();
            this.ObstBuilderRadio = new System.Windows.Forms.RadioButton();
            this.RemRadio = new System.Windows.Forms.RadioButton();
            this.ColBuilderRadio = new System.Windows.Forms.RadioButton();
            this.SwitchLearning = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.overlayGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOverlay)).BeginInit();
            this.Builders.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(800, 500);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 30;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Location = new System.Drawing.Point(831, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(100, 100);
            this.pictureBox2.TabIndex = 2;
            this.pictureBox2.TabStop = false;
            // 
            // showTrajectoryBox
            // 
            this.showTrajectoryBox.AutoSize = true;
            this.showTrajectoryBox.Checked = true;
            this.showTrajectoryBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showTrajectoryBox.Location = new System.Drawing.Point(6, 19);
            this.showTrajectoryBox.Name = "showTrajectoryBox";
            this.showTrajectoryBox.Size = new System.Drawing.Size(86, 17);
            this.showTrajectoryBox.TabIndex = 3;
            this.showTrajectoryBox.Text = "Траектория";
            this.showTrajectoryBox.UseVisualStyleBackColor = true;
            // 
            // overlayGroup
            // 
            this.overlayGroup.Controls.Add(this.camDebugBox);
            this.overlayGroup.Controls.Add(this.showGeometryBox);
            this.overlayGroup.Controls.Add(this.showChangedWeightsBox);
            this.overlayGroup.Controls.Add(this.showTrajectoryBox);
            this.overlayGroup.Location = new System.Drawing.Point(806, 133);
            this.overlayGroup.Name = "overlayGroup";
            this.overlayGroup.Size = new System.Drawing.Size(154, 108);
            this.overlayGroup.TabIndex = 4;
            this.overlayGroup.TabStop = false;
            this.overlayGroup.Text = "Overlay";
            // 
            // camDebugBox
            // 
            this.camDebugBox.AutoSize = true;
            this.camDebugBox.Location = new System.Drawing.Point(6, 76);
            this.camDebugBox.Name = "camDebugBox";
            this.camDebugBox.Size = new System.Drawing.Size(112, 17);
            this.camDebugBox.TabIndex = 6;
            this.camDebugBox.Text = "Отладка камеры";
            this.camDebugBox.UseVisualStyleBackColor = true;
            // 
            // showGeometryBox
            // 
            this.showGeometryBox.AutoSize = true;
            this.showGeometryBox.Checked = true;
            this.showGeometryBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showGeometryBox.Location = new System.Drawing.Point(6, 57);
            this.showGeometryBox.Name = "showGeometryBox";
            this.showGeometryBox.Size = new System.Drawing.Size(81, 17);
            this.showGeometryBox.TabIndex = 5;
            this.showGeometryBox.Text = "Геометрия";
            this.showGeometryBox.UseVisualStyleBackColor = true;
            // 
            // showChangedWeightsBox
            // 
            this.showChangedWeightsBox.AutoSize = true;
            this.showChangedWeightsBox.Checked = true;
            this.showChangedWeightsBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showChangedWeightsBox.Location = new System.Drawing.Point(6, 38);
            this.showChangedWeightsBox.Name = "showChangedWeightsBox";
            this.showChangedWeightsBox.Size = new System.Drawing.Size(119, 17);
            this.showChangedWeightsBox.TabIndex = 4;
            this.showChangedWeightsBox.Text = "Изменённые веса";
            this.showChangedWeightsBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.showChangedWeightsBox.UseVisualStyleBackColor = true;
            // 
            // pictureBoxOverlay
            // 
            this.pictureBoxOverlay.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxOverlay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxOverlay.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxOverlay.Name = "pictureBoxOverlay";
            this.pictureBoxOverlay.Size = new System.Drawing.Size(800, 500);
            this.pictureBoxOverlay.TabIndex = 5;
            this.pictureBoxOverlay.TabStop = false;
            this.pictureBoxOverlay.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxOverlay_MouseClick);
            this.pictureBoxOverlay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxOverlay_MouseMove);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(844, 299);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Старт";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Builders
            // 
            this.Builders.Controls.Add(this.saveloadRadio);
            this.Builders.Controls.Add(this.rposeRadio);
            this.Builders.Controls.Add(this.ObstBuilderRadio);
            this.Builders.Controls.Add(this.RemRadio);
            this.Builders.Controls.Add(this.ColBuilderRadio);
            this.Builders.Location = new System.Drawing.Point(813, 339);
            this.Builders.Name = "Builders";
            this.Builders.Size = new System.Drawing.Size(146, 135);
            this.Builders.TabIndex = 9;
            this.Builders.TabStop = false;
            this.Builders.Text = "Builders";
            // 
            // saveloadRadio
            // 
            this.saveloadRadio.AutoSize = true;
            this.saveloadRadio.Location = new System.Drawing.Point(25, 89);
            this.saveloadRadio.Name = "saveloadRadio";
            this.saveloadRadio.Size = new System.Drawing.Size(106, 17);
            this.saveloadRadio.TabIndex = 4;
            this.saveloadRadio.TabStop = true;
            this.saveloadRadio.Text = "SaveLoadBuilder";
            this.saveloadRadio.UseVisualStyleBackColor = true;
            this.saveloadRadio.Click += new System.EventHandler(this.saveloadRadio_Click);
            // 
            // rposeRadio
            // 
            this.rposeRadio.AutoSize = true;
            this.rposeRadio.Location = new System.Drawing.Point(25, 67);
            this.rposeRadio.Name = "rposeRadio";
            this.rposeRadio.Size = new System.Drawing.Size(81, 17);
            this.rposeRadio.TabIndex = 3;
            this.rposeRadio.TabStop = true;
            this.rposeRadio.Text = "RobotPoser";
            this.rposeRadio.UseVisualStyleBackColor = true;
            this.rposeRadio.Click += new System.EventHandler(this.rposeRadio_Click);
            // 
            // ObstBuilderRadio
            // 
            this.ObstBuilderRadio.AutoSize = true;
            this.ObstBuilderRadio.Location = new System.Drawing.Point(26, 44);
            this.ObstBuilderRadio.Name = "ObstBuilderRadio";
            this.ObstBuilderRadio.Size = new System.Drawing.Size(99, 17);
            this.ObstBuilderRadio.TabIndex = 2;
            this.ObstBuilderRadio.TabStop = true;
            this.ObstBuilderRadio.Text = "ObstacleBuilder";
            this.ObstBuilderRadio.UseVisualStyleBackColor = true;
            this.ObstBuilderRadio.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ObstBuilderRadio_MouseClick);
            // 
            // RemRadio
            // 
            this.RemRadio.AutoSize = true;
            this.RemRadio.Location = new System.Drawing.Point(25, 112);
            this.RemRadio.Name = "RemRadio";
            this.RemRadio.Size = new System.Drawing.Size(68, 17);
            this.RemRadio.TabIndex = 1;
            this.RemRadio.TabStop = true;
            this.RemRadio.Text = "Remover";
            this.RemRadio.UseVisualStyleBackColor = true;
            this.RemRadio.Click += new System.EventHandler(this.RemRadio_Click);
            // 
            // ColBuilderRadio
            // 
            this.ColBuilderRadio.AutoSize = true;
            this.ColBuilderRadio.Location = new System.Drawing.Point(26, 21);
            this.ColBuilderRadio.Name = "ColBuilderRadio";
            this.ColBuilderRadio.Size = new System.Drawing.Size(92, 17);
            this.ColBuilderRadio.TabIndex = 0;
            this.ColBuilderRadio.TabStop = true;
            this.ColBuilderRadio.Text = "ColumnBuilder";
            this.ColBuilderRadio.UseVisualStyleBackColor = true;
            this.ColBuilderRadio.Click += new System.EventHandler(this.ColBuilderRadio_Click);
            // 
            // SwitchLearning
            // 
            this.SwitchLearning.AutoSize = true;
            this.SwitchLearning.Checked = true;
            this.SwitchLearning.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SwitchLearning.Location = new System.Drawing.Point(812, 266);
            this.SwitchLearning.Name = "SwitchLearning";
            this.SwitchLearning.Size = new System.Drawing.Size(150, 17);
            this.SwitchLearning.TabIndex = 10;
            this.SwitchLearning.Text = "Включить самообучение";
            this.SwitchLearning.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 501);
            this.Controls.Add(this.SwitchLearning);
            this.Controls.Add(this.Builders);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBoxOverlay);
            this.Controls.Add(this.overlayGroup);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Могильный робот";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.overlayGroup.ResumeLayout(false);
            this.overlayGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOverlay)).EndInit();
            this.Builders.ResumeLayout(false);
            this.Builders.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.CheckBox showTrajectoryBox;
        private System.Windows.Forms.GroupBox overlayGroup;
        private System.Windows.Forms.CheckBox showGeometryBox;
        private System.Windows.Forms.CheckBox showChangedWeightsBox;
        private System.Windows.Forms.PictureBox pictureBoxOverlay;
        private System.Windows.Forms.CheckBox camDebugBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox Builders;
        private System.Windows.Forms.RadioButton ColBuilderRadio;
        private System.Windows.Forms.RadioButton RemRadio;
        private System.Windows.Forms.RadioButton ObstBuilderRadio;
        private System.Windows.Forms.RadioButton rposeRadio;
        private System.Windows.Forms.CheckBox SwitchLearning;
        private System.Windows.Forms.RadioButton saveloadRadio;
    }
}

