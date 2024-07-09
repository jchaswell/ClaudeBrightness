namespace ClaudeBrightness
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
            this.trackBarBrightness = new System.Windows.Forms.TrackBar();
            this.labelBrightness = new System.Windows.Forms.Label();
            this.buttonApply = new System.Windows.Forms.Button();
            this.checkBoxOverrideSchedule = new System.Windows.Forms.CheckBox();
            this.OpenScheduleFile = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBrightness)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBarBrightness
            // 
            this.trackBarBrightness.Location = new System.Drawing.Point(38, 182);
            this.trackBarBrightness.Maximum = 100;
            this.trackBarBrightness.Name = "trackBarBrightness";
            this.trackBarBrightness.Size = new System.Drawing.Size(245, 45);
            this.trackBarBrightness.TabIndex = 0;
            this.trackBarBrightness.TickFrequency = 5;
            this.trackBarBrightness.Scroll += new System.EventHandler(this.trackBarBrightness_Scroll);
            // 
            // labelBrightness
            // 
            this.labelBrightness.AutoSize = true;
            this.labelBrightness.Location = new System.Drawing.Point(35, 166);
            this.labelBrightness.Name = "labelBrightness";
            this.labelBrightness.Size = new System.Drawing.Size(85, 13);
            this.labelBrightness.TabIndex = 1;
            this.labelBrightness.Text = "Brightness:  50%";
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(289, 182);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 2;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            // 
            // checkBoxOverrideSchedule
            // 
            this.checkBoxOverrideSchedule.AutoSize = true;
            this.checkBoxOverrideSchedule.Location = new System.Drawing.Point(398, 182);
            this.checkBoxOverrideSchedule.Name = "checkBoxOverrideSchedule";
            this.checkBoxOverrideSchedule.Size = new System.Drawing.Size(120, 17);
            this.checkBoxOverrideSchedule.TabIndex = 3;
            this.checkBoxOverrideSchedule.Text = "Override Schedule?";
            this.checkBoxOverrideSchedule.UseVisualStyleBackColor = true;
            // 
            // OpenScheduleFile
            // 
            this.OpenScheduleFile.Location = new System.Drawing.Point(38, 259);
            this.OpenScheduleFile.Name = "OpenScheduleFile";
            this.OpenScheduleFile.Size = new System.Drawing.Size(191, 23);
            this.OpenScheduleFile.TabIndex = 4;
            this.OpenScheduleFile.Text = "Open Schedule File";
            this.OpenScheduleFile.UseVisualStyleBackColor = true;
            this.OpenScheduleFile.Click += new System.EventHandler(this.OpenScheduleFile_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.OpenScheduleFile);
            this.Controls.Add(this.checkBoxOverrideSchedule);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.labelBrightness);
            this.Controls.Add(this.trackBarBrightness);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBrightness)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar trackBarBrightness;
        private System.Windows.Forms.Label labelBrightness;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.CheckBox checkBoxOverrideSchedule;
        private System.Windows.Forms.Button OpenScheduleFile;
    }
}

