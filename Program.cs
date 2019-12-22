using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FtpSampleApp
{
    class Program
    {
        const string FTP_URL= "ftp://localhost";
        const string USERNAME = "ftpuser";
        const string PASSWORD = "test_password";
        static async Task Main(string[] args)
        {
            Console.WriteLine(await GetDirectoryListing());
            Console.WriteLine(await DownloadFile());
            Console.WriteLine(await UploadFile());

            Thread.Sleep(5000);
        }

        private static async Task<string> GetDirectoryListing()
        {
            StringBuilder stringBuilder = new StringBuilder();
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(FTP_URL);
            req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            req.Credentials = new NetworkCredential(USERNAME, PASSWORD);
            req.EnableSsl = false;
            FtpWebResponse resp = (FtpWebResponse)await req.GetResponseAsync();

            using (var respStream = resp.GetResponseStream())
            {
                using (var reader = new StreamReader(respStream))
                {
                    stringBuilder.Append(reader.ReadToEnd());
                    stringBuilder.AppendLine("--------");
                    stringBuilder.Append(resp.WelcomeMessage);
                    stringBuilder.Append($"Request Returned Status {resp.StatusCode}");
                }
                return stringBuilder.ToString();
            }
        }

        private static async Task<string> DownloadFile()
        {
            StringBuilder stringBuilder = new StringBuilder();
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create($"{FTP_URL}/New folder/New_Text_Document.txt");
            req.Method = WebRequestMethods.Ftp.DownloadFile;

            req.Credentials = new NetworkCredential(USERNAME, PASSWORD);
            //req.EnableSsl = false;
            req.UsePassive = true;
            try
            {
                using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
                {
                    using (var respStream = resp.GetResponseStream())
                    {
                        stringBuilder.Append(resp.StatusDescription);
                        if (!File.Exists(@"./Copy_New_Text_Document.txt"))
                        {
                            using (var file = File.Create(@"./Copy_New_Text_Document.txt"))
                            {
                                //only use to create file in our ftp directory
                                string msg = "welcome test, file copied successfully";
                                byte[] bytes = Encoding.ASCII.GetBytes(msg);
                                file.Write(bytes, 0, bytes.Length);
                                file.Close();
                            }
                        }
                        using (var respReader = new StreamReader(respStream))
                        {
                            using (var fileWriter = File.OpenWrite(@"../New_Text_Document.txt"))
                            {
                                using (StreamWriter strWriter = new StreamWriter(fileWriter))
                                {
                                    await strWriter.WriteAsync(await respReader.ReadToEndAsync());
                                }
                            }
                        }
                    }
                    return stringBuilder.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return e.Message;
            }
        }

        private static async Task<string> UploadFile()
        {
            StringBuilder stringBuilder = new StringBuilder();
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create($"{FTP_URL}/New folder/Program.cs");
            req.Method = WebRequestMethods.Ftp.UploadFile;

            req.Credentials = new NetworkCredential(USERNAME, PASSWORD);
            req.UsePassive = true;

            byte[] fileBytes;

            using(var reader = new StreamReader("./Copy_New_Text_Document.txt"))
            {
                fileBytes = Encoding.ASCII.GetBytes(reader.ReadToEnd());
            }

            req.ContentLength = fileBytes.Length;

            using(var reqStream = await req.GetRequestStreamAsync())
            {
                await reqStream.WriteAsync(fileBytes, 0, fileBytes.Length);
            }

            using(FtpWebResponse ftpWebResponse = (FtpWebResponse)req.GetResponse())
            {
                stringBuilder.Append(ftpWebResponse.StatusDescription);
            }
            return stringBuilder.ToString();
        }
    }
}
