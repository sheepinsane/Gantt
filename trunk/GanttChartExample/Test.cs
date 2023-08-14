using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Braincase.GanttChart
{
    public partial class Test : Form
    {
        ProjectManager _mManager = null;
        int spin = 14;
        public Test()
        {
            InitializeComponent();
            // start a new Project and init the chart with the project
            _mManager = new ProjectManager();
            var mTask1 = new Task() { Name = "New Task" };
            _mManager.Add(mTask1);
            _mManager.SetStart(mTask1, TimeSpan.FromDays(14));
            _mManager.SetComplete(mTask1, 0.7f);
            //_mManager.SetDuration(mTask1, TimeSpan.FromDays(2));
            _mManager.SetEnd(mTask1, TimeSpan.FromDays(17));

            _mManager.Start = DateTime.Now.AddDays(-14);
            _mManager.Now = TimeSpan.FromDays(14);

            _mChart.Init(_mManager);
            _mChart.Invalidate();
        }
    }
}
