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
using System.Configuration.Internal;
using System.IO;
using System.Net.Mail;
using EmailApp.ViewModels;

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
        // GET: Inbox View
        public IActionResult Inbox(int pageNumber, int id)
        {
            var inboxView = _emailService.GetInbox(pageNumber, id);
            return View(inboxView);
        }

        // GET:  message view
        public IActionResult ViewEmail(int id,int pageNumber)
        {
            var emailView = _emailService.GetMessage(id);
            emailView.CurrentPageNuber = pageNumber;
            return View(emailView);
        }

        // POST:  Set as archived
        public IActionResult SetAsArchive(int id, int pageNumber)
        {
            var configId = _emailService.SetMessageAsArchived(id);
            return RedirectToAction(nameof(Inbox), new { PageNumber = pageNumber, id = configId });
        }

        // POST:  Set as deleted
        public IActionResult SetAsDeleted(int id, int pageNumber)
        {
            var configId = _emailService.SetMessageAsDeleted(id);
            return RedirectToAction(nameof(Inbox), new { PageNumber = pageNumber, id = configId });
        }

        // GET:  download attachment
        public IActionResult DownloadAttachment(int id,string file)
        {
            var attachmentView = _emailService.GetAttachment(id, file);
            if (attachmentView == null || attachmentView.FileStream == null)
                return BadRequest("file not find");

            // Return the attachment as a file download
            return File(attachmentView.FileStream, "application/octet-stream", attachmentView.FileName ?? attachmentView.ContentType);
        }
        #endregion

        #region Send Email
        // GET: Inbox View
        [HttpGet]
        public IActionResult Compose(int id)
        {
            var model = new SendEmailViewModel();
            model.ConfigId = id;
            return View(model);
        }

        // GET: Inbox View
        [HttpGet]
        public IActionResult EditDraft(int draftId)
        {
            if (draftId == null || _context.SendEmail == null)
            {
                return NotFound();
            }

            var sendEmail = _context.SendEmail.Find(draftId);
            if (sendEmail == null)
            {
                return NotFound();
            }            
            SendEmailViewModel viewModel = new SendEmailViewModel
            {
                Bcc = sendEmail.Bcc,
                Body = sendEmail.Body,
                Cc = sendEmail.Cc,
                ConfigId = sendEmail.ConfigId,                
                CreateOn = sendEmail.CreateOn,
                Subject = sendEmail.Subject,
                To = sendEmail.To
            };
            return View("Compose", viewModel);
        }

        [HttpPost]
        public IActionResult Compose(SendEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Send the email
                    var id=_emailService.SendEmail(model, model.ConfigId);

                    // Redirect to a "success" page or show a success message
                    return RedirectToAction("Details", "SendEmails", new { id, model.ConfigId });
                }
                catch (Exception ex)
                {                    
                    ModelState.AddModelError("", "An error occurred while sending the email. Please try again later.");
                }
            }

            // If we get here, the model state was invalid, so redisplay the form with errors
            return View("Compose", model);
        }
        #endregion

        private bool EmailConfigExists(int id)
        {
          return (_context.EmailConfig?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
