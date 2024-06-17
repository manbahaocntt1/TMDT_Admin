using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace admin
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.BindGrid();
                //GridView1.DataBind();
                FillMaTacGiaDropDownList();
                FillMaDanhMucDropDownList();

            }
        }

        private void BindGrid()
        {
            string constr = ConfigurationManager.ConnectionStrings["CuaHangSachDBConnectionString4"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Sach WHERE TenSach LIKE '%' + CAST(@TenSach AS NVARCHAR(MAX)) + '%'", con))
                {
                    // optional text box filter (blank = all)
                    // Đã sửa: chỉ cần thiết lập giá trị cho tham số, không cần thêm câu lệnh WHERE nữa
                    string searchTearm = txtSearch.Text.Trim();
                    if (string.IsNullOrEmpty(searchTearm))
                    {
                        searchTearm = "%"; // tìm tất cả sách nếu không nhập gì
                    }

                    cmd.Parameters.Add("@TenSach", SqlDbType.NVarChar).Value = searchTearm;
                    GridView1.DataSourceID = null;
                    con.Open();
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }
            }
        }



        protected void btnSearch_Click(object sender, EventArgs e)
        {
            this.BindGrid();
        }



        private void FillMaTacGiaDropDownList()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CuaHangSachDBConnectionString4"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT MaTacGia, TenTacGia FROM TacGia WHERE Visible = 1", con);

                con.Open();
                ddlMaTacGia.DataSource = cmd.ExecuteReader();
                ddlMaTacGia.DataTextField = "TenTacGia"; // cột bạn muốn hiển thị
                ddlMaTacGia.DataValueField = "MaTacGia"; // giá trị khi chọn
                ddlMaTacGia.DataBind();

            }
            ddlMaTacGia.Items.Insert(0, new ListItem("--Chọn Tác Giả--", "0"));
        }

        private void FillMaDanhMucDropDownList()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CuaHangSachDBConnectionString4"].ToString(); // Thay thế YourConnectionString với tên chuỗi kết nối thực của bạn

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Thêm điều kiện WHERE để chỉ lấy những danh mục có Visible = 1
                SqlCommand cmd = new SqlCommand("SELECT MaDanhMucPhu, TenDanhMuc FROM DanhMucPhu WHERE Visible = 1", con); // Điều chỉnh tên cột và bảng cho phù hợp

                con.Open();
                ddlMaDanhMuc.DataSource = cmd.ExecuteReader();
                ddlMaDanhMuc.DataTextField = "TenDanhMuc"; // cột bạn muốn hiển thị
                ddlMaDanhMuc.DataValueField = "MaDanhMucPhu"; // giá trị khi chọn
                ddlMaDanhMuc.DataBind();
            }
            ddlMaDanhMuc.Items.Insert(0, new ListItem("--Chọn Danh Mục--", "0")); // Thêm lựa chọn không chọn
        }


        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {

            // Viết code xử lý khi một hàng trong GridView được chọn ở đây
        }

        


        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                // Lấy chỉ số dòng từ CommandArgument
                int rowIndex = Convert.ToInt32(e.CommandArgument);

                // Lấy MaSach từ DataKeys của GridView
                string maSach = GridView1.DataKeys[rowIndex].Value.ToString();

                // Chuyển hướng đến trang SuaSach.aspx với MaSach trong query string
                Response.Redirect("SuaSach.aspx?MaSach=" + maSach);
            }
        }




        protected void btnAddBook_Click(object sender, EventArgs e)
        {
            // Lấy thông tin từ các TextBox

            // Kiểm tra nhập liệu
            if (string.IsNullOrWhiteSpace(txtNewTenSach.Text) ||
                string.IsNullOrWhiteSpace(txtNewGiaGoc.Text) ||
                string.IsNullOrWhiteSpace(txtNewGiaBan.Text) ||
                string.IsNullOrWhiteSpace(txtNewSoLuongDaBan.Text) ||
                string.IsNullOrWhiteSpace(txtNewSoLuongConDu.Text) ||
                string.IsNullOrWhiteSpace(txtNewTomTat.Text) ||
                string.IsNullOrWhiteSpace(txtNewNhaXuatBan.Text) ||
                string.IsNullOrWhiteSpace(txtNewNamXuatBan.Text) ||
                string.IsNullOrWhiteSpace(txtNewHinhThuc.Text) ||
                string.IsNullOrWhiteSpace(txtNewSoTrang.Text) ||
                string.IsNullOrWhiteSpace(txtNewKichThuoc.Text) ||
                !fileUploadAnhSach.HasFile ||
                ddlMaTacGia.SelectedIndex == 0 ||
                ddlMaDanhMuc.SelectedIndex == 0)
            {
                // Hiển thị thông báo lỗi
                Response.Write("<script>alert('Bạn phải nhập đầy đủ thông tin.');</script>");
                return; // Thoát sớm khỏi phương thức nếu có thông tin bị thiếu
            }


            string tenSach = txtNewTenSach.Text;
            decimal giaGoc = Convert.ToDecimal(txtNewGiaGoc.Text);
            decimal giaBan = Convert.ToDecimal(txtNewGiaBan.Text);
            int soLuongDaBan = Convert.ToInt32(txtNewSoLuongDaBan.Text);
            int soLuongConDu = Convert.ToInt32(txtNewSoLuongConDu.Text);
            string tomTat = txtNewTomTat.Text;
            string nhaXuatBan = txtNewNhaXuatBan.Text;
            int namXuatBan = Convert.ToInt32(txtNewNamXuatBan.Text);
            string hinhThuc = txtNewHinhThuc.Text;
            int soTrang = Convert.ToInt32(txtNewSoTrang.Text);
            string kichThuoc = txtNewKichThuoc.Text;
            double trongLuong = Convert.ToDouble(txtNewTrongLuong.Text);
            string maTacGia = ddlMaTacGia.SelectedValue;
            string maDanhMuc = ddlMaDanhMuc.SelectedValue;
            bool Visible = chkVisible.Checked;
            //bool visible = txtNewVisible.Text.ToLower() == "true" || txtNewVisible.Text == "1";



            // Sử dụng SqlDataSource để thực hiện việc thêm dữ liệu vào database
            SqlDataSource1.InsertParameters["TenSach"].DefaultValue = tenSach;
            SqlDataSource1.InsertParameters["GiaGoc"].DefaultValue = giaGoc.ToString();
            SqlDataSource1.InsertParameters["GiaBan"].DefaultValue = giaBan.ToString();
            SqlDataSource1.InsertParameters["SoLuongDaBan"].DefaultValue = soLuongDaBan.ToString();
            SqlDataSource1.InsertParameters["SoLuongConDu"].DefaultValue = soLuongConDu.ToString();
            SqlDataSource1.InsertParameters["TomTat"].DefaultValue = tomTat;
            SqlDataSource1.InsertParameters["NhaXuatBan"].DefaultValue = nhaXuatBan;
            SqlDataSource1.InsertParameters["NamXuatBan"].DefaultValue = namXuatBan.ToString();
            SqlDataSource1.InsertParameters["HinhThuc"].DefaultValue = hinhThuc;
            SqlDataSource1.InsertParameters["SoTrang"].DefaultValue = soTrang.ToString();
            SqlDataSource1.InsertParameters["KichThuoc"].DefaultValue = kichThuoc;
            SqlDataSource1.InsertParameters["TrongLuong"].DefaultValue = trongLuong.ToString();
            SqlDataSource1.InsertParameters["MaTacGia"].DefaultValue = maTacGia.ToString();
            SqlDataSource1.InsertParameters["MaDanhMuc"].DefaultValue = maDanhMuc.ToString();
            SqlDataSource1.InsertParameters["Visible"].DefaultValue = Visible.ToString();
            //SqlDataSource1.InsertParameters["Visible"].DefaultValue = visible.ToString();


            // chọn File ảnh sách
            string filePath = Server.MapPath("~/Images/") + fileUploadAnhSach.FileName;
            try
            {
                fileUploadAnhSach.SaveAs(filePath); // Lưu ảnh vào thư mục Images
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Có lỗi khi tải ảnh: " + ex.Message + "');</script>");
                return; // Thoát sớm khỏi phương thức nếu có lỗi
            }
            filePath = string.Empty;
            if (fileUploadAnhSach.HasFile)
            {
                try
                {
                    string filename = Path.GetFileName(fileUploadAnhSach.FileName);
                    filePath = Path.Combine("~/Images/", filename);
                    string serverPath = Server.MapPath(filePath);

                    // Lưu file ảnh vào thư mục Images
                    fileUploadAnhSach.SaveAs(serverPath);

                    // Gán tên file (không kèm thư mục) vào tham số của SqlDataSource
                    // Chú ý: Ở đây chỉ gán tên file, không gán cả đường dẫn
                    SqlDataSource1.InsertParameters["AnhSach"].DefaultValue = filename;
                }
                catch (Exception ex2)
                {
                    // Xử lý lỗi ở đây, có thể log lỗi hoặc hiển thị thông báo
                    Response.Write("Có lỗi xảy ra: " + ex2.Message);
                    return; // Dừng việc xử lý nếu có lỗi
                }
            }
            else
            {
                // Set một thông báo nếu không có file nào được chọn
                Response.Write("Vui lòng chọn file ảnh cho sách.");
                return; // Dừng việc xử lý nếu không có file
            }

            // Set các tham số còn lại cho SqlDataSource
            // Sử dụng SqlDataSource để thực hiện việc thêm dữ liệu vào database
            // Các tham số khác như đã set ở trên

            try
            {
                // Thực hiện lệnh Insert
                SqlDataSource1.Insert();
                
                // Làm mới lại GridView để hiển thị dữ liệu mới được thêm vào
                this.BindGrid();
                Response.Write("<script>alert('Thêm thành công');</script>");

                // Xóa các giá trị trong TextBox sau khi thêm thành công
                txtNewTenSach.Text = string.Empty;
                txtNewGiaGoc.Text = string.Empty;
                txtNewGiaBan.Text = string.Empty;
                txtNewSoLuongDaBan.Text = string.Empty;
                txtNewSoLuongConDu.Text = string.Empty;
                txtNewTomTat.Text = string.Empty;
                txtNewNhaXuatBan.Text = string.Empty;
                txtNewNamXuatBan.Text = string.Empty;
                txtNewHinhThuc.Text = string.Empty;
                txtNewSoTrang.Text = string.Empty;
                txtNewKichThuoc.Text = string.Empty;
                txtNewTrongLuong.Text = string.Empty;
                chkVisible.Checked = false;

                // Reset Dropdownlist lại giá trị mặc định
                ddlMaTacGia.SelectedIndex = 0;
                ddlMaDanhMuc.SelectedIndex = 0;

                // Xóa các giá trị trong TextBox sau khi thêm thành công
                // Code để xóa các giá trị TextBox nếu cần
            }
            catch (Exception ex3)
            {
                // Xử lý lỗi ở đây, có thể log lỗi hoặc hiển thị thông báo
                Response.Write("Có lỗi xảy ra khi thêm sách mới: " + ex3.Message);
            }

            

        }
        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            BindGrid(); // Gọi lại phương thức BindGrid() để nạp lại dữ liệu với trang mới.
        }







    }

}