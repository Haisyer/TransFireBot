using SysBot.Pokemon.WinForms.Properties;

namespace SysBot.Pokemon.WinForms
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            TC_Main = new System.Windows.Forms.TabControl();
            Tab_Bots = new System.Windows.Forms.TabPage();
            CB_Protocol = new System.Windows.Forms.ComboBox();
            FLP_Bots = new System.Windows.Forms.FlowLayoutPanel();
            TB_IP = new System.Windows.Forms.TextBox();
            CB_Routine = new System.Windows.Forms.ComboBox();
            NUD_Port = new System.Windows.Forms.NumericUpDown();
            B_New = new System.Windows.Forms.Button();
            Tab_Hub = new System.Windows.Forms.TabPage();
            PG_Hub = new System.Windows.Forms.PropertyGrid();
            Tab_Logs = new System.Windows.Forms.TabPage();
            RTB_Logs = new System.Windows.Forms.RichTextBox();
            B_Stop = new System.Windows.Forms.Button();
            B_Start = new System.Windows.Forms.Button();
            notifyIcon1 = new System.Windows.Forms.NotifyIcon(components);
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            TC_Main.SuspendLayout();
            Tab_Bots.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NUD_Port).BeginInit();
            Tab_Hub.SuspendLayout();
            Tab_Logs.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // TC_Main
            // 
            TC_Main.Controls.Add(Tab_Bots);
            TC_Main.Controls.Add(Tab_Hub);
            TC_Main.Controls.Add(Tab_Logs);
            TC_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            TC_Main.Location = new System.Drawing.Point(0, 0);
            TC_Main.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            TC_Main.Name = "TC_Main";
            TC_Main.SelectedIndex = 0;
            TC_Main.Size = new System.Drawing.Size(685, 475);
            TC_Main.TabIndex = 3;
            // 
            // Tab_Bots
            // 
            Tab_Bots.Controls.Add(CB_Protocol);
            Tab_Bots.Controls.Add(FLP_Bots);
            Tab_Bots.Controls.Add(TB_IP);
            Tab_Bots.Controls.Add(CB_Routine);
            Tab_Bots.Controls.Add(NUD_Port);
            Tab_Bots.Controls.Add(B_New);
            Tab_Bots.Location = new System.Drawing.Point(4, 29);
            Tab_Bots.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            Tab_Bots.Name = "Tab_Bots";
            Tab_Bots.Size = new System.Drawing.Size(677, 442);
            Tab_Bots.TabIndex = 0;
            Tab_Bots.Text = "机器人";
            Tab_Bots.UseVisualStyleBackColor = true;
            // 
            // CB_Protocol
            // 
            CB_Protocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CB_Protocol.FormattingEnabled = true;
            CB_Protocol.Location = new System.Drawing.Point(373, 7);
            CB_Protocol.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            CB_Protocol.Name = "CB_Protocol";
            CB_Protocol.Size = new System.Drawing.Size(85, 28);
            CB_Protocol.TabIndex = 10;
            CB_Protocol.SelectedIndexChanged += CB_Protocol_SelectedIndexChanged;
            // 
            // FLP_Bots
            // 
            FLP_Bots.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            FLP_Bots.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            FLP_Bots.Location = new System.Drawing.Point(0, 48);
            FLP_Bots.Margin = new System.Windows.Forms.Padding(0);
            FLP_Bots.Name = "FLP_Bots";
            FLP_Bots.Size = new System.Drawing.Size(673, 384);
            FLP_Bots.TabIndex = 9;
            FLP_Bots.Resize += FLP_Bots_Resize;
            // 
            // TB_IP
            // 
            TB_IP.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            TB_IP.Location = new System.Drawing.Point(95, 8);
            TB_IP.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            TB_IP.Name = "TB_IP";
            TB_IP.Size = new System.Drawing.Size(171, 23);
            TB_IP.TabIndex = 8;
            TB_IP.Text = "192.168.0.1";
            // 
            // CB_Routine
            // 
            CB_Routine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CB_Routine.FormattingEnabled = true;
            CB_Routine.Location = new System.Drawing.Point(468, 7);
            CB_Routine.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            CB_Routine.Name = "CB_Routine";
            CB_Routine.Size = new System.Drawing.Size(149, 28);
            CB_Routine.TabIndex = 7;
            // 
            // NUD_Port
            // 
            NUD_Port.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            NUD_Port.Location = new System.Drawing.Point(275, 8);
            NUD_Port.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            NUD_Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NUD_Port.Name = "NUD_Port";
            NUD_Port.Size = new System.Drawing.Size(86, 23);
            NUD_Port.TabIndex = 6;
            NUD_Port.Value = new decimal(new int[] { 6000, 0, 0, 0 });
            // 
            // B_New
            // 
            B_New.Location = new System.Drawing.Point(5, 8);
            B_New.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            B_New.Name = "B_New";
            B_New.Size = new System.Drawing.Size(81, 32);
            B_New.TabIndex = 0;
            B_New.Text = "添加";
            B_New.UseVisualStyleBackColor = true;
            B_New.Click += B_New_Click;
            // 
            // Tab_Hub
            // 
            Tab_Hub.Controls.Add(PG_Hub);
            Tab_Hub.Location = new System.Drawing.Point(4, 29);
            Tab_Hub.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            Tab_Hub.Name = "Tab_Hub";
            Tab_Hub.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            Tab_Hub.Size = new System.Drawing.Size(677, 442);
            Tab_Hub.TabIndex = 2;
            Tab_Hub.Text = "菜单";
            Tab_Hub.UseVisualStyleBackColor = true;
            // 
            // PG_Hub
            // 
            PG_Hub.Dock = System.Windows.Forms.DockStyle.Fill;
            PG_Hub.Location = new System.Drawing.Point(5, 5);
            PG_Hub.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            PG_Hub.Name = "PG_Hub";
            PG_Hub.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            PG_Hub.Size = new System.Drawing.Size(667, 432);
            PG_Hub.TabIndex = 0;
            // 
            // Tab_Logs
            // 
            Tab_Logs.Controls.Add(RTB_Logs);
            Tab_Logs.Location = new System.Drawing.Point(4, 29);
            Tab_Logs.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            Tab_Logs.Name = "Tab_Logs";
            Tab_Logs.Size = new System.Drawing.Size(677, 442);
            Tab_Logs.TabIndex = 1;
            Tab_Logs.Text = "日志";
            Tab_Logs.UseVisualStyleBackColor = true;
            // 
            // RTB_Logs
            // 
            RTB_Logs.Dock = System.Windows.Forms.DockStyle.Fill;
            RTB_Logs.Location = new System.Drawing.Point(0, 0);
            RTB_Logs.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            RTB_Logs.Name = "RTB_Logs";
            RTB_Logs.ReadOnly = true;
            RTB_Logs.Size = new System.Drawing.Size(677, 442);
            RTB_Logs.TabIndex = 0;
            RTB_Logs.Text = "";
            // 
            // B_Stop
            // 
            B_Stop.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            B_Stop.Location = new System.Drawing.Point(539, 0);
            B_Stop.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            B_Stop.Name = "B_Stop";
            B_Stop.Size = new System.Drawing.Size(117, 32);
            B_Stop.TabIndex = 4;
            B_Stop.Text = "全部停止";
            B_Stop.UseVisualStyleBackColor = true;
            B_Stop.Click += B_Stop_Click;
            // 
            // B_Start
            // 
            B_Start.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            B_Start.Location = new System.Drawing.Point(413, 0);
            B_Start.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            B_Start.Name = "B_Start";
            B_Start.Size = new System.Drawing.Size(117, 32);
            B_Start.TabIndex = 3;
            B_Start.Text = "全部开始";
            B_Start.UseVisualStyleBackColor = true;
            B_Start.Click += B_Start_Click;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (System.Drawing.Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "King-Bot";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { exitToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(109, 28);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(108, 24);
            exitToolStripMenuItem.Text = "退出";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click_1;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(685, 475);
            Controls.Add(B_Stop);
            Controls.Add(B_Start);
            Controls.Add(TC_Main);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            Name = "Main";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "King-Bot";
            FormClosing += Main_FormClosing;
            TC_Main.ResumeLayout(false);
            Tab_Bots.ResumeLayout(false);
            Tab_Bots.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NUD_Port).EndInit();
            Tab_Hub.ResumeLayout(false);
            Tab_Logs.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TabControl TC_Main;
        private System.Windows.Forms.TabPage Tab_Bots;
        private System.Windows.Forms.TabPage Tab_Logs;
        private System.Windows.Forms.RichTextBox RTB_Logs;
        private System.Windows.Forms.TabPage Tab_Hub;
        private System.Windows.Forms.PropertyGrid PG_Hub;
        private System.Windows.Forms.Button B_Stop;
        private System.Windows.Forms.Button B_Start;
        private System.Windows.Forms.TextBox TB_IP;
        private System.Windows.Forms.ComboBox CB_Routine;
        private System.Windows.Forms.NumericUpDown NUD_Port;
        private System.Windows.Forms.Button B_New;
        private System.Windows.Forms.FlowLayoutPanel FLP_Bots;
        private System.Windows.Forms.ComboBox CB_Protocol;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

