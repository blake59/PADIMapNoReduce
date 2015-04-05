namespace PuppetMaster
{
    partial class PuppetMasterForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.puppetMasterTB = new System.Windows.Forms.TextBox();
            this.puppetMasterUrlBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.commandTB = new System.Windows.Forms.TextBox();
            this.submitBtn = new System.Windows.Forms.Button();
            this.loadScriptBtn = new System.Windows.Forms.Button();
            this.commandsLbl = new System.Windows.Forms.Label();
            this.infoLbl = new System.Windows.Forms.Label();
            this.infoLB = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "PuppetMasterURL";
            // 
            // puppetMasterTB
            // 
            this.puppetMasterTB.Location = new System.Drawing.Point(104, 47);
            this.puppetMasterTB.Name = "puppetMasterTB";
            this.puppetMasterTB.Size = new System.Drawing.Size(212, 20);
            this.puppetMasterTB.TabIndex = 1;
            this.puppetMasterTB.TextChanged += new System.EventHandler(this.puppetMasterTB_TextChanged);
            // 
            // puppetMasterUrlBtn
            // 
            this.puppetMasterUrlBtn.Enabled = false;
            this.puppetMasterUrlBtn.Location = new System.Drawing.Point(322, 45);
            this.puppetMasterUrlBtn.Name = "puppetMasterUrlBtn";
            this.puppetMasterUrlBtn.Size = new System.Drawing.Size(53, 23);
            this.puppetMasterUrlBtn.TabIndex = 2;
            this.puppetMasterUrlBtn.Text = "Apply";
            this.puppetMasterUrlBtn.UseVisualStyleBackColor = true;
            this.puppetMasterUrlBtn.Click += new System.EventHandler(this.puppetMasterUrlBtn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(44, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Command";
            // 
            // commandTB
            // 
            this.commandTB.Location = new System.Drawing.Point(104, 112);
            this.commandTB.Name = "commandTB";
            this.commandTB.Size = new System.Drawing.Size(212, 20);
            this.commandTB.TabIndex = 4;
            // 
            // submitBtn
            // 
            this.submitBtn.Location = new System.Drawing.Point(322, 109);
            this.submitBtn.Name = "submitBtn";
            this.submitBtn.Size = new System.Drawing.Size(53, 23);
            this.submitBtn.TabIndex = 5;
            this.submitBtn.Text = "Submit";
            this.submitBtn.UseVisualStyleBackColor = true;
            this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
            // 
            // loadScriptBtn
            // 
            this.loadScriptBtn.Location = new System.Drawing.Point(104, 138);
            this.loadScriptBtn.Name = "loadScriptBtn";
            this.loadScriptBtn.Size = new System.Drawing.Size(212, 23);
            this.loadScriptBtn.TabIndex = 6;
            this.loadScriptBtn.Text = "Load Script";
            this.loadScriptBtn.UseVisualStyleBackColor = true;
            this.loadScriptBtn.Click += new System.EventHandler(this.loadScriptBtn_Click);
            // 
            // commandsLbl
            // 
            this.commandsLbl.AutoSize = true;
            this.commandsLbl.BackColor = System.Drawing.SystemColors.Highlight;
            this.commandsLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.commandsLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.commandsLbl.Location = new System.Drawing.Point(144, 89);
            this.commandsLbl.Name = "commandsLbl";
            this.commandsLbl.Size = new System.Drawing.Size(96, 20);
            this.commandsLbl.TabIndex = 7;
            this.commandsLbl.Text = "Commands";
            // 
            // infoLbl
            // 
            this.infoLbl.BackColor = System.Drawing.SystemColors.Highlight;
            this.infoLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.infoLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.infoLbl.Location = new System.Drawing.Point(144, 181);
            this.infoLbl.Name = "infoLbl";
            this.infoLbl.Size = new System.Drawing.Size(96, 20);
            this.infoLbl.TabIndex = 8;
            this.infoLbl.Text = "Info";
            this.infoLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // infoLB
            // 
            this.infoLB.FormattingEnabled = true;
            this.infoLB.Location = new System.Drawing.Point(12, 204);
            this.infoLB.Name = "infoLB";
            this.infoLB.Size = new System.Drawing.Size(363, 82);
            this.infoLB.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.SystemColors.Highlight;
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(144, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 20);
            this.label3.TabIndex = 10;
            this.label3.Text = "General";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PuppetMasterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 298);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.infoLB);
            this.Controls.Add(this.infoLbl);
            this.Controls.Add(this.commandsLbl);
            this.Controls.Add(this.loadScriptBtn);
            this.Controls.Add(this.submitBtn);
            this.Controls.Add(this.commandTB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.puppetMasterUrlBtn);
            this.Controls.Add(this.puppetMasterTB);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "PuppetMasterForm";
            this.Text = "PuppetMaster";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox puppetMasterTB;
        private System.Windows.Forms.Button puppetMasterUrlBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox commandTB;
        private System.Windows.Forms.Button submitBtn;
        private System.Windows.Forms.Button loadScriptBtn;
        private System.Windows.Forms.Label commandsLbl;
        private System.Windows.Forms.Label infoLbl;
        private System.Windows.Forms.ListBox infoLB;
        private System.Windows.Forms.Label label3;
    }
}

