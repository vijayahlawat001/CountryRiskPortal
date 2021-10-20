using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WA_EPSDS;

namespace CountryRiskPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        static string NullToString(object Value)
        {
            return Value == null ? "" : Value.ToString();
        }
        [HttpPost]
        public ActionResult GetUserName()
        {
            try
            {
                string Name = uEmail(User.Identity.Name, "UserName");
                return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject(Name) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Index(List<Model.Index> listCollections, string TravellerName, string DepartureDate, string Country, string RiskLevel)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Type");
                dt.Columns.Add("ID");
                dt.Columns.Add("Description");
                dt.Columns.Add("Checked");
                dt.Columns.Add("Comments");
                foreach (Model.Index list in listCollections)
                {
                    DataRow r = dt.NewRow();
                    r["Type"] = list.Type.ToString();
                    r["ID"] = list.ID;
                    r["Description"] = list.Description.ToString();
                    r["Checked"] = list.Checked.ToString();
                    r["Comments"] = NullToString(list.Comments);
                    dt.Rows.Add(r);
                }
                string data = string.Empty;
                dt.TableName = "DATA";
                using (StringWriter sw = new StringWriter())
                {
                    dt.WriteXml(sw);
                    data = sw.ToString();
                }
                string name = User.Identity.Name;
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "SaveTravelCheckList";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TravellerName", TravellerName);
                cmd.Parameters.AddWithValue("@DepartureDate", DepartureDate);
                cmd.Parameters.AddWithValue("@Country", Country);
                cmd.Parameters.AddWithValue("@RiskLevel", RiskLevel);
                cmd.Parameters.AddWithValue("@CreatedBy", name);
                cmd.Parameters.AddWithValue("@data", data);
                dt = new DataTable();
                dt = (new DBAccess()).GetRcdSetByCmdTrans(cmd);
                if (dt.Rows.Count > 0)
                {
                    Common common = new Common();
                    string Email = uEmail(User.Identity.Name);
                    string FileName = Server.MapPath("~/PDF/TravelCheckList" + name + "_" + DateTime.Now.ToString("ddMMyyhhmmss") + ".pdf");
                    common.GenerateReport(Server.MapPath("~/TravelCheckList.rpt"), dt, "PDF", FileName, Server.MapPath("~/TravelCheckList2.pdf"));
                    FileInfo f = new FileInfo(FileName);
                    string BossEmail = uEmail(User.Identity.Name, "Boss");
                    common.SendEmail(Email, BossEmail, "", "Travel Check List for Departure Date " + DepartureDate, "Dear user<br>PFA travel check list as request for your departure dated " + DepartureDate + " To " + Country + " with reference No : " + dt.Rows[0]["TCLNo"] + " <br> Regards<br>Country Risk Portal Team", "Yes", f);
                    //f.Delete();
                    return Json(new { success = true, responseText = Newtonsoft.Json.JsonConvert.SerializeObject("TCL has been Submitted Successfully. Check your email for document.") }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = false, responseText = "There is an error in saving TCL" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private string uEmail(string uid, string Value = "Mail")
        {
            DirectorySearcher searcher = new DirectorySearcher();
            searcher.Filter = string.Format("sAMAccountName={0}", uid);
            SearchResult user = searcher.FindOne();

            if (Value == "UserName")
            {
                return user.Properties["Name"][0].ToString();
            }
            else if (Value == "Boss")
            {
                searcher = new DirectorySearcher();
                if (NullToString(user.Properties["manager"][0]) != "")
                {
                    searcher.Filter = string.Format("distinguishedname={0}", user.Properties["manager"][0]);
                    SearchResult Boss = searcher.FindOne();
                    return Boss.Properties["mail"][0].ToString();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return user.Properties["mail"][0].ToString();
            }
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}