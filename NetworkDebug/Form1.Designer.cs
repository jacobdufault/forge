namespace NetworkDebug {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TextPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TextName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TextIPToConnectTo = new System.Windows.Forms.TextBox();
            this.ButtonStartServer = new System.Windows.Forms.Button();
            this.ButtonConnect = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TextLocalIP = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TextConnectionInformation = new System.Windows.Forms.Label();
            this.LabelLog = new System.Windows.Forms.RichTextBox();
            this.ButtonSendAll = new System.Windows.Forms.Button();
            this.ButtonSendClients = new System.Windows.Forms.Button();
            this.ButtonSendServer = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.TextPassword);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.TextName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TextIPToConnectTo);
            this.groupBox1.Controls.Add(this.ButtonStartServer);
            this.groupBox1.Controls.Add(this.ButtonConnect);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(205, 199);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Initialization";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Password";
            // 
            // TextPassword
            // 
            this.TextPassword.Location = new System.Drawing.Point(64, 46);
            this.TextPassword.Name = "TextPassword";
            this.TextPassword.Size = new System.Drawing.Size(135, 20);
            this.TextPassword.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Name";
            // 
            // TextName
            // 
            this.TextName.Location = new System.Drawing.Point(64, 20);
            this.TextName.Name = "TextName";
            this.TextName.Size = new System.Drawing.Size(135, 20);
            this.TextName.TabIndex = 5;
            this.TextName.Text = "myname";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "IP";
            // 
            // TextIPToConnectTo
            // 
            this.TextIPToConnectTo.Location = new System.Drawing.Point(64, 141);
            this.TextIPToConnectTo.Name = "TextIPToConnectTo";
            this.TextIPToConnectTo.Size = new System.Drawing.Size(135, 20);
            this.TextIPToConnectTo.TabIndex = 3;
            this.TextIPToConnectTo.Text = "127.0.0.1";
            // 
            // ButtonStartServer
            // 
            this.ButtonStartServer.Location = new System.Drawing.Point(6, 91);
            this.ButtonStartServer.Name = "ButtonStartServer";
            this.ButtonStartServer.Size = new System.Drawing.Size(193, 23);
            this.ButtonStartServer.TabIndex = 2;
            this.ButtonStartServer.Text = "Start server";
            this.ButtonStartServer.UseVisualStyleBackColor = true;
            this.ButtonStartServer.Click += new System.EventHandler(this.ButtonStartServer_Click);
            // 
            // ButtonConnect
            // 
            this.ButtonConnect.Location = new System.Drawing.Point(6, 167);
            this.ButtonConnect.Name = "ButtonConnect";
            this.ButtonConnect.Size = new System.Drawing.Size(193, 23);
            this.ButtonConnect.TabIndex = 1;
            this.ButtonConnect.Text = "Connect to server";
            this.ButtonConnect.UseVisualStyleBackColor = true;
            this.ButtonConnect.Click += new System.EventHandler(this.ButtonConnect_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TextLocalIP);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.TextConnectionInformation);
            this.groupBox2.Location = new System.Drawing.Point(254, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(364, 142);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Information";
            // 
            // TextLocalIP
            // 
            this.TextLocalIP.AutoSize = true;
            this.TextLocalIP.Location = new System.Drawing.Point(62, 123);
            this.TextLocalIP.Name = "TextLocalIP";
            this.TextLocalIP.Size = new System.Drawing.Size(0, 13);
            this.TextLocalIP.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Local IP";
            // 
            // TextConnectionInformation
            // 
            this.TextConnectionInformation.AutoSize = true;
            this.TextConnectionInformation.Location = new System.Drawing.Point(7, 20);
            this.TextConnectionInformation.Name = "TextConnectionInformation";
            this.TextConnectionInformation.Size = new System.Drawing.Size(151, 13);
            this.TextConnectionInformation.TabIndex = 0;
            this.TextConnectionInformation.Text = "dynamically generated content";
            // 
            // LabelLog
            // 
            this.LabelLog.Location = new System.Drawing.Point(257, 186);
            this.LabelLog.Name = "LabelLog";
            this.LabelLog.Size = new System.Drawing.Size(361, 186);
            this.LabelLog.TabIndex = 4;
            this.LabelLog.Text = "";
            // 
            // ButtonSendAll
            // 
            this.ButtonSendAll.Location = new System.Drawing.Point(13, 18);
            this.ButtonSendAll.Name = "ButtonSendAll";
            this.ButtonSendAll.Size = new System.Drawing.Size(186, 23);
            this.ButtonSendAll.TabIndex = 5;
            this.ButtonSendAll.Text = "Send to all";
            this.ButtonSendAll.UseVisualStyleBackColor = true;
            this.ButtonSendAll.Click += new System.EventHandler(this.ButtonSendAll_Click);
            // 
            // ButtonSendClients
            // 
            this.ButtonSendClients.Location = new System.Drawing.Point(13, 47);
            this.ButtonSendClients.Name = "ButtonSendClients";
            this.ButtonSendClients.Size = new System.Drawing.Size(186, 23);
            this.ButtonSendClients.TabIndex = 6;
            this.ButtonSendClients.Text = "Send to clients";
            this.ButtonSendClients.UseVisualStyleBackColor = true;
            this.ButtonSendClients.Click += new System.EventHandler(this.ButtonSendClients_Click);
            // 
            // ButtonSendServer
            // 
            this.ButtonSendServer.Location = new System.Drawing.Point(13, 76);
            this.ButtonSendServer.Name = "ButtonSendServer";
            this.ButtonSendServer.Size = new System.Drawing.Size(186, 23);
            this.ButtonSendServer.TabIndex = 7;
            this.ButtonSendServer.Text = "Send to server";
            this.ButtonSendServer.UseVisualStyleBackColor = true;
            this.ButtonSendServer.Click += new System.EventHandler(this.ButtonSendServer_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ButtonSendClients);
            this.groupBox3.Controls.Add(this.ButtonSendServer);
            this.groupBox3.Controls.Add(this.ButtonSendAll);
            this.groupBox3.Location = new System.Drawing.Point(12, 266);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(205, 106);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Send a message";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 384);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.LabelLog);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ButtonConnect;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label TextConnectionInformation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TextIPToConnectTo;
        private System.Windows.Forms.Button ButtonStartServer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TextPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TextName;
        private System.Windows.Forms.Label TextLocalIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox LabelLog;
        private System.Windows.Forms.Button ButtonSendAll;
        private System.Windows.Forms.Button ButtonSendClients;
        private System.Windows.Forms.Button ButtonSendServer;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}

