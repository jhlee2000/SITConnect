using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Web.Script.Serialization;
using System.Web.Services;


namespace SITConnect
{
    public partial class Login : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public class MyObject
        {
            public string success { get; set; }
            public List<string> ErrorMessage { get; set; }
        }

        public bool ValidateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest
                .Create("https://www.google.com/recaptcha/api/siteverify?secret=6LcIiT8aAAAAAO5ZyheJ5nFCLGKuSQsLMT56WVQh &response=" + captchaResponse);
            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        //lbl_gScore.Text = jsonResponse.ToString();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);
                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
         }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                string pwd = HttpUtility.HtmlEncode(tbPassword.Text.ToString().Trim());
                string userid = HttpUtility.HtmlEncode(tbEmail.Text.ToString().Trim());
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
                        if (userHash.Equals(dbHash) && !getAccountLocked(userid))
                        {
                            // Successful Login
                            clearAttempts();
                            Session["Email"] = userid;
                            string guid = Guid.NewGuid().ToString();
                            Session["AuthToken"] = guid;
                            Response.Cookies.Add(new HttpCookie("AuthToken", guid));

                            Response.Redirect("UserProfile.aspx", false);

                            //lbMsg.Text = "Success!";
                            //displayUserProfile(userid);
                            //Response.Redirect("Success.aspx", false);
                        }
                        else
                        {
                            if (getAccountLocked(userid))
                            {
                                lbMsg.Text = "Your account has been locked! Try again in 30 mins.";
                            }
                            else
                            {
                                checkLoginAttempts();
                                lbMsg.Text = "Userid or password is not valid. Please try again.";
                            }
                        }
                    }
                    else
                    {
                        //errorMsg = "Userid or password is not valid. Please try again.";
                        lbMsg.Text = "Userid or password is not valid. Please try again.";

                        //Response.Redirect("Login.aspx", false);
                    }
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.ToString());
                    lbMsg.Text = "fok";
                }
                finally { }
            } else
            {
                lbMsg.Text = "reCAPTCHA: Please try again.";
            }
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

        public void checkLoginAttempts()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET LoginAttempts = @LoginAttempts WHERE Email=@Email"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            // If try 3 times
                            if (getLoginAttempts(tbEmail.Text) + 1 == 3)
                            {
                                using (SqlCommand cmd2 = new SqlCommand("UPDATE Account SET Locked = @Locked, LockedDateTime = @LockedDateTime WHERE Email=@Email"))
                                {
                                    // Set locked
                                    cmd2.CommandType = CommandType.Text;
                                    cmd2.Parameters.AddWithValue("@Locked", true);
                                    cmd2.Parameters.AddWithValue("@Email", tbEmail.Text.Trim());
                                    cmd2.Parameters.AddWithValue("@LockedDateTime", DateTime.Now);
                                    cmd2.Connection = con;
                                    con.Open();
                                    cmd2.ExecuteNonQuery();
                                    con.Close();
                                }
                                clearAttempts();
                            } else
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.AddWithValue("@LoginAttempts", getLoginAttempts(tbEmail.Text) + 1);
                                cmd.Parameters.AddWithValue("@Email", tbEmail.Text.Trim());
                                cmd.Connection = con;
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void clearAttempts()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET LoginAttempts = @LoginAttempts WHERE Email=@Email"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@LoginAttempts", 0);
                            cmd.Parameters.AddWithValue("@Email", tbEmail.Text.Trim());
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
                throw new Exception(ex.ToString());
            }
        }

        public void clearLocked()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Account SET Locked = @Locked WHERE Email=@Email"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Locked", false);
                            cmd.Parameters.AddWithValue("@Email", tbEmail.Text.Trim());
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
                throw new Exception(ex.ToString());
            }
        }

        protected int getLoginAttempts(string userid)
        {
            int s = 0;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select LoginAttempts FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["LoginAttempts"] != null)
                        {
                            if (reader["LoginAttempts"] != DBNull.Value)
                            {
                                s = (int) reader["LoginAttempts"];
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

        protected bool getAccountLocked(string userid)
        {
            bool s = false;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select Locked FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Locked"] != null)
                        {
                            if (reader["Locked"] != DBNull.Value)
                            {
                                s = Convert.ToBoolean(reader["Locked"]);
                                // ACCOUNT LOCKOUT FINISH AFTER 30 MINS
                                if ((DateTime.Now - getLockedDateTime(userid)).TotalMinutes > 30)
                                {
                                    //lbMsg.Text = "Please wait atleast " + Convert.ToInt32(5 - (DateTime.Now - getPasswordDateTime()).TotalMinutes) + "minute(s) more";
                                    //return;
                                    // should change to update locked to false
                                    clearLocked();
                                    s = false;
                                }
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

        protected DateTime getLockedDateTime(string userid)
        {
            DateTime s = DateTime.Now;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select LockedDateTime FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["LockedDateTime"] != null)
                        {
                            if (reader["LockedDateTime"] != DBNull.Value)
                            {
                                s = Convert.ToDateTime(reader["LockedDateTime"]);
                            }
                            else
                            {
                                s = s.AddHours(-1);
                            }
                        } 
                        else
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
    }
}