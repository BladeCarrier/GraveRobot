namespace Car.builders
{
    partial class RemoverBuilder
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
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.RemLastBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // RemLastBtn
            // 
            this.RemLastBtn.Location = new System.Drawing.Point(25, 38);
            this.RemLastBtn.Name = "RemLastBtn";
            this.RemLastBtn.Size = new System.Drawing.Size(253, 23);
            this.RemLastBtn.TabIndex = 0;
            this.RemLastBtn.Text = "Удалить последний добавленный объект";
            this.RemLastBtn.UseVisualStyleBackColor = true;
            this.RemLastBtn.Click += new System.EventHandler(this.RemLastBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(279, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Для просмотра изменений наведите на карту курсор";
            // 
            // RemoverBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 73);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RemLastBtn);
            this.Name = "RemoverBuilder";
            this.Text = "Удалятель";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button RemLastBtn;
        private System.Windows.Forms.Label label1;
    }
}