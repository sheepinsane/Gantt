using System;
using System.Windows.Forms;
using MaterialSkin.Controls;
namespace ERP.FORM
{
    public partial class BaseDialog : Form
    {
        public BaseDialog()
        {
            // 設定共享的屬性
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }
    }
    public partial class TitleDialog : MaterialForm
    {
        public TitleDialog()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            // 設定共享的屬性
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }
    }
        
}
