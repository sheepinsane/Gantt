
namespace ERP.PM
{
    partial class frmMain
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.基本資料管理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFactory = new System.Windows.Forms.ToolStripMenuItem();
            this.機台資訊ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.工單基本設定ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.工單圖表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新增工單ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.工單管理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.修改ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chart1 = new Braincase.GanttChart.Chart();
            this.panMain = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.基本資料管理ToolStripMenuItem,
            this.工單管理ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 63);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1205, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 基本資料管理ToolStripMenuItem
            // 
            this.基本資料管理ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFactory,
            this.機台資訊ToolStripMenuItem,
            this.工單基本設定ToolStripMenuItem});
            this.基本資料管理ToolStripMenuItem.Name = "基本資料管理ToolStripMenuItem";
            this.基本資料管理ToolStripMenuItem.Size = new System.Drawing.Size(91, 20);
            this.基本資料管理ToolStripMenuItem.Text = "基本資料管理";
            // 
            // menuFactory
            // 
            this.menuFactory.Name = "menuFactory";
            this.menuFactory.Size = new System.Drawing.Size(180, 22);
            this.menuFactory.Text = "廠商資訊";
            this.menuFactory.Click += new System.EventHandler(this.menuFactory_Click);
            // 
            // 機台資訊ToolStripMenuItem
            // 
            this.機台資訊ToolStripMenuItem.Name = "機台資訊ToolStripMenuItem";
            this.機台資訊ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.機台資訊ToolStripMenuItem.Text = "機台資訊";
            // 
            // 工單基本設定ToolStripMenuItem
            // 
            this.工單基本設定ToolStripMenuItem.Name = "工單基本設定ToolStripMenuItem";
            this.工單基本設定ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.工單基本設定ToolStripMenuItem.Text = "工單基本設定";
            // 
            // 工單圖表ToolStripMenuItem
            // 
            this.工單圖表ToolStripMenuItem.Name = "工單圖表ToolStripMenuItem";
            this.工單圖表ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.工單圖表ToolStripMenuItem.Text = "圖表";
            // 
            // 新增工單ToolStripMenuItem
            // 
            this.新增工單ToolStripMenuItem.Name = "新增工單ToolStripMenuItem";
            this.新增工單ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.新增工單ToolStripMenuItem.Text = "新增";
            // 
            // 工單管理ToolStripMenuItem
            // 
            this.工單管理ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.工單圖表ToolStripMenuItem,
            this.新增工單ToolStripMenuItem,
            this.修改ToolStripMenuItem});
            this.工單管理ToolStripMenuItem.Name = "工單管理ToolStripMenuItem";
            this.工單管理ToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.工單管理ToolStripMenuItem.Text = "工單管理";
            // 
            // 修改ToolStripMenuItem
            // 
            this.修改ToolStripMenuItem.Name = "修改ToolStripMenuItem";
            this.修改ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.修改ToolStripMenuItem.Text = "修改";
            // 
            // chart1
            // 
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart1.Location = new System.Drawing.Point(0, 87);
            this.chart1.Margin = new System.Windows.Forms.Padding(0);
            this.chart1.Name = "chart1";
            this.chart1.Size = new System.Drawing.Size(1205, 484);
            this.chart1.TabIndex = 1;
            // 
            // panMain
            // 
            this.panMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panMain.Location = new System.Drawing.Point(0, 87);
            this.panMain.Name = "panMain";
            this.panMain.Size = new System.Drawing.Size(1205, 484);
            this.panMain.TabIndex = 2;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1205, 571);
            this.Controls.Add(this.panMain);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Padding = new System.Windows.Forms.Padding(0, 63, 0, 0);
            this.Text = "ERP.PM";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 基本資料管理ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuFactory;
        private System.Windows.Forms.ToolStripMenuItem 機台資訊ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 工單基本設定ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 工單管理ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 工單圖表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 新增工單ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 修改ToolStripMenuItem;
        private Braincase.GanttChart.Chart chart1;
        private System.Windows.Forms.Panel panMain;
    }
}

