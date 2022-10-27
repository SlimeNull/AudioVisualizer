namespace AudioVisualizer
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
            this.dataTimer = new System.Windows.Forms.Timer(this.components);
            this.drawingPanel = new System.Windows.Forms.Panel();
            this.drawingTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // dataTimer
            // 
            this.dataTimer.Interval = 30;
            this.dataTimer.Tick += new System.EventHandler(this.DataTimer_Tick);
            // 
            // drawingPanel
            // 
            this.drawingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawingPanel.Location = new System.Drawing.Point(0, 0);
            this.drawingPanel.Name = "drawingPanel";
            this.drawingPanel.Size = new System.Drawing.Size(800, 450);
            this.drawingPanel.TabIndex = 0;
            this.drawingPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.DrawingPanel_MouseDoubleClick);
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
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.drawingPanel);
            this.Name = "MainWindow";
            this.Text = "Music Visualizer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer dataTimer;
        private Panel drawingPanel;
        private System.Windows.Forms.Timer drawingTimer;
    }
}