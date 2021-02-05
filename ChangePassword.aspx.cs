using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SITConnect
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        byte[] Key;
        byte[] IV;
        static string finalHash;
        byte[] CCInfo = null;
        string Email = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Email"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    Response.Redirect("Login.aspx", false);
                }
                else
                {
                    Email = (string)Session["Email"];
                    if (Session["PwdChangeMsg"] != null)
                    {
                        lbPwdMsg.Text = (string)Session["PwdChangeMsg"];
                    }
                }
            }
            else
            {
                Response.Redirect("Login.aspx", false);
            }
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            string pwd = tbPassword.Text.ToString().Trim();
            string userid = (string)Session["Email"];
            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash(userid);
            string dbSalt = getDBSalt(userid);
            try
            {
                if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                {
                    string pwdWithSalt = pwd + dbSalt;
                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                    string userHash = Convert.ToBase64String(hashWithSalt);
                    if (userHash.Equals(dbHash))
                    {
                        // Check for 5 mins or PW1 is empty
                        if (getPW1(userid) != null)
                        {
                            // HERE TO CHANGE MIN PASSWORD TIME
                            // SET THE TotalMinues < 1 <-- , HERE IS IF LESS THAN 5 MIN CANNOT CHANGE
                            if ((DateTime.Now - getPasswordDateTime()).TotalMinutes < 5)
                            {
                                lbMsg.Text = "Please wait atleast " + Convert.ToInt32(5 - (DateTime.Now - getPasswordDateTime()).TotalMinutes) + "minute(s) more";
                                return;
                            }
                        }
                        // Password Complexity
                        int scores = checkPassword(tbNewPassword.Text);
                        string status = "";
                        switch (scores)
                        {
                            case 1:
                                status = "Very Weak";
                                break;
                            case 2:
                                status = "Weak";
                                break;
                            case 3:
                                status = "Medium";
                                break;
                            case 4:
                                status = "Strong";
                                break;
                            case 5:
                                status = "Excellent";
                                break;
                            default:
                                break;
                        }
                        lbl_pwdchecker.Text = "Status : " + status;
                        if (scores < 4)
                        {
                            lbl_pwdchecker.ForeColor = Color.Red;
                            return;
                        }
                        lbl_pwdchecker.ForeColor = Color.Green;

                        // Hashing New Password
                        string new_pwd = tbNewPassword.Text.ToString().Trim(); ;

                        SHA512Managed _hashing = new SHA512Managed();
                        string new_pwdWithSalt = new_pwd + dbSalt;
                        byte[] plainHash = _hashing.ComputeHash(Encoding.UTF8.GetBytes(new_pwd));
                        byte[] _hashWithSalt = _hashing.ComputeHash(Encoding.UTF8.GetBytes(new_pwdWithSalt));
                        finalHash = Convert.ToBase64String(_hashWithSalt);
                        // Check Password History
                        if (finalHash == getPW1(userid) || finalHash == getPW2(userid) || finalHash == dbHash)
                        {
                            lbMsg.Text = "Cannot use previous 2 passwords or current password.";
                            return;
                        }
                        
                        string ppone = getPW1(userid); // get previous password 1
                        setPW2(ppone);
                        setPW1(dbHash);
                        changePassword();
                        Session["PwdChangeMsg"] = null;
                        lbPwdMsg.Text = "";
                        lbMsg.Text = "Password Changed Successfully!";
                    }
                    else
                    {
                        lbMsg.Text = "Current Password Is Wrong!";
                    }
                }
                else
                {
                    //errorMsg = "Userid or password is not valid. Please try again.";
                    //lbMsg.Text = "Userid or password is not valid. Please try again.";

                    //Response.Redirect("Login.aspx", false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
        }

        private int checkPassword(string password)
        {
            int score = 0;
            if (password.Length < 8)
            {
                return 1;
            }
            else
            {
                score++;
            }
            if (Regex.IsMatch(password, "[a-z]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[A-Z]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[0-9]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[^A-Za-z0-9]"))
            {
                score++;
            }
            return score;
        }

        // Get password history 1
        protected string getPW1(string userid)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PreviousPasswordOne FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PreviousPasswordOne"] != null)
                        {
                            if (reader["PreviousPasswordOne"] != DBNull.Value)
                            {
                                s = reader["PreviousPasswordOne"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }

        // Get password history 2
        protected string getPW2(string userid)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PreviousPasswordTwo FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PreviousPasswordTwo"] != null)
                        {
                            if (reader["PreviousPasswordTwo"] != DBNull.Value)
                            {
                                s = reader["PreviousPasswordTwo"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }

        public void setPW1(string password)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET PreviousPasswordOne=@PreviousPasswordOne WHERE Email=@Email"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Email", (string)Session["Email"]);
                            cmd.Parameters.AddWithValue("@PreviousPasswordOne", password);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                lbMsg.Text = "Please make sure your inputs are not longer than 50 characters.";
            }
        }

        public void setPW2(string password)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET PreviousPasswordTwo=@PreviousPasswordTwo WHERE Email=@Email"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Email", (string)Session["Email"]);
                            cmd.Parameters.AddWithValue("@PreviousPasswordTwo", password);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                lbMsg.Text = "Please make sure your inputs are not longer than 50 characters.";
            }
        }

        public void changePassword()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET PasswordHash=@PasswordHash WHERE Email=@Email"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Email", (string)Session["Email"]);
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                            setPasswordDateTime();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                lbMsg.Text = "Please make sure your inputs are not longer than 50 characters.";
            }
        }

        public void setPasswordDateTime()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET PasswordDateTime=@PasswordDateTime WHERE Email=@Email"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Email", (string)Session["Email"]);
                            cmd.Parameters.AddWithValue("@PasswordDateTime", DateTime.Now);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                lbMsg.Text = "Please make sure your inputs are not longer than 50 characters.";
            }
        }

        protected DateTime getPasswordDateTime()
        {
            DateTime s = DateTime.Now;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordDateTime FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", (string)Session["Email"]);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordDateTime"] != null)
                        {
                            if (reader["PasswordDateTime"] != DBNull.Value)
                            {
                                s = Convert.ToDateTime(reader["PasswordDateTime"]);
                            }
                            else
                            {
                                s = s.AddHours(-1);
                            }
                        } else
                        {
                            s = s.AddHours(-1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }

        protected string getDBHash(string userid)
        {
            string h = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordHash FROM Account WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            {
                                h = reader["PasswordHash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return h;
        }

        protected string getDBSalt(string userid)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PASSWORDSALT FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PASSWORDSALT"] != null)
                        {
                            if (reader["PASSWORDSALT"] != DBNull.Value)
                            {
                                s = reader["PASSWORDSALT"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }

        //protected void btnBack_Click(object sender, EventArgs e)
        //{
        //    Response.Redirect("UserProfile.aspx", false);
        //}
    }
}