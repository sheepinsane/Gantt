using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ERP.FORM
{
    public partial class frmCustomerEdit : TitleDialog
    {
        public Customer modCustomer { get; set; }
        public frmCustomerEdit()
        {
            InitializeComponent();
            if (modCustomer != null)
                this.Text = $"資料修改:{modCustomer.CustomerName}";
            else
                this.Text = $"資料新增";
            
        }
    }
}
