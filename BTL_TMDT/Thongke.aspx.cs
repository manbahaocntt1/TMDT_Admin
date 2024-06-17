
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace BTL_TMDT
{
    public partial class Thongke : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateYearDropdown();

                // Bind the GridView initially for "Today"
                BindGridView("Today");
                revThang.SelectedIndexChanged += new EventHandler(revThang_SelectedIndexChanged);
                string selectedMonth = revThang.SelectedValue;
                LoadOrderDataThang(selectedMonth);
                string selectedYear = revNam.SelectedValue;
                LoadOrderDataNam(selectedYear);
            }
        }


        protected void ddlDateRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedValue = ddlDateRange.SelectedValue;
            BindGridView(selectedValue);
            string selectedMonth = revThang.SelectedValue;
            LoadOrderDataThang(selectedMonth);
            string selectedYear = revNam.SelectedValue;
            LoadOrderDataNam(selectedYear);
        }

        protected void grvRecentSale_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grvRecentSale.PageIndex = e.NewPageIndex;
            string selectedValue = ddlDateRange.SelectedValue;
            BindGridView(selectedValue); // Hàm này bạn cần tự định nghĩa để load dữ liệu mới từ cơ sở dữ liệu
            string selectedMonth = revThang.SelectedValue;
            LoadOrderDataThang(selectedMonth);
            string selectedYear = revNam.SelectedValue;
            LoadOrderDataNam(selectedYear);
        }



          

        private void BindGridView(string dateRange)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CuaHangSachDBConnectionString4"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Query to get the order details
                string query = @"SELECT 
                    dh.MaDonHang, 
                    dh.ThoiGianDatHang, 
                    tk.HoTen, 
                    s.TenSach, 
                    ctdh.SoLuong,
                    dh.TongGiaTri, 
                    dh.TrangThai
                FROM 
                    DonHang dh
                INNER JOIN 
                    ChiTietDonHang ctdh ON dh.MaDonHang = ctdh.MaDonHang
                INNER JOIN 
                    Sach s ON ctdh.MaSach = s.MaSach
                INNER JOIN 
                    TaiKhoan tk ON dh.MaTaiKhoan = tk.MaTaiKhoan";

                // Query to get the total quantity sold and the distinct dates
                string totalQuantityAndDateQuery = @"SELECT 
                                        SUM(ctdh.SoLuong) AS TotalQuantity, 
                                        MIN(CAST(dh.ThoiGianDatHang AS DATE)) AS StartDate,
                                        MAX(CAST(dh.ThoiGianDatHang AS DATE)) AS EndDate
                                     FROM 
                                        DonHang dh
                                     INNER JOIN 
                                        ChiTietDonHang ctdh ON dh.MaDonHang = ctdh.MaDonHang";

                // Query to get the total revenue
                string totalRevenueQuery = @"SELECT 
                                SUM(dh.TongGiaTri) AS TotalRevenue
                             FROM 
                                DonHang dh
                             ";

                // Append date filter based on selected value
                string dateFilter = string.Empty;
                string dateText = string.Empty;
                switch (dateRange)
                {
                    case "Today":
                        dateFilter = " WHERE CAST(dh.ThoiGianDatHang AS DATE) = CAST(GETDATE() AS DATE)";
                        DateTime currentTime = DateTime.Now;
                        dateText = currentTime.ToString("dd/MM/yyyy");
                        break;
                    case "Yesterday":
                        dateFilter = " WHERE CAST(dh.ThoiGianDatHang AS DATE) = CAST(GETDATE() - 1 AS DATE)";
                        break;
                    case "7Days":
                        dateFilter = " WHERE dh.ThoiGianDatHang >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))";
                        DateTime endDate = DateTime.Now;
                        DateTime startDate = endDate.AddDays(-7);
                        dateText = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
                        break;
                    default:
                        break;
                }

                query += dateFilter;
                totalQuantityAndDateQuery += dateFilter;
                totalRevenueQuery += dateFilter;

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            pnlNoSales.Visible = true;
                            grvRecentSale.Visible = false;
                        }
                        else
                        {
                            pnlNoSales.Visible = false;
                            grvRecentSale.Visible = true;
                            grvRecentSale.DataSource = dt;
                            grvRecentSale.DataBind();
                        }
                    }
                }

                // Get the total quantity sold and the dates
                using (SqlCommand totalQuantityAndDateCmd = new SqlCommand(totalQuantityAndDateQuery, con))
                {
                    con.Open();
                    SqlDataReader reader = totalQuantityAndDateCmd.ExecuteReader();
                    string quantityAndDateText = "";
                    if (reader.Read())
                    {
                        int totalQuantity = reader["TotalQuantity"] != DBNull.Value ? Convert.ToInt32(reader["TotalQuantity"]) : 0;
                        DateTime startDate = reader["StartDate"] != DBNull.Value ? Convert.ToDateTime(reader["StartDate"]) : DateTime.MinValue;
                        DateTime endDate = reader["EndDate"] != DBNull.Value ? Convert.ToDateTime(reader["EndDate"]) : DateTime.MinValue;

                        if (string.IsNullOrEmpty(dateText))
                        {
                            dateText = startDate == endDate ? startDate.ToString("dd/MM/yyyy") : $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
                        }
                        quantityAndDateText = $"Tổng số sách đã bán: {totalQuantity}";
                    }
                    reader.Close();
                    con.Close();

                    // Display the dates in thoigian panel
                    thoigianban.Controls.Clear();
                    thoigianban.Controls.Add(new Literal { Text = dateText });
                    thoigiandt.Controls.Clear();
                    thoigiandt.Controls.Add(new Literal { Text = dateText });

                    // Display the total items sold in slDay panel
                    slDay.Controls.Clear();
                    slDay.Controls.Add(new Literal { Text = quantityAndDateText });

                    // Get the total revenue
                    using (SqlCommand totalRevenueCmd = new SqlCommand(totalRevenueQuery, con))
                    {
                        con.Open();
                        object revenueResult = totalRevenueCmd.ExecuteScalar();
                        con.Close();

                        decimal totalRevenue = revenueResult != DBNull.Value ? Convert.ToDecimal(revenueResult) : 0;

                        // Display total revenue in doanhthu panel
                        doanhthu.Controls.Clear();
                        doanhthu.Controls.Add(new Literal { Text = $"{totalRevenue:C}" });
                    }
                }
            }
        }



        protected void revThang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMonth = revThang.SelectedValue;
            LoadOrderDataThang(selectedMonth);
            string selectedValue = ddlDateRange.SelectedValue;
            BindGridView(selectedValue);
            string selectedYear = revNam.SelectedValue;
            LoadOrderDataNam(selectedYear);
        }

        
        

        private void LoadOrderDataThang(string monthValue)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CuaHangSachDBConnectionString4"].ConnectionString;
            string dateFilter = string.Empty;

            switch (monthValue)
            {
                case "t1":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 1";
                    break;
                case "t2":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 2";
                    break;
                case "t3":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 3";
                    break;
                case "t4":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 4";
                    break;
                case "t5":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 5";
                    break;
                case "t6":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 6";
                    break;
                case "t7":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 7";
                    break;
                case "t8":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 8";
                    break;
                case "t9":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 9";
                    break;
                case "t10":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 10";
                    break;
                case "t11":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 11";
                    break;
                case "t12":
                    dateFilter = " WHERE MONTH(dh.ThoiGianDatHang) = 12";
                    break;
                default:
                    break;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = $@"
                SELECT 
                    COUNT(MaDonHang) AS TotalOrders, 
                    ISNULL(SUM(TongGiaTri), 0) AS TotalRevenue 
                FROM DonHang dh
                {dateFilter}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int totalOrdersmonth = reader.GetInt32(0);
                        decimal totalRevenue = reader.GetDecimal(1);

                        pnThang.Controls.Clear();
                        SumDoanhThuThang.Controls.Clear();

                        if (totalOrdersmonth == 0)
                        {
                            pnThang.Controls.Add(new Literal { Text = "Không có đơn hàng nào trong tháng này." });
                            SumDoanhThuThang.Controls.Add(new Literal { Text = "Không có doanh thu trong tháng này." });
                            
                        }
                        else
                        {
                            pnThang.Controls.Add(new Literal { Text = totalOrdersmonth.ToString() });
                            SumDoanhThuThang.Controls.Add(new Literal { Text = totalRevenue.ToString("C2") });
                        }
                    }
                }
            }
        }


        private void PopulateYearDropdown()
        {
            // Clear existing items to avoid duplicates
            revNam.Items.Clear();

            // Populate DropDownList with a range of years (e.g., from 2000 to current year)
            int startYear = 2000;
            int currentYear = DateTime.Now.Year;

            for (int year = startYear; year <= currentYear; year++)
            {
                revNam.Items.Add(new ListItem(year.ToString(), year.ToString()));
            }

            // Set the default selected value to the current year
            revNam.SelectedValue = currentYear.ToString();
        }

        protected void revNam_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedYear = revNam.SelectedValue;
            LoadOrderDataNam(selectedYear);
            string selectedValue = ddlDateRange.SelectedValue;
            BindGridView(selectedValue);
            string selectedMonth = revThang.SelectedValue;
            LoadOrderDataThang(selectedMonth);
        }

        private void LoadOrderDataNam(string yearValue)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CuaHangSachDBConnectionString4"].ConnectionString;

            string dateFilter = $" WHERE YEAR(dh.ThoiGianDatHang) = {yearValue}";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = $@"
                SELECT 
                    COUNT(MaDonHang) AS TotalOrders, 
                    ISNULL(SUM(TongGiaTri), 0) AS TotalRevenue 
                FROM DonHang dh
                {dateFilter}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int totalOrders = reader.GetInt32(0);
                        decimal totalRevenue = reader.GetDecimal(1);

                        pnNam.Controls.Clear();
                        SumDoanhThuNam.Controls.Clear();

                        if (totalOrders == 0)
                        {
                            pnNam.Controls.Add(new Literal { Text = "Không có đơn hàng nào trong năm này." });
                            SumDoanhThuNam.Controls.Add(new Literal { Text = "Không có doanh thu trong năm này." });


                        }
                        else
                        {
                            pnNam.Controls.Add(new Literal { Text = totalOrders.ToString() });
                            SumDoanhThuNam.Controls.Add(new Literal { Text = totalRevenue.ToString("C2") });
                        }
                    }
                }
            }
        }






    }
}