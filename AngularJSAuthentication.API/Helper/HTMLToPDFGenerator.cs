using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
//using Pechkin;
using NLog;

namespace AngularJSAuthentication.API.WebAPIHelper
{
    public static class HTMLToPDFGenerator
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Convert HTML to PDF
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filename"></param>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public static string Convert(string directory, string filename, string htmlString)
        {
            string generatedFile = string.Empty;
            //try
            //{
            //    byte[] pdfBuffer = new SimplePechkin(new GlobalConfig()).Convert(htmlString);

            //    if (!Directory.Exists(HttpContext.Current.Server.MapPath(directory)))
            //        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(directory));
            //    string filenamepath = HttpContext.Current.Server.MapPath(directory + filename);
            //    if (ByteArrayToFile(filenamepath, pdfBuffer))
            //    {
            //        generatedFile = filenamepath;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.Error("Error in Convert Method: " + ex.Message);
            //}

            return generatedFile;
        }

        /// <summary>
        /// Writes a byte array (format returned by SimplePechkin) into a file
        /// </summary>
        /// <param name="_FileName"></param>
        /// <param name="_ByteArray"></param>
        /// <returns></returns>
        private static bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                // Open file for reading
                FileStream _FileStream = new FileStream(_FileName, FileMode.Create, FileAccess.Write);
                // Writes a block of bytes to this stream using data from  a byte array.
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

                // Close file stream
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                logger.Error("Exception caught in process while trying to save pdf : " + _Exception.ToString());
            }

            return false;
        }
    }
}