using System.Windows.Forms;

namespace AudioVisualizerDx
{
    partial class MainWindow
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
            this.components = new System.ComponentModel.Container();
            this.drawingPanel = new System.Windows.Forms.Panel();
            this.dataTimer = new System.Windows.Forms.Timer(this.components);
            this.drawingTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // drawingPanel
            // 
            this.drawingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawingPanel.Location = new System.Drawing.Point(0, 0);
            this.drawingPanel.Name = "drawingPanel";
            this.drawingPanel.Size = new System.Drawing.Size(1002, 664);
            this.drawingPanel.TabIndex = 0;
            // 
            // dataTimer
            // 
            this.dataTimer.Interval = 30;
            this.dataTimer.Tick += new System.EventHandler(this.DataTimer_Tick);
            // 
            // drawingTimer
            // 
            this.drawingTimer.Interval = 30;
            this.drawingTimer.Tick += new System.EventHandler(this.DrawingTimer_Tick);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1002, 664);
            this.Controls.Add(this.drawingPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Music Visualizer (DX)";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel drawingPanel;
        private System.Windows.Forms.Timer dataTimer;
        private System.Windows.Forms.Timer drawingTimer;
    }
}