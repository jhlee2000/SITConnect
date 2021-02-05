using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SITConnect
{
    public partial class Registration : System.Web.UI.Page
    {
        // Hash Password
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {

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

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // Password Complexity
            int scores = checkPassword(tbPassword.Text);
            string status = "";
            switch (scores)
            {
                case 1:
                    status = "[Password] Very Weak";
                    break;
                case 2:
                    status = "[Password] Weak";
                    break;
                case 3:
                    status = "[Password] Medium";
                    break;
                case 4:
                    status = "[Password] Strong";
                    break;
                case 5:
                    status = "[Password] Excellent";
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

            if (getEmail(tbEmail.Text) != null)
            {
                lbError.ForeColor = Color.Red;
                lbError.Text = "Email already exists!";
                return;
            }

            // Hashing Password
            string pwd = tbPassword.Text.ToString().Trim(); ;

            //Generate random "salt"
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] saltByte = new byte[8];
            //Fills array of bytes with a cryptographically strong sequence of random values.
            rng.GetBytes(saltByte);
            salt = Convert.ToBase64String(saltByte);
            SHA512Managed hashing = new SHA512Managed();
            string pwdWithSalt = pwd + salt;
            byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwd));
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
            finalHash = Convert.ToBase64String(hashWithSalt);
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.GenerateKey();
            Key = cipher.Key;
            IV = cipher.IV;
            createAccount();
            Response.Redirect("Login.aspx", false);
        }

        public void createAccount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Account VALUES(@FirstName, @LastName, @CCInfo, @Email, @PasswordHash, @PasswordSalt, @Dob, @IV, @Key, @LoginAttempts, @Locked, @PPOne, @PPTwo, @PwdDateTime, @LockedDateTime)"))
                {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@FirstName", HttpUtility.HtmlEncode(tbFirstName.Text.Trim()));
                            cmd.Parameters.AddWithValue("@LastName", HttpUtility.HtmlEncode(tbLastName.Text.Trim()));
                            //cmd.Parameters.AddWithValue("@Nric", encryptData(tb_nric.Text.Trim()));
                            cmd.Parameters.AddWithValue("@CCInfo", Convert.ToBase64String(encryptData(tbCCInfo.Text.Trim())));
                            cmd.Parameters.AddWithValue("@Email", HttpUtility.HtmlEncode(tbEmail.Text.Trim()));
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                            cmd.Parameters.AddWithValue("@Dob", tbDob.Text.Trim());
                            cmd.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                            cmd.Parameters.AddWithValue("@Key", Convert.ToBase64String(Key));
                            cmd.Parameters.AddWithValue("@LoginAttempts", 0);
                            cmd.Parameters.AddWithValue("@Locked", false);
                            cmd.Parameters.AddWithValue("@PPOne", DBNull.Value);
                            cmd.Parameters.AddWithValue("@PPTwo", DBNull.Value);
                            cmd.Parameters.AddWithValue("@PwdDateTime", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockedDateTime", DBNull.Value);
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
                lbError.Text = "Please make sure your inputs are not longer than 50 characters.";
            }
        }

        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return cipherText;
        }

        protected string getEmail(string userid)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select Email FROM ACCOUNT WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Email"] != null)
                        {
                            if (reader["Email"] != DBNull.Value)
                            {
                                s = reader["Email"].ToString();
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

    }
}