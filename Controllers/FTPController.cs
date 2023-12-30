using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Download_Delete_FTP_POC.Controllers
{
    public class FTPController : Controller
    {
        public static string Baseurl = "ftp://findosaurs.com/forhisham/directory";
        public string username = "hishamtest";
        public string password = "Admin@123";
        public static string downloadDirectory = @"C:\targetFTP\directoryMVC";

        public ActionResult DownloadAction()
        {
            NetworkCredential credentials = new NetworkCredential(username, password);
            TempData["message"] = downloadDirectory;
            DownloadFtpDirectory(Baseurl, credentials, downloadDirectory);
            return View();
        }
        public ActionResult DeleteAction()
        {
            NetworkCredential credentials = new NetworkCredential(username, password);
            DeleteFtpDirectory(Baseurl, credentials);
            TempData["message"] = Baseurl;
            return View();
        }


        public void DownloadFtpDirectory(string url, NetworkCredential credentials, string localPath)
        {
            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(url);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = credentials;

            List<string> lines = new List<string>();

            using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (var listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                    lines.Add(listReader.ReadLine());
                }
            }

            foreach (string line in lines)
            {
                string[] tokens =
                    line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[3];
                string permissions = tokens[2];

                string localFilePath = downloadDirectory + "\\" + name;// Path.Combine(localPath, name);
                string fileUrl = url + "/" + name;

                if (permissions.ToLower() == "<dir>")
                {

                    DownloadFtpDirectory(fileUrl + "/", credentials, localFilePath);
                }
                else
                {
                    FtpWebRequest downloadRequest =
                        (FtpWebRequest)WebRequest.Create(fileUrl);
                    downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    downloadRequest.Credentials = credentials;

                    using (FtpWebResponse downloadResponse =
                              (FtpWebResponse)downloadRequest.GetResponse())
                    using (Stream sourceStream = downloadResponse.GetResponseStream())
                    using (Stream targetStream = System.IO.File.Create(localFilePath))
                    {
                        byte[] buffer = new byte[10240];
                        int read;
                        while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            targetStream.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }

        public static void DeleteFtpDirectory(string url, NetworkCredential credentials)
        {
            var listRequest = (FtpWebRequest)WebRequest.Create(url);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = credentials;

            List<string> lines = new List<string>();

            using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (var listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                    lines.Add(listReader.ReadLine());
                }
            }

            foreach (string line in lines)
            {
                string[] tokens =
                    line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[3];
                string permissions = tokens[2];

                string fileUrl = url + "/" + name;

                if (permissions.ToLower() == "<dir>")
                {
                    DeleteFtpDirectory(fileUrl + "/", credentials);
                }
                else
                {
                    var deleteRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                    deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    deleteRequest.Credentials = credentials;

                    deleteRequest.GetResponse();
                }
            }
            if (url != Baseurl)
            {
                var removeRequest = (FtpWebRequest)WebRequest.Create(url);
                removeRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                removeRequest.Credentials = credentials;
                removeRequest.GetResponse();
            }
        }
    }
}