using UnityEngine;
using System.Collections;
using System.Net.Mail;
using System.IO;

public class ErrorLogging : MonoBehaviour {
    public static bool allowInputLogging = false;
    private static StreamWriter outputStream;

    public static bool logInput {
        get {
            return allowInputLogging;
        }
        set {
            allowInputLogging = value;

            if(value) {
                outputStream = File.CreateText(Application.temporaryCachePath + "/log.txt");
            } else {
                if(File.Exists(Application.temporaryCachePath + "/log.txt")) File.Delete(Application.temporaryCachePath + "/log.txt");
                outputStream = null;
            }
        }
    }

    [Header("Email addresses to which email reports should be addressed to")]
    public string[] sendTo;
    [Header("Email addresses to which email reports should be CC'd to")]
    public string[] ccTo;

    void OnEnable() {
        Application.logMessageReceived += HandleLogging;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLogging;
    }

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

        System.Text.StringBuilder bodyBuilder = new System.Text.StringBuilder();
        bodyBuilder.Append("An error has been reported on the 1000 Configurator by the user ");
        bodyBuilder.Append(System.Environment.UserDomainName);
        bodyBuilder.Append(" on the machine ");
        bodyBuilder.Append(System.Environment.MachineName);
        bodyBuilder.Append(".\n\n");

        email.Body = bodyBuilder.ToString();
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
        outputStream.Write("\n");
        outputStream.Flush();
    }

    void Update() {
        if(!allowInputLogging) return;

        if(Input.anyKeyDown) {
            char pressed = Input.inputString[0];

            if((pressed >= 'a' && pressed <= 'z') || (pressed >= '0' && pressed <= '9')) {
                string modifiers = "" + pressed.ToString().ToUpper();

                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) modifiers = "Shift+" + modifiers;
                if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) modifiers = "Alt+" + modifiers;
                if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) modifiers = "Ctrl+" + modifiers;
                LogInput(modifiers);
            } else {
                switch(pressed) {
                    case '\b':
                        LogInput("Backspace");
                        break;
                    case '\n':
                    case '\r':
                        LogInput("Enter");
                        break;
                    case '-':
                    case '_':
                    case '=':
                    case '+':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case ';':
                    case ':':
                    case ',':
                    case '<':
                    case '.':
                    case '>':
                    case '/':
                    case '?':
                    case '\'':
                    case '\"':
                    case '|':
                    case '\\':
                    case '`':
                    case '~':
                        string modifiers = "" + pressed;

                        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) modifiers = "Shift+" + modifiers;
                        if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) modifiers = "Alt+" + modifiers;
                        if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) modifiers = "Ctrl+" + modifiers;
                        LogInput(modifiers);
                        break;
                    default:
                        if(Input.GetKeyDown(KeyCode.Tab)) {
                            LogInput("Tab");
                        } else if(Input.GetKeyDown(KeyCode.Insert)) {
                            LogInput("Insert");
                        } else if(Input.GetKeyDown(KeyCode.Delete)) {
                            LogInput("Delete");
                        } else if(Input.GetKeyDown(KeyCode.Home)) {
                            LogInput("Home");
                        } else if(Input.GetKeyDown(KeyCode.End)) {
                            LogInput("End");
                        } else if(Input.GetKeyDown(KeyCode.PageUp)) {
                            LogInput("Page Up");
                        } else if(Input.GetKeyDown(KeyCode.PageDown)) {
                            LogInput("Page Down");
                        } else if(Input.GetKeyDown(KeyCode.Escape)) {
                            LogInput("Escape");
                        }
                        break;
                }
            }
        }
    }

    public static void LogInput(string inputDesc) {
        if(!allowInputLogging) return;

        outputStream.WriteLine("[Input] " + inputDesc + "\n");
        outputStream.Flush();
    }
}
