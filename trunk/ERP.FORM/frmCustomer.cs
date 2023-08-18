using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
namespace ERP.FORM
{
    public partial class frmCustomer : BaseDialog
    {
        List<Customer> lstCustomer = new List<Customer>();
        public frmCustomer()
        {
            InitializeComponent();
            dgvCustomer.MakeDoubleBuffered(true);
            // dgvCustomer.AutoGenerateColumns = false;
        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {


        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //AddCustomer(new Customer
            //{
            //    CustomerID = 1,
            //    CustomerName = "Example Company 1",
            //    CompanyName = $"Example Corp {lstCustomer.Count()}",
            //    ContactName = "John Doe",
            //    ContactPhone = "123-456-7890",
            //    Email = "john@example.com",
            //    MainAddress = "123 Main St",
            //    ShippingAddress = "456 Shipping St",
            //    BillingAddress = "789 Billing St",
            //    PostalCode = "12345",
            //    City = "Cityville",
            //    Country = "Countryland",
            //    Currency = "USD",
            //    CreditLimit = 10000,
            //    AccountBalance = 5000,
            //    PaymentTerms = "Net 30",
            //    SalesRepresentative = "Jane Smith",
            //    SalesTeam = "Sales Team A",
            //    CustomerCategory = "Premium",
            //    CustomerLevel = "Gold",
            //    ContractExpirationDate = DateTime.Parse("2023-12-31"),
            //    ContractType = "Service",
            //    ContractDetails = "Service Agreement",
            //    Notes = "Important customer",
            //    AssociatedCustomers = "Subsidiary Company",
            //    Partnerships = "Strategic Partner",
            //    CommunicationRecords = "Phone calls, emails",
            //    MarketInfo = "Technology",
            //});
            //LoadData();

            frmCustomerEdit frm = new frmCustomerEdit();
            frm.ShowDialog();
         }
        private void LoadData() 
        {
            dgvCustomer.DataSource = null;
            dgvCustomer.DataSource =lstCustomer;
        }

        public void AddCustomer(Customer customer)
        {
            lstCustomer.Add(customer);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            frmCustomerSearch frm = new frmCustomerSearch();
            frm.ShowDialog();
        }
    }
    public static class ControlExtentions
    {
        public static void MakeDoubleBuffered(this Control control, bool setting)
        {
            Type controlType = control.GetType();
            PropertyInfo pi = controlType.GetProperty("DoubleBuffered",
            BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(control, setting, null);
        }
    }
    public class Customer
    {
        // Basic Information - 基本資訊
        public int CustomerID { get; set; } // 客戶編號
        public string CustomerName { get; set; } // 客戶名稱
        public string CompanyName { get; set; } // 公司名稱
        public string ContactName { get; set; } // 聯絡人名稱
        public string ContactPhone { get; set; } // 聯絡電話
        public string Email { get; set; } // 電子郵件

        // Address Information - 地址資訊
        public string MainAddress { get; set; } // 主要地址
        public string ShippingAddress { get; set; } // 送貨地址
        public string BillingAddress { get; set; } // 帳單地址
        public string PostalCode { get; set; } // 郵遞區號
        public string City { get; set; } // 城市
        public string Country { get; set; } // 國家/地區

        // Financial Information - 財務資訊
        public string Currency { get; set; } // 貨幣
        public decimal CreditLimit { get; set; } // 信用額度
        public decimal AccountBalance { get; set; } // 帳款餘額
        public string PaymentTerms { get; set; } // 付款條件

        // Sales Information - 業務資訊
        public string SalesRepresentative { get; set; } // 業務代表
        public string SalesTeam { get; set; } // 銷售團隊
        public string CustomerCategory { get; set; } // 客戶分類
        public string CustomerLevel { get; set; } // 客戶等級

        // Contract Information - 合約資訊
        public DateTime ContractExpirationDate { get; set; } // 合約到期日
        public string ContractType { get; set; } // 合約類型
        public string ContractDetails { get; set; } // 合約內容

        // Notes and Additional Information - 備註與附加資訊
        public string Notes { get; set; } // 備註欄位，供使用者輸入額外的說明或資訊

        // Customer Relationships - 客戶關聯性
        public string AssociatedCustomers { get; set; } // 關聯的其他客戶或子公司
        public string Partnerships { get; set; } // 合作夥伴

        // Communication Records - 溝通記錄
        public string CommunicationRecords { get; set; } // 與客戶的通話、電子郵件和訊息的記錄

        // Market Information - 市場資訊
        public string MarketInfo { get; set; } // 客戶所屬的市場/行業

        // Custom Fields - 自訂欄位
        // public string CustomField1 { get; set; }
        // public string CustomField2 { get; set; }

        // You can add more properties for other fields as needed

        // Constructor - 建構子
        public Customer()
        {
            // Initialize any properties or set default values here
        }
    }
}

