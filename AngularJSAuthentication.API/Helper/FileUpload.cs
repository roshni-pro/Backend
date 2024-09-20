using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public static class FileUpload
    {
        public static string ISFileUpload(HttpPostedFile httpPostedFile, string Extension, string FolderName, int MaxSize)
        {
            string ReturnUrl = string.Empty;
            string logfilesPath = (ConfigurationManager.AppSettings["logfilesPath"]).Replace(@"\\", @"\");

            //string folderPath = HttpContext.Current.Server.MapPath("~/"+FolderName+"/");
            if (!Directory.Exists(logfilesPath + FolderName))
            {
                //string FolderNameLocal = new Uri(FolderName).LocalPath;
                Directory.CreateDirectory(logfilesPath+FolderName);
            }
            IsFileValid(httpPostedFile, httpPostedFile.ContentType, Extension, MaxSize, httpPostedFile.ContentLength);
            string extension = Path.GetExtension(httpPostedFile.FileName);
            string newFileName = Guid.NewGuid() + extension;
            string ActualFile = Path.Combine(logfilesPath+ FolderName , newFileName);
            httpPostedFile.SaveAs(ActualFile);
            if (File.Exists(logfilesPath + FolderName +"/"+ newFileName))
            {
                ReturnUrl = "http://192.168.1.149:8080/ImagesFolder/" + FolderName + "/" + newFileName; 
            }
            return ReturnUrl;
        }

        private static void IsFileValid(HttpPostedFile httpPostedFile,string FileExtension, string Extension, int MaxSize,int FileSize)
        {
            //string Extension = Path.GetExtension(FilePath);
            if(FileExtension.ToUpper().Replace("IMAGE/", "") != Extension.ToUpper())
            {
                throw new Exception("Please upload File in " + FileExtension + "only.");
            }
            if((FileSize/1024) > MaxSize)
            {
                throw new Exception("Size of uploaded file should be less than "+ MaxSize + "Kb only");
            }
        }
    }
}