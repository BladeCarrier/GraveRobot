namespace Car.builders
{
    partial class ObstacleBuilder
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
            this.button1 = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.SizeBValue = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SizeAValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.MassValue = new System.Windows.Forms.TextBox();
            this.radioDynNo = new System.Windows.Forms.RadioButton();
            this.radioDynYes = new System.Windows.Forms.RadioButton();
            this.GroupPass = new System.Windows.Forms.GroupBox();
            this.radioPassNo = new System.Windows.Forms.RadioButton();
            this.radioPassYes = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.PassValue = new System.Windows.Forms.TextBox();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.GroupPass.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(32, 95);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Выбрать цвет";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.SizeBValue);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.SizeAValue);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Location = new System.Drawing.Point(5, 209);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(183, 124);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Внешний вид";
            // 
            // SizeBValue
            // 
            this.SizeBValue.Location = new System.Drawing.Point(21, 69);
            this.SizeBValue.Name = "SizeBValue";
            this.SizeBValue.Size = new System.Drawing.Size(136, 20);
            this.SizeBValue.TabIndex = 5;
            this.SizeBValue.Text = "3";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Высота:";
            // 
            // SizeAValue
            // 
            this.SizeAValue.Location = new System.Drawing.Point(21, 35);
            this.SizeAValue.Name = "SizeAValue";
            this.SizeAValue.Size = new System.Drawing.Size(136, 20);
            this.SizeAValue.TabIndex = 0;
            this.SizeAValue.Text = "3";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Ширина:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.MassValue);
            this.groupBox2.Controls.Add(this.radioDynNo);
            this.groupBox2.Controls.Add(this.radioDynYes);
            this.groupBox2.Location = new System.Drawing.Point(5, 1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(183, 96);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Движимость";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Масса объекта:";
            // 
            // MassValue
            // 
            this.MassValue.Enabled = false;
            this.MassValue.Location = new System.Drawing.Point(21, 69);
            this.MassValue.Name = "MassValue";
            this.MassValue.Size = new System.Drawing.Size(136, 20);
            this.MassValue.TabIndex = 5;
            this.MassValue.Text = "3";
            // 
            // radioDynNo
            // 
            this.radioDynNo.AutoSize = true;
            this.radioDynNo.Checked = true;
            this.radioDynNo.Location = new System.Drawing.Point(113, 19);
            this.radioDynNo.Name = "radioDynNo";
            this.radioDynNo.Size = new System.Drawing.Size(44, 17);
            this.radioDynNo.TabIndex = 7;
            this.radioDynNo.TabStop = true;
            this.radioDynNo.Text = "Нет";
            this.radioDynNo.UseVisualStyleBackColor = true;
            // 
            // radioDynYes
            // 
            this.radioDynYes.AutoSize = true;
            this.radioDynYes.Location = new System.Drawing.Point(21, 19);
            this.radioDynYes.Name = "radioDynYes";
            this.radioDynYes.Size = new System.Drawing.Size(40, 17);
            this.radioDynYes.TabIndex = 7;
            this.radioDynYes.Text = "Да";
            this.radioDynYes.UseVisualStyleBackColor = true;
            this.radioDynYes.CheckedChanged += new System.EventHandler(this.radioDynYes_CheckedChanged);
            // 
            // GroupPass
            // 
            this.GroupPass.Controls.Add(this.radioPassNo);
            this.GroupPass.Controls.Add(this.radioPassYes);
            this.GroupPass.Controls.Add(this.label5);
            this.GroupPass.Controls.Add(this.PassValue);
            this.GroupPass.Location = new System.Drawing.Point(5, 103);
            this.GroupPass.Name = "GroupPass";
            this.GroupPass.Size = new System.Drawing.Size(183, 100);
            this.GroupPass.TabIndex = 8;
            this.GroupPass.TabStop = false;
            this.GroupPass.Text = "Проходимость";
            // 
            // radioPassNo
            // 
            this.radioPassNo.AutoSize = true;
            this.radioPassNo.Checked = true;
            this.radioPassNo.Location = new System.Drawing.Point(113, 19);
            this.radioPassNo.Name = "radioPassNo";
            this.radioPassNo.Size = new System.Drawing.Size(44, 17);
            this.radioPassNo.TabIndex = 1;
            this.radioPassNo.TabStop = true;
            this.radioPassNo.Text = "Нет";
            this.radioPassNo.UseVisualStyleBackColor = true;
            // 
            // radioPassYes
            // 
            this.radioPassYes.AutoSize = true;
            this.radioPassYes.Location = new System.Drawing.Point(21, 19);
            this.radioPassYes.Name = "radioPassYes";
            this.radioPassYes.Size = new System.Drawing.Size(40, 17);
            this.radioPassYes.TabIndex = 0;
            this.radioPassYes.Text = "Да";
            this.radioPassYes.UseVisualStyleBackColor = true;
            this.radioPassYes.CheckedChanged += new System.EventHandler(this.radioPassYes_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Степень проходимости:";
            // 
            // PassValue
            // 
            this.PassValue.Enabled = false;
            this.PassValue.Location = new System.Drawing.Point(21, 69);
            this.PassValue.Name = "PassValue";
            this.PassValue.Size = new System.Drawing.Size(136, 20);
            this.PassValue.TabIndex = 3;
            this.PassValue.Text = "3";
            // 
            // ObstacleBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(196, 345);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.GroupPass);
            this.Name = "ObstacleBuilder";
            this.Text = "Прямоугольник";
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.GroupPass.ResumeLayout(false);
            this.GroupPass.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox SizeAValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox MassValue;
        private System.Windows.Forms.RadioButton radioDynNo;
        private System.Windows.Forms.RadioButton radioDynYes;
        private System.Windows.Forms.GroupBox GroupPass;
        private System.Windows.Forms.RadioButton radioPassNo;
        private System.Windows.Forms.RadioButton radioPassYes;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox PassValue;
        private System.Windows.Forms.TextBox SizeBValue;
        private System.Windows.Forms.Label label1;
    }
}