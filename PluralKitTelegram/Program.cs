//Meow

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using ehoh = System.IO;

namespace PluralKitTelegram
{
#pragma warning disable 4014 // Allow for bot.SendChatAction to not be awaited
    // ReSharper disable FunctionNeverReturns
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
    // ReSharper disable CatchAllClause

    class Program
    {
        public static string s_cfgfile = Environment.CurrentDirectory + @"\botcfg.cfg"; // Main config
        public static string s_botdb = Environment.CurrentDirectory + @"\data\botdata.s3db"; // Main config
        private static readonly TelegramBotClient Bot = new TelegramBotClient(nwGrabString("botapitoken")); // Don't hard-code this.

        /// <summary>
        /// Grabs data from a specified key in your bots settings file.
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <returns>The value of the settings key.</returns>
        /// <remarks>Very BETA.</remarks>
        private static string nwGrabString(string key)
        {
            XmlDocument doc = new XmlDocument();
            string s;

            doc.Load(s_cfgfile);

            if (doc.SelectSingleNode("config/" + key) != null)
            {

                s = doc.SelectSingleNode("config/" + key).InnerText;
                return s;

            }
            else
            {

                //Console.WriteLine("Error!");
                return "Error";

            }
        }

        static void Main(string[] args)
        {
            var me = Bot.GetMeAsync().Result;
            Console.Title = "PluralKit Telegram Bot Test";

            DateTime dt = new DateTime(2016, 2, 2, 5, 30, 0);
            dt = DateTime.Now;

            // Do the title
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("----------------- PluralKit Telegram Bot Test -------------------");
            Console.WriteLine("-----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            // Events
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            // Do the initial starting routine, populate our settings file if it doesn't exist.
            nwInitialStuff(dt);

            Console.WriteLine(); // blank line

            Bot.StartReceiving(Array.Empty<UpdateType>());
            nwSystemCCWrite(dt.ToString(nwParseFormat(false)), $"Start listening for @{me.Username}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
            Bot.StopReceiving();
        }

        /// <summary>
        /// Returns a string to be used as part of a date time format.
        /// </summary>
        /// <param name="nodate">If true, don't return a date, false otherwise.</param>
        /// <returns>Returns the format.</returns>
        private static string nwParseFormat(bool nodate)
        {
            string t;

            t = nwGrabString("timeformat");

            if (nodate == false)
                return "dd/MM/yyyy " + t;
            else
                return t;
        }

        /// <summary>
        /// Write system messages in the appropriate colours
        /// </summary>
        /// <param name="date"></param>
        /// <param name="message"></param>
        public static void nwSystemCCWrite(string date, string message)
        {
            nwColoredConsoleWrite(ConsoleColor.White, "[" + date + "]");
            nwColoredConsoleWrite(ConsoleColor.Yellow, " * System: ");
            nwColoredConsoleWrite(ConsoleColor.Cyan, message + "\r\n");
        }

        /// <summary>
        /// Multi-color line method.
        /// </summary>
        /// <param name="color">The ConsoleColor.</param>
        /// <param name="text">The text to write.</param>
        public static void nwColoredConsoleWrite(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }

        private static void nwInitialStuff(DateTime dt)
        {
            try
            {

                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Loading configuration...");

                // Work item 01. Create our XML document if it doesn't exist
                //if (File.Exists(s_cfgfile) != true)
                //    nwCreateSettings();

                Console.WriteLine(); // blank line

                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Using configuration file: " + s_cfgfile);
                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Logging to file: " + Environment.CurrentDirectory + @"\logs\<this chatroom id>." + dt.ToString(nwGrabString("dateformat")) + ".log");
                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Finished loading configuration...");

                Console.WriteLine(); // blank line

            }
            catch (Exception ex)
            {
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: " + ex.Message);
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            DateTime curTime = DateTime.Now; // current time

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine("* System: Received error: {0} — {1}",
                e.ApiRequestException.ErrorCode,
                e.ApiRequestException.Message);
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;

            using (ehoh.StreamWriter sw = new ehoh.StreamWriter(ehoh.Directory.GetCurrentDirectory() + @"\pkbot.log", true))
            {
                sw.WriteLine("-----------------------------------------------------------------------------");
                sw.WriteLine("* System: Error has occurred at " + curTime.ToLongTimeString());
                sw.WriteLine("* System: Error has occurred: " + e.ApiRequestException.HResult + " " + e.ApiRequestException.Message + Environment.NewLine +
                   "* System: Stack Trace: " + e.ApiRequestException.StackTrace + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.Data.ToString() + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.Message + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.Source + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.StackTrace + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.TargetSite + Environment.NewLine +
                   "* System: Source: " + e.ApiRequestException.Source + Environment.NewLine +
                  "* System: Target Site: " + e.ApiRequestException.TargetSite + Environment.NewLine +
                  "* System: Help Link: " + e.ApiRequestException.HelpLink);
            }
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs e)
        {
            Console.WriteLine($"Received inline query from: {e.InlineQuery.From.Id}");
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            ChatType ct = message.Chat.Type;

            DateTime dt = new DateTime(2016, 2, 2);
            dt = DateTime.Now;

            long n_chanid = message.Chat.Id;
            DateTime m = message.Date.ToLocalTime();

            //remove unsightly characters from first names.
            string s_mffn = message.From.FirstName;
            s_mffn = Regex.Replace(s_mffn, @"[^\u0000-\u007F]", string.Empty);

            if (s_mffn.Contains(" ") == true)
                s_mffn.Replace(" ", string.Empty);

            // variable for username, if blank, use firstname.
            string s_mfun = message.From.Username;

            if (s_mfun == " " || s_mfun == string.Empty)
                s_mfun = s_mffn;

            // END SAVE MESSAGES
            
            var text = message.Text;
            var s_replyToUser = string.Empty;
            var replyAnimation = string.Empty; // For Gifs
            var replyAnimationCaption = string.Empty; // For Gifs
            var replyImage = string.Empty;
            var replyImageCaption = string.Empty;
            var replyText = string.Empty;
            var replyTextEvent = string.Empty;
            var replyHtml = string.Empty;
            var replyVideo = string.Empty;
            var replyVideoCaption = string.Empty;

            // Test last message date.
            dt = message.Date;
            dt = dt.ToLocalTime();

            if (message.Type == MessageType.Audio)
            {

            }

            if (message.Type == MessageType.Voice)
            {
                
            }

            if (message.Type == MessageType.Contact)
            {
                
            }

            if (message.Type == MessageType.Document)
            {

            }

            if (message.Type == MessageType.ChatMembersAdded)
            {
                
            }

            if (message.Type == MessageType.Sticker)
            {

            }

            if (message.Type == MessageType.Video)
            {

            }

            if (message.Type == MessageType.Venue) return;
            if (message.Type == MessageType.Location) return;

            if (message == null || message.Type != MessageType.Text) return;

            if (text.Length == 1 && text.Contains('/') || text.Length == 1 && text.Contains('!')) return;



        }
    }
}
