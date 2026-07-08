using trix_site.Models;

namespace trix_site.Services
{
    /// <summary>
    /// Abstraction for handling a marketing-site contact submission.
    ///
    /// IMPLEMENTATION NOTE FOR AMI:
    /// Since app.12trix.com already has working DB + email infrastructure,
    /// the cleanest setup is to put the *real* implementation of this interface
    /// in a shared class library (e.g. "Trix12.Shared") that BOTH the app project
    /// and the co.il marketing project reference. The marketing controller then
    /// depends only on this interface — no cross-origin calls, no HTTP hop.
    ///
    /// The concrete implementation can reuse whatever the app already uses to
    /// send mail (SmtpClient / SendGrid / MailKit) and, if desired, log the
    /// lead to the existing database.
    /// </summary>
    public interface IMarketingContactService
    {
        /// <summary>
        /// Handles a validated contact submission: sends a notification email
        /// to Ami and (optionally) persists the lead. Returns true on success.
        /// </summary>
        Task<bool> HandleContactAsync(ContactViewModel model);
    }
}
