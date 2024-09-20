using System;
using System.IO;
using System.IO.Compression;
//using System.IO.Compression.FileSystem;
using System.Web.Http;



namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Multipleimg")]
    public class MultipleimgController : ApiController
    {

        [HttpPost()]
        public string UploadFiles()
        {
            int iUploadedCnt = 0;

            // DEFINE THE PATH WHERE WE WANT TO SAVE THE FILES.
            string sPath = "";
            //sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploadmultiple/");
            sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/UploadedLogos/");

            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;

            // CHECK THE FILE COUNT.
            for (int iCnt = 0; iCnt <= hfc.Count - 1; iCnt++)
            {
                System.Web.HttpPostedFile hpf = hfc[iCnt];

                if (hpf.ContentLength > 0)
                {
                    if (hpf.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        // CHECK IF THE SELECTED FILE(S) ALREADY EXISTS IN FOLDER. (AVOID DUPLICATE)
                        if (!File.Exists(sPath + Path.GetFileName(hpf.FileName)))
                        {
                            // SAVE THE FILES IN THE FOLDER.
                            hpf.SaveAs(sPath + Path.GetFileName(hpf.FileName));
                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(hpf.FileName, sPath, sPath + Path.GetFileName(hpf.FileName));

                            iUploadedCnt = iUploadedCnt + 1;

                            //unzip file
                            string startPath = sPath;
                            string zipPath = sPath + (hpf.FileName);
                            string extractPath = sPath;
                            //System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!File.Exists(extractPath + entry.FullName))
                                        {

                                            ZipFile.ExtractToDirectory(zipPath, extractPath);


                                        }
                                    }
                                }
                            }

                            File.Delete(zipPath);


                            //try
                            //{
                            //    string[] filePaths = Directory.GetFiles(@"E:\Inetpub\WebFortis\www\AnnotationFiles\");
                            //    foreach (string filePath in filePaths)
                            //    {
                            //         File.Delete(filePath);
                            //    }
                            //}
                            //catch
                            //{

                            //}



                        }
                    }
                }
            }


            // RETURN A MESSAGE (OPTIONAL).
            if (iUploadedCnt > 0)
            {
                return iUploadedCnt + " Files Uploaded Successfully";
            }
            else
            {
                return "Upload Failed";
            }
        }

    }
}
