using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WhoNeedsHelp;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Models;

namespace WhoNeedsHelp.Controllers
{
    public class LocalesController : ApiController
    {
        private HelpContext db = new HelpContext();

        // GET: api/Locales
        public IQueryable<Locale> GetLocales()
        {
            return db.Locales;
        }

        // GET: api/Locales/5
        [ResponseType(typeof(Locale))]
        public IHttpActionResult GetLocale(int id)
        {
            Locale locale = db.Locales.Find(id);
            if (locale == null)
            {
                return NotFound();
            }

            return Ok(locale);
        }

        // PUT: api/Locales/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutLocale(int id, Locale locale)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != locale.Id)
            {
                return BadRequest();
            }

            db.Entry(locale).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocaleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Locales
        [ResponseType(typeof(Locale))]
        public IHttpActionResult PostLocale(Locale locale)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Locales.Add(locale);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = locale.Id }, locale);
        }

        // DELETE: api/Locales/5
        [ResponseType(typeof(Locale))]
        public IHttpActionResult DeleteLocale(int id)
        {
            Locale locale = db.Locales.Find(id);
            if (locale == null)
            {
                return NotFound();
            }

            db.Locales.Remove(locale);
            db.SaveChanges();

            return Ok(locale);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LocaleExists(int id)
        {
            return db.Locales.Count(e => e.Id == id) > 0;
        }
    }
}