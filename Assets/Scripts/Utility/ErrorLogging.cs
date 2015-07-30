using UnityEngine;
using System.Collections;
using System.Net.Mail;

public class ErrorLogging : MonoBehaviour {
    public string[] sendTo;
    public string[] ccTo;


    public void SendEmail() {
        MailMessage email = new MailMessage();

        email.From = new MailAddress(System.Environment.UserDomainName + "@star1889.com");
        foreach(string alpha in sendTo) {
            email.To.Add(alpha);
        }
        foreach(string alpha in ccTo) {
            email.CC.Add(alpha);
        }

        email.Subject = "An Error Has Been Reported (1000 Configurator)";
        email.Body = "";
    }
}
