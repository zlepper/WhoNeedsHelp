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
    public class ChannelsController : ApiController
    {
        private HelpContext db = new HelpContext();

        // GET: api/Channels
        public IQueryable<Channel> GetChannels()
        {
            return db.Channels;
        }

        // GET: api/Channels/5
        [ResponseType(typeof(Channel))]
        public IHttpActionResult GetChannel(int id)
        {
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return NotFound();
            }

            return Ok(channel);
        }

        // PUT: api/Channels/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutChannel(int id, Channel channel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != channel.Id)
            {
                return BadRequest();
            }

            db.Entry(channel).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChannelExists(id))
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

        // POST: api/Channels
        [ResponseType(typeof(Channel))]
        public IHttpActionResult PostChannel(Channel channel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Channels.Add(channel);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = channel.Id }, channel);
        }

        // DELETE: api/Channels/5
        [ResponseType(typeof(Channel))]
        public IHttpActionResult DeleteChannel(int id)
        {
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return NotFound();
            }

            db.Channels.Remove(channel);
            db.SaveChanges();

            return Ok(channel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ChannelExists(int id)
        {
            return db.Channels.Count(e => e.Id == id) > 0;
        }
    }
}