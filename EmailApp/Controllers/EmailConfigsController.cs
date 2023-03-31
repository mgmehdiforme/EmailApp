using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmailApp.Data;
using EmailApp.DataModels;
using EmailApp.Services;
using MailKit.Search;

namespace EmailApp.Controllers
{
    public class EmailConfigsController : Controller
    {
        private readonly EmailAppContext _context;
        private readonly IEmailService _emailService;

        public EmailConfigsController(EmailAppContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        #region Crud EmailConfig

        // GET: EmailConfigs
        public async Task<IActionResult> Index()
        {
            return _context.EmailConfig != null ?
                        View(await _context.EmailConfig.ToListAsync()) :
                        Problem("Entity set 'EmailAppContext.EmailConfig'  is null.");
        }

        // GET: EmailConfigs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EmailConfig == null)
            {
                return NotFound();
            }

            var emailConfig = await _context.EmailConfig
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emailConfig == null)
            {
                return NotFound();
            }

            return View(emailConfig);
        }

        // GET: EmailConfigs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EmailConfigs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,From,SmtpServer,SmtpPort,UseSSLForSmtp,ImapServer,ImapPort,UseSSLForImap,UserName,Password")] EmailConfig emailConfig)
        {
            if (ModelState.IsValid)
            {
                _context.Add(emailConfig);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(emailConfig);
        }

        // GET: EmailConfigs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EmailConfig == null)
            {
                return NotFound();
            }

            var emailConfig = await _context.EmailConfig.FindAsync(id);
            if (emailConfig == null)
            {
                return NotFound();
            }
            return View(emailConfig);
        }

        // POST: EmailConfigs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,From,SmtpServer,SmtpPort,UseSSLForSmtp,ImapServer,ImapPort,UseSSLForImap,UserName,Password")] EmailConfig emailConfig)
        {
            if (id != emailConfig.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(emailConfig);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmailConfigExists(emailConfig.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(emailConfig);
        }

        // GET: EmailConfigs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EmailConfig == null)
            {
                return NotFound();
            }

            var emailConfig = await _context.EmailConfig
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emailConfig == null)
            {
                return NotFound();
            }

            return View(emailConfig);
        }

        // POST: EmailConfigs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EmailConfig == null)
            {
                return Problem("Entity set 'EmailAppContext.EmailConfig'  is null.");
            }
            var emailConfig = await _context.EmailConfig.FindAsync(id);
            if (emailConfig != null)
            {
                _context.EmailConfig.Remove(emailConfig);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Inbox

        // GET: EmailConfigs/Create
        public IActionResult Inbox(int id)
        {
            var config=_context.EmailConfig.First(x => x.Id == id);
            List<MimeKit.MimeMessage> list =_emailService.ReadMessagesImap(config,SearchQuery.All);
            return View(list);
        }
        #endregion

        private bool EmailConfigExists(int id)
        {
          return (_context.EmailConfig?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
