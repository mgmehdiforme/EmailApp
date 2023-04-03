using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmailApp.Data;
using EmailApp.DataModels;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using EmailApp.ViewModels;
using System.Drawing.Printing;

namespace EmailApp.Controllers
{
    public class SendEmailsController : Controller
    {
        private readonly EmailAppContext _context;

        public SendEmailsController(EmailAppContext context)
        {
            _context = context;
        }

        // GET: SendEmails
        public async Task<IActionResult> SentBox(int id, int pageNumber)
        {
            // Retrieve a paged list of sent emails
            var pageSize = 10;
            var emails = _context.SendEmail
                .Where(x=>!x.IsDraft)
                .OrderByDescending(e => e.CreateOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Count the total number of sent emails
            var totalEmails = _context.SendEmail.Count();

            // Calculate the total number of pages
            var totalPages = (int)Math.Ceiling((double)totalEmails / pageSize);

            // Create a view model for the paged list of sent emails
            var model = new SendEmailListViewModel
            {
                Emails = emails,
                CurrentPageNumber = pageNumber,
                TotalPages = totalPages
            };

            // Display the paged list of sent emails view
            return View(model);
        }

        public async Task<IActionResult> DraftBox(int id, int pageNumber)
        {
            // Retrieve a paged list of sent emails
            var pageSize = 10;
            var emails = _context.SendEmail
                .Where(x => x.IsDraft)
                .OrderByDescending(e => e.CreateOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Count the total number of sent emails
            var totalEmails = _context.SendEmail.Count();

            // Calculate the total number of pages
            var totalPages = (int)Math.Ceiling((double)totalEmails / pageSize);

            // Create a view model for the paged list of sent emails
            var model = new SendEmailListViewModel
            {
                Emails = emails,
                CurrentPageNumber = pageNumber,
                TotalPages = totalPages
            };

            // Display the paged list of sent emails view
            return View(model);
        }

        // GET: SendEmails/Details/5
        public async Task<IActionResult> Details(int? id, int ConfigId)
        {
            if (id == null || _context.SendEmail == null)
            {
                return NotFound();
            }

            var sendEmail = await _context.SendEmail
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sendEmail == null)
            {
                return NotFound();
            }

            return View(sendEmail);
        }


        // GET: SendEmails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SendEmail == null)
            {
                return NotFound();
            }

            var sendEmail = await _context.SendEmail.FindAsync(id);
            if (sendEmail == null)
            {
                return NotFound();
            }
            return View(sendEmail);
        }

        // GET: SendEmails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.SendEmail == null)
            {
                return NotFound();
            }

            var sendEmail = await _context.SendEmail
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sendEmail == null)
            {
                return NotFound();
            }

            return View(sendEmail);
        }

        // POST: SendEmails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SendEmail == null)
            {
                return Problem("Entity set 'EmailAppContext.SendEmail'  is null.");
            }
            var sendEmail = await _context.SendEmail.FindAsync(id);
            if (sendEmail != null)
            {
                _context.SendEmail.Remove(sendEmail);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SendEmailExists(int id)
        {
          return (_context.SendEmail?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
