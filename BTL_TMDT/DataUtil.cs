using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;


namespace BTL_TMDT
{
    public class DataUtil
    {
        SqlConnection con;
        public DataUtil()
        {
            string sqlCon = "Data Source=DESKTOP-6TL1N9S;Initial Catalog=CuaHangSachDB;Integrated Security=True;TrustServerCertificate=True";
            con = new SqlConnection(sqlCon);
        }

        public DataTable GetDonHangData()
        {
            DataTable dt = new DataTable();
            using (con)
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
    SELECT 
        DH.MaDonHang,
        TK.HoTen,
        DH.ThoiGianDatHang,
        SUM(CTDH.SoLuong) AS SoLuong,
        DH.TrangThai,
        DH.PhuongThucThanhToan,
        DH.TongGiaTri
    FROM 
        DonHang DH
    JOIN 
        TaiKhoan TK ON DH.MaTaiKhoan = TK.MaTaiKhoan
    JOIN 
        ChiTietDonHang CTDH ON DH.MaDonHang = CTDH.MaDonHang
    GROUP BY 
        DH.MaDonHang, 
        TK.HoTen, 
        DH.ThoiGianDatHang, 
        DH.TrangThai, 
        DH.PhuongThucThanhToan, 
        DH.TongGiaTri
    ORDER BY 
        DH.ThoiGianDatHang DESC;", con);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }


        

        
        
    }
}