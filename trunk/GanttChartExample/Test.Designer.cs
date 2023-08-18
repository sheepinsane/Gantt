
namespace Braincase.GanttChart
{
    partial class Test
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
            this._mChart = new Braincase.GanttChart.Chart();
            this.SuspendLayout();
            // 
            // _mChart
            // 
            this._mChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mChart.Location = new System.Drawing.Point(0, 0);
            this._mChart.Margin = new System.Windows.Forms.Padding(0);
            this._mChart.Name = "_mChart";
            this._mChart.Size = new System.Drawing.Size(939, 473);
            this._mChart.TabIndex = 0;
            // 
            // Test
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(939, 473);
            this.Controls.Add(this._mChart);
            this.Name = "Test";
            this.Text = "Test";
            this.ResumeLayout(false);

        }

        #endregion

        private Chart _mChart;
    }
}