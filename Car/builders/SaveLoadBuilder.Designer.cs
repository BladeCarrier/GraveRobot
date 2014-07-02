namespace Car.builders
{
    partial class SaveLoadBuilder
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
            this.saveBTN = new System.Windows.Forms.Button();
            this.loadBTN = new System.Windows.Forms.Button();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // saveBTN
            // 
            this.saveBTN.Location = new System.Drawing.Point(75, 33);
            this.saveBTN.Name = "saveBTN";
            this.saveBTN.Size = new System.Drawing.Size(92, 23);
            this.saveBTN.TabIndex = 0;
            this.saveBTN.Text = "Сохранить";
            this.saveBTN.UseVisualStyleBackColor = true;
            this.saveBTN.Click += new System.EventHandler(this.saveBTN_Click);
            // 
            // loadBTN
            // 
            this.loadBTN.Location = new System.Drawing.Point(173, 33);
            this.loadBTN.Name = "loadBTN";
            this.loadBTN.Size = new System.Drawing.Size(87, 23);
            this.loadBTN.TabIndex = 1;
            this.loadBTN.Text = "Загрузить";
            this.loadBTN.UseVisualStyleBackColor = true;
            this.loadBTN.Click += new System.EventHandler(this.loadBTN_Click);
            // 
            // nameBox
            // 
            this.nameBox.Location = new System.Drawing.Point(75, 7);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(185, 20);
            this.nameBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Название:";
            // 
            // SaveLoadBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 65);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.loadBTN);
            this.Controls.Add(this.saveBTN);
            this.Name = "SaveLoadBuilder";
            this.Text = "Сохранить/загрузить карту";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveBTN;
        private System.Windows.Forms.Button loadBTN;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.Label label1;

    }
}