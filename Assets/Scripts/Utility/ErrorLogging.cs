using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net.Mail;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class ErrorLogging : MonoBehaviour {
    public static bool allowInputLogging = false;
    private static StreamWriter outputStream;

    public InputField emailAddr, probdesc;
    public Toggle screen, log;

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

    [Header("Email addresses to which email reports should be addressed to")]
    public string[] sendTo;
    [Header("Email addresses to which email reports should be CC'd to")]
    public string[] ccTo;

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        Application.logMessageReceived += HandleLogging;
    }

    /// <summary>
    /// Called immediately when the Component's GameObject is disable
    /// </summary>
    void OnDisable() {
        Application.logMessageReceived -= HandleLogging;
    }

    public void SendEmail() {
        if(!screen.isOn && (!log.isOn || !allowInputLogging) && string.IsNullOrEmpty(probdesc.text)) {
            ErrorText.inst.DispError("Message not sent; without any details, that email would be useless.");
        } else {
            ErrorText.inst.DispInfo("Starting to send message...");
            StartCoroutine(BeginSend(new MailMessage()));
        }
    }

    public IEnumerator BeginSend(MailMessage email) {

        email.From = new MailAddress("report-noreply@star1889.com");
        foreach(string alpha in sendTo) {
            email.To.Add(alpha);
        }
        foreach(string alpha in ccTo) {
            email.CC.Add(alpha);
        }

        email.Subject = "A Problem Has Been Reported (Phaser Configurator)";

        System.Text.StringBuilder bodyBuilder = new System.Text.StringBuilder();
        bodyBuilder.Append("A problem has been reported on the Phaser Configurator by the user ");
        bodyBuilder.Append(System.Environment.UserName);
        bodyBuilder.Append(" from the domain ");
        bodyBuilder.Append(System.Environment.UserDomainName);
        bodyBuilder.Append(" on the machine ");
        bodyBuilder.Append(System.Environment.MachineName);
        bodyBuilder.Append(".");

        if(!string.IsNullOrEmpty(emailAddr.text)) {
            email.ReplyTo = new MailAddress(emailAddr.text);

            bodyBuilder.Append("  They gave an email with which to respond to: ");
            bodyBuilder.Append(emailAddr.text);
        }

        bodyBuilder.Append("\n\n");

        if(!string.IsNullOrEmpty(probdesc.text)) {
            bodyBuilder.Append("The user had given this problem description: \n    ");
            bodyBuilder.Append(probdesc.text);
        } else {
            bodyBuilder.Append("The user had given no problem description.");
        }

        email.Body = bodyBuilder.ToString();

        MemoryStream screenMem = null;
        if(screen.isOn) {
            Texture2D tex = new Texture2D(Screen.width, Screen.height);
            yield return new WaitForEndOfFrame();
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            screenMem = new MemoryStream(tex.EncodeToPNG());

            email.Attachments.Add(new Attachment(screenMem, "err.png", "image/png"));
        }

        if(log.isOn && outputStream != null) {
            outputStream.Flush();
            outputStream.BaseStream.Seek(0, SeekOrigin.Begin);
            email.Attachments.Add(new Attachment(outputStream.BaseStream, "log.txt", System.Net.Mime.MediaTypeNames.Text.Plain));
        }

        SmtpClient outbox = new SmtpClient("smtpout.secureserver.net", 80);
        outbox.DeliveryMethod = SmtpDeliveryMethod.Network;
        outbox.Credentials = (ICredentialsByHost)new NetworkCredential("christophercheng@star1889.com", "christopher");
        outbox.EnableSsl = false;
        outbox.Timeout = 2000;
        outbox.UseDefaultCredentials = false;

        try {
            outbox.Send(email);
            ErrorText.inst.DispInfo("Your report has been successfully sent.");

        } catch(System.Exception ex) {
            Debug.LogException(ex);
            ErrorText.inst.DispError("Could not send report email: " + ex.Message);
        } finally {
            email.Dispose();

            if(screenMem != null) screenMem.Dispose();
        }

        yield return null;
    }

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

    public static void LogInput(string inputDesc) {
        if(!allowInputLogging) return;

        outputStream.WriteLine("[Input] " + inputDesc);
    }
}
