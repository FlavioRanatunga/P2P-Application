using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServer.Data;
using WebServer.Models;
using API_Library;

namespace WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly DBManager _context;

        public ClientsController(DBManager context)
        {
            _context = context;
            //_logger = logger;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients.ToListAsync();
        }

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        // PUT: api/Clients/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(int id, Client client)
        {
            if (id != client.Id)
            {
                return BadRequest();
            }

            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Clients
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = client.Id }, client);
        }

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("register")]
        public IActionResult RegisterClient([FromBody] ClientInfo clientInfo)
        {
            if (clientInfo == null || string.IsNullOrEmpty(clientInfo.IPAddress) || clientInfo.Port <= 0)
            {
                return BadRequest("Invalid client information.");
            }

            var exClient = _context.Clients.FirstOrDefault(c => c.Port == clientInfo.Port);
            if (exClient != null)
            {
                return BadRequest(new { message = "Port already registered." });
            }

            Client client = new Client
            {
                IPAddress = clientInfo.IPAddress,
                Port = clientInfo.Port,
            };

            _context.Clients.Add(client);
            _context.SaveChanges();
            return Ok(client);
        }

        [HttpPost("taskProcessingUpdate/{clientId}")]
        public IActionResult TaskProcessingUpdate(int clientId)
        {
            var client = _context.Clients.FirstOrDefault(c => c.Id == clientId);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            client.State = Status.Busy;

            _context.SaveChanges();

            return Ok("Client state updated successfully.");
        }

        [HttpPost("taskCompleteUpdate/{clientId}")]
        public IActionResult TaskCompleteUpdate(int clientId)
        {
            var client = _context.Clients.FirstOrDefault(c => c.Id == clientId);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            client.NoOfCompletedTasks += 1;
            client.State = Status.Idle;

            _context.SaveChanges();

            return Ok("Client state updated successfully.");
        }

        [HttpPost("updateStopped/{clientId}")]
        public async Task<IActionResult> UpdateClientStopped(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            client.State = Status.Stopped;

            await _context.SaveChangesAsync();

            return Ok("Client state updated successfully.");
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }


        public class ClientInfo
        {
            public string IPAddress { get; set; }
            public int Port { get; set; }
        }
    }
}
