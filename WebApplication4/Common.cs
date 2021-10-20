using System;
using System.Data;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Mail;
using System.Net;
using Spire.Pdf;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text.RegularExpressions;

namespace WA_EPSDS
{
    public class Common
    {
        public enum MessageType { SuccessMessage, ErrorMessage };
        public bool GenerateReport(String RPT_NAME, DataTable dt, String ExportType, String FileName, string GuidenceFilePath)
        {
            try
            {
                ReportDocument rptDoc = new ReportDocument();
                rptDoc.FileName = RPT_NAME;
                if (dt == null)
                {
                }
                else
                {
                    rptDoc.SetDataSource(dt);
                }
                if (ExportType.ToUpper() == "PDF")
                {
                    rptDoc.PrintOptions.PaperSize = PaperSize.PaperA3;
                    rptDoc.ExportToDisk(ExportFormatType.PortableDocFormat, FileName.Replace(".pdf", "_1.pdf"));
                    MergePDF(FileName.Replace(".pdf", "_1.pdf"), GuidenceFilePath, FileName);
                    File.Delete(FileName.Replace(".pdf", "_1.pdf"));
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static void MergePDF(string File1, string File2, string outputPdfPath)
        {
            string[] fileArray = new string[3];
            fileArray[0] = File1;
            fileArray[1] = File2;

            PdfReader reader = null;
            Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;

            sourceDocument = new Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();
            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {
                int pages = TotalPageCount(fileArray[f]);
                reader = new PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }
                reader.Close();
            }
            //save the output file  
            sourceDocument.Close();
        }
        private static int TotalPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }
        static string NullToString(object Value)
        {
            return Value == null ? "" : Value.ToString();
        }
        public void SendEmail(string EmailTo, string CC, string BCC, string Subject, string Body, string Attachment_Yes_no, [Optional] FileInfo fileInfo)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                string UserName = NullToString(System.Configuration.ConfigurationManager.AppSettings["UserName"]);
                string UserPassword = NullToString(System.Configuration.ConfigurationManager.AppSettings["UserPassword"]);
                string FromUserEmail = NullToString(System.Configuration.ConfigurationManager.AppSettings["FromUserEmail"]);
                string EmailCC = NullToString(System.Configuration.ConfigurationManager.AppSettings["EmailCC"]);
                message.From = new MailAddress(FromUserEmail);
                message.To.Add(new MailAddress(EmailTo));
                if (CC != "")
                {
                    message.CC.Add(new MailAddress(CC));
                }
                if (EmailCC != "")
                {
                    message.CC.Add(new MailAddress(EmailCC));
                }
                if (BCC != "")
                {
                    message.Bcc.Add(new MailAddress(BCC));
                }
                if (fileInfo != null)
                {
                    Attachment attachment = new Attachment(fileInfo.FullName);
                    message.Attachments.Add(attachment);
                }
                message.Subject = Subject;
                message.IsBodyHtml = true;
                message.Body = Body;
                smtp.Port = 25;
                smtp.Host = "mail.xyz.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(UserName, UserPassword);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}