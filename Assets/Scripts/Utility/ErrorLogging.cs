using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net.Mail;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

/// <summary>
/// Component used to capture logging information (and send it off when asked)
/// </summary>
public class ErrorLogging : MonoBehaviour {
    /// <summary>
    /// Whether or not to be logging input
    /// </summary>
    public static bool allowInputLogging = false;
    /// <summary>
    /// The stream where logging information is output
    /// </summary>
    private static StreamWriter outputStream;

    /// <summary>
    /// The field the user optionally places their Respond To address
    /// </summary>
    public InputField emailAddr;
    /// <summary>
    /// The field the user places a description of the issue they're having
    /// </summary>
    public InputField probdesc;
    /// <summary>
    /// The Toggle indicating whether or not they want to send a screenshot
    /// </summary>
    public Toggle screen;
    /// <summary>
    /// The Toggle indicating whether or not they want to send a copy of the log
    /// </summary>
    public Toggle log;

    /// <summary>
    /// Gets or sets a value indicating whether the user wants input logged.
    /// </summary>
    public static bool logInput {
        get {
            return allowInputLogging;
        }
        set {
            allowInputLogging = value;

            if(value) {
                outputStream = new StreamWriter(new MemoryStream(4*1024*1024));
                outputStream.Write("Begin Log: " + System.DateTime.Now.ToString("yyyy-MM-dd @ hh:mm:ss tt") + "\n");
            } else {
                if(outputStream != null) outputStream.Dispose();
                outputStream = null;
            }
        }
    }

    /// <summary>
    /// The email addresses to specifically address report emails to
    /// </summary>
    [Header("Email addresses to which email reports should be addressed to")]
    public string[] sendTo;
    /// <summary>
    /// The email addresses that should recieve emails, but aren't being addressed specifically
    /// </summary>
    [Header("Email addresses to which email reports should be CC'd to")]
    public string[] ccTo;

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        Application.logMessageReceived += HandleLogging;
    }

    /// <summary>
    /// Called immediately when the Component's GameObject is disabled
    /// </summary>
    void OnDisable() {
        Application.logMessageReceived -= HandleLogging;
    }

    /// <summary>
    /// Sends a report email.
    /// </summary>
    public void SendEmail() {
        if(!screen.isOn && (!log.isOn || !allowInputLogging) && string.IsNullOrEmpty(probdesc.text)) {
            ErrorText.inst.DispError("Message not sent; without any details, that email would be useless.");
        } else {
            ErrorText.inst.DispInfo("Starting to send message...");
            StartCoroutine(BeginSend());
        }
    }

    /// <summary>
    /// Coroutine.  Sends the report email.
    /// </summary>
    public IEnumerator BeginSend() {
        MailMessage email = new MailMessage();

        #region Email Metadata
        email.From = new MailAddress("report-noreply@star1889.com");
        foreach(string alpha in sendTo) {
            email.To.Add(alpha);
        }
        foreach(string alpha in ccTo) {
            email.CC.Add(alpha);
        }

        email.Subject = "A Problem Has Been Reported (Phaser Configurator)"; 
        #endregion

        #region Email Body
        System.Text.StringBuilder bodyBuilder = new System.Text.StringBuilder();
        #region Get Machine Information
        bodyBuilder.Append("A problem has been reported on the Phaser Configurator by the user ");
        bodyBuilder.Append(System.Environment.UserName);
        bodyBuilder.Append(" from the domain ");
        bodyBuilder.Append(System.Environment.UserDomainName);
        bodyBuilder.Append(" on the machine ");
        bodyBuilder.Append(System.Environment.MachineName);
        bodyBuilder.Append("."); 
        #endregion

        #region If Respond To address given, add it in
        if(!string.IsNullOrEmpty(emailAddr.text)) {
            email.ReplyTo = new MailAddress(emailAddr.text);

            bodyBuilder.Append("  They gave an email with which to respond to: ");
            bodyBuilder.Append(emailAddr.text);
        } 
        #endregion

        bodyBuilder.Append("\n\n");

        #region If problem description given, add it in
        if(!string.IsNullOrEmpty(probdesc.text)) {
            bodyBuilder.Append("The user had given this problem description: \n    ");
            bodyBuilder.Append(probdesc.text);
        } else {
            bodyBuilder.Append("The user had given no problem description.");
        } 
        #endregion

        email.Body = bodyBuilder.ToString(); 
        #endregion

        #region Add a screencapture as an attachment if desired
        MemoryStream screenMem = null;
        if(screen.isOn) {
            Texture2D tex = new Texture2D(Screen.width, Screen.height);
            yield return new WaitForEndOfFrame();
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            screenMem = new MemoryStream(tex.EncodeToPNG());

            email.Attachments.Add(new Attachment(screenMem, "err.png", "image/png"));
        } 
        #endregion

        #region Add log as an attachment if desired
        if(log.isOn && outputStream != null) {
            outputStream.Flush();
            outputStream.BaseStream.Seek(0, SeekOrigin.Begin);
            email.Attachments.Add(new Attachment(outputStream.BaseStream, "log.txt", System.Net.Mime.MediaTypeNames.Text.Plain));
        } 
        #endregion

        #region Prepare to send email
        SmtpClient outbox = new SmtpClient("smtpout.secureserver.net", 80);
        outbox.DeliveryMethod = SmtpDeliveryMethod.Network;
        outbox.Credentials = (ICredentialsByHost)new NetworkCredential("christophercheng@star1889.com", "christopher");
        outbox.EnableSsl = false;
        outbox.Timeout = 2000;
        outbox.UseDefaultCredentials = false; 
        #endregion

        try {
            outbox.Send(email);  // Actually send the email
            ErrorText.inst.DispInfo("Your report has been successfully sent."); // Let user know email was sent
        } catch(System.Exception ex) { // Could not send email, problem occurred
            Debug.LogException(ex);
            ErrorText.inst.DispError("Could not send report email: " + ex.Message); // Let user know email was not sent
        } finally {
            email.Dispose(); // Trash unnecessary email

            if(screenMem != null) screenMem.Dispose(); // Trash screencap information
        }

        yield return null;
    }

    /// <summary>
    /// Handles the logging.  Called when something gets pushed to Debug.Log*
    /// </summary>
    /// <param name="logDesc">The log description.</param>
    /// <param name="stackTrace">The stack trace of the log.</param>
    /// <param name="type">The type of logging occurring.</param>
    public void HandleLogging(string logDesc, string stackTrace, LogType type) {
        if(!allowInputLogging) return;
        switch(type) {
            case LogType.Assert:
                outputStream.Write("[ASSERT] ");
                break;
            case LogType.Error:
                outputStream.Write("[ERROR] ");
                break;
            case LogType.Exception:
                outputStream.Write("[EXCEP] ");
                break;
            case LogType.Log:
                outputStream.Write("[Log] ");
                break;
            case LogType.Warning:
                outputStream.Write("[Warn] ");
                break;
            default:
                break;
        }

        outputStream.WriteLine(logDesc);
        outputStream.WriteLine("---@ " + stackTrace);
    }

    /// <summary>
    /// Logs input.
    /// </summary>
    /// <param name="inputDesc">The description of the input occurring.</param>
    public static void LogInput(string inputDesc) {
        if(!allowInputLogging) return;

        outputStream.WriteLine("[Input] " + inputDesc);
    }
}
