using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Paster.Models;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Text;

namespace Paster.Controllers
{
    public class PastesController : Controller
    {
        private PasteContext db = new PasteContext();

        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void CleanUp()
        {
            var pastes = from p in db.Pastes
                         where p.Expires < DateTime.Now
                         select p;

            db.Pastes.RemoveRange(pastes);
            db.SaveChanges();

        }

        private bool limit_exceeded()
        {
            DateTime checkDate = DateTime.Now;
            checkDate.AddDays(-1);

            var pastes = (from p in db.Pastes
                          where p.Date >= checkDate
                          where p.Ip == Request.UserHostAddress
                          select p).Count();

            //return (pastes > limit);

            if (pastes > Int32.Parse(ConfigurationManager.AppSettings["limit_day"]))
            {
                return true;
            }

            checkDate = DateTime.Now;
            checkDate.AddHours(-1);

            pastes = (from p in db.Pastes
                          where p.Date >= checkDate
                          where p.Ip == Request.UserHostAddress
                          select p).Count();

            //return (pastes > limit);

            if (pastes > Int32.Parse(ConfigurationManager.AppSettings["limit_hour"]))
            {
                return true;
            }

            return false;

        }

        private string generate_ident()
        {
	        bool exists = true;
            string ident = "";
            while ( exists )
	        {
                // generate identifier. must have enough entropy (so rand()) but since we can't trust php's
                // rand, we hash it together with a site-specific secret. then to compress the url, we base64
                // encode instead of hex encode the result. Furthermore, + and / might give trouble in GET
                // parameters, so we replace those.
                ident = RandomString(12);

                var pastes = (from p in db.Pastes
                             where p.Ident == ident
                             select p).Count();
			    if(pastes == 0)
                {
                    exists = false;
                }
            }
            return ident;
        }

        public ActionResult Create()
        {
            CleanUp();

            ViewBag.page_title = ConfigurationManager.AppSettings["page_title"];
            ViewBag.limit_hour = ConfigurationManager.AppSettings["limit_hour"];
            ViewBag.limit_day = ConfigurationManager.AppSettings["limit_day"];
            ViewBag.ttl_min = ConfigurationManager.AppSettings["ttl_min"];
            ViewBag.ttl_max = ConfigurationManager.AppSettings["ttl_max"];
            ViewBag.paste_max_chars = ConfigurationManager.AppSettings["paste_max_chars"];
            ViewBag.server_name = ConfigurationManager.AppSettings["server_name"];
            return View();
        }
        
        [HttpPost]
        public ActionResult Create([Bind(Include = "Content,Ttl")] PostModel post)
        {
            CleanUp();

            ViewBag.page_title = ConfigurationManager.AppSettings["page_title"];
            ViewBag.limit_hour = ConfigurationManager.AppSettings["limit_hour"];
            ViewBag.limit_day = ConfigurationManager.AppSettings["limit_day"];
            ViewBag.ttl_min = ConfigurationManager.AppSettings["ttl_min"];
            ViewBag.ttl_max = ConfigurationManager.AppSettings["ttl_max"];
            ViewBag.paste_max_chars = ConfigurationManager.AppSettings["paste_max_chars"];
            ViewBag.server_name = ConfigurationManager.AppSettings["server_name"];

            if (!ModelState.IsValid)
            {
                ViewBag.errmsg = "You did not set all parameters";
                return View("Error");
            }
            if(post.Content.Length > Int32.Parse(ConfigurationManager.AppSettings["paste_max_chars"]))
            {
                ViewBag.errmsg = String.Format("Your paste exceeds the max limit of {0}", ConfigurationManager.AppSettings["paste_max_chars"]);
                return View("Error");
            }

            if (post.Ttl < Int32.Parse(ConfigurationManager.AppSettings["ttl_min"]))
            {
                post.Ttl = Int32.Parse(ConfigurationManager.AppSettings["ttl_min"]);
            }
            else if (post.Ttl > Int32.Parse(ConfigurationManager.AppSettings["ttl_max"]))
            {
                post.Ttl = Int32.Parse(ConfigurationManager.AppSettings["ttl_max"]);
            }
            
            bool limit = limit_exceeded();
            if (limit)
            {
                ViewBag.errmsg = "You have reached your throttle limit, try again later.";
                return View("Error");
            }
   
            string mime_type = "text/plain";

            byte[] data = Encoding.UTF8.GetBytes(post.Content);
            string decodedString = Convert.ToBase64String(data);

            Pastes paste = new Pastes();
            paste.Ident = generate_ident();
            paste.Ip = Request.UserHostAddress;
            paste.Date = DateTime.Now;
            paste.Text = decodedString;
            paste.MimeType = mime_type;
            paste.Expires = DateTime.Now.AddSeconds(post.Ttl);

            db.Pastes.Add(paste);
            db.SaveChanges();
            return RedirectToRoute("Paste", new { ident = paste.Ident });
        }

        // GET: Pastes/Edit/5
        public ActionResult Show(string ident)
        {
            CleanUp();

            if (ident == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pastes pastes = db.Pastes.Find(ident);
            if (pastes == null)
            {
                return HttpNotFound();
            }
            Response.ContentType = pastes.MimeType;
            byte[] data = Convert.FromBase64String(pastes.Text);
            string decodedString = Encoding.UTF8.GetString(data);
            ViewBag.text = decodedString;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
