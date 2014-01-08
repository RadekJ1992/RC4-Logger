using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RC4_Logger.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace RC4_Logger
{
    /// <summary>
    /// Klasa głównego okna aplikacji
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // Constants
        const int REMOTE_PORT = 10100;  // The Echo protocol uses port 7 in this sample
        int ckey;
        public static SocketClient client;
        bool connected;
        Thread loggerThread;
        /// <summary>
        /// Obsługa wciśnięcia przycisku logowania się na serwerze
        /// </summary>
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!connected)
            {
                // Clear the log 
                ClearLog();
                // Instantiate the SocketClient
                client = new SocketClient();

                // Attempt to connect to the echo server
                Log(String.Format("Connecting to server encryptserver.cloudapp.net over port 10100 ..."), true);
                client.Connect("encryptserver.cloudapp.net", REMOTE_PORT);
                //client.Receive();
                string result = client.Send("logger\n");
                Log(result, false);
                if (result == "Success")
                {
                    connected = true;
                    btnLogin.Content = "Log out!";
                    loggerThread = new Thread(logger);
                    loggerThread.Start();
                }
                else
                {
                    connected = false;
                    btnLogin.Content = "Log in!";
                }
            }
            else
            {
                loggerThread.Abort();
                client.Send("quit\n");
                Log("Logged out!", false);
                connected = false;
                btnLogin.Content = "Log in!";
                client = null;
            }
        }
        /// <summary>
        /// Metoda wypisująca komunikaty z innych wątków w polu tekstowym
        /// </summary>
        private void logger() {
            while (true)
            {
                if (client !=null) {
                String rec = client.Receive();
                DispatchInvoke(() =>
                    {
                        txtOutput.Text += Environment.NewLine + "<<" + rec;
                    });
                }
            }
        }
        /// <summary>
        /// Metoda służąca do obsługi wielowątkowości
        /// </summary>
        /// <param name="a">akcja do wykonania</param>
        public void DispatchInvoke(Action a)
        {
#if SILVERLIGHT
            if (Dispatcher == null) {
                a();
            } else {
                Dispatcher.BeginInvoke(a);
            }
#else
            if ((Dispatcher != null) && (!Dispatcher.HasThreadAccess))
            {
                Dispatcher.InvokeAsync(
                                Windows.UI.Core.CoreDispatcherPriority.Normal,
                                (obj, invokedArgs) => { a(); }, this, null
                    );
            }
            else
                a();
#endif
        }

        /// <summary>
        /// Metoda wypisująca komunikaty w polu tekstowym
        /// </summary>
        /// <param name="message">Wiadomość do wyświetlenia</param>
        /// <param name="isOutgoing">True gdy z klienta do serwera
        /// False gdy z serwera do klienta
        /// </param>
        private void Log(string message, bool isOutgoing)
        {
            string direction = (isOutgoing) ? ">> " : "<< ";
            txtOutput.Text += Environment.NewLine + direction + message;
        }

        /// <summary>
        /// Wyczyszczenie pola tekstowego
        /// </summary>
        private void ClearLog()
        {
            txtOutput.Text = String.Empty;
        }
        /// <summary>
        /// Konstruktor
        /// </summary>
        public MainPage()
        {
            
            InitializeComponent();
            connected = false;
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }
       /// <summary>
       /// Metoda wysyłająca wiadomość do serwera i wylogowująca się
       /// </summary>
       public static void logOut() {
           if (client != null)
           {
               client.Send("quit\n");
           }
       }
  }

    
}