using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using TwitchLib;
using TwitchLib.Models.API;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using TwitchLib.Exceptions.API;
using TwitchLib.Events.PubSub;
using TwitchLib.Events.Services.FollowerService;
using TwitchLib.Events.Services.MessageThrottler;
using TwitchLib.Enums;
using TwitchLib.Extensions.Client;
using static TwitchLib.Models.API.Video;
using static TwitchLib.Models.API.User;
using static TwitchLib.Models.API.Block;
using static TwitchLib.Models.API.Game;
using static TwitchLib.Models.API.Follow;
using static TwitchLib.Models.API.Stream;
using static TwitchLib.Models.API.Channel;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Reflection;
using TwitchAPIViewer;
using TwitchCSharp;

namespace YuSsChat
{
    public partial class Form1 : Form
    {
        List<String> users = new List<String>();
        List<String> SmilesForPidor = new List<String>();
        string path = Environment.CurrentDirectory.ToString() + "UsersLog.txt";
        string path2 = Environment.CurrentDirectory.ToString() + "UniqueUsers.txt";
        public TwitchClient client = new TwitchClient(new ConnectionCredentials("vingerBot", "zzczxdt37h7267hr0kh85uorws2i39"));//vingerBot fdj34jkzxfcd@321  1k9jrf1qr58zmr602rnghf12t685u4

       
        public Form1()
        {
            InitializeComponent();
        }
        delegate void ShowTwitchTV(TwitchTV twitchTV);
        event ShowTwitchTV m_OnShowTwitchTV;
        TwitchTV m_TwitchTV = null;
        Thread m_ThreadLoadKraken;
        bool m_KrakenThreadRunning = false;
        bool m_NeedKrakenUpdate = true;
        void StartKrakenConnectThread()
        {
            if (m_ThreadLoadKraken == null)
            {
                TwitchApi.SetClientId("1k9jrf1qr58zmr602rnghf12t685u4");
                
                m_ThreadLoadKraken = new Thread(new ThreadStart(LoadKraken));
                m_ThreadLoadKraken.Start();
            }
        }
        int maxViewrs=0;
        string curViewrs;

       
        void LoadKraken()
        {
            m_KrakenThreadRunning = true;
            WebClient webClient = new WebClient();
            while (m_KrakenThreadRunning)
            {
                try
                {
                    if (m_NeedKrakenUpdate)
                    {
                        TwitchLib.Models.API.Stream stream = TwitchApi.Streams.GetStream(textBox1.Text);

                        label3.Text = stream.Viewers.ToString();
                        curViewrs =stream.Viewers.ToString();
                        if (maxViewrs < stream.Viewers) {
                            maxViewrs = stream.Viewers;
                        }
                        this.BeginInvoke(m_OnShowTwitchTV, m_TwitchTV);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in LoadKraken: " + e.Message);
                }
                Thread.Sleep(1000);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (client.IsConnected)
            {
                MainChat.Text = MainChat.Text + "\n" + ChatTextBox.Text;

                
                client.SendMessage(ChatTextBox.Text);
                //client.Channel
                ChatTextBox.Text = "";
            }
            else {
                MainChat.Text = MainChat.Text + "\n" + "<<< Вы не подключены >>>";
            }
           
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            client.OnConnected += new EventHandler<OnConnectedArgs>(onConnected);
            client.OnDisconnected += new EventHandler<OnDisconnectedArgs>(onDisconnected);
            client.OnMessageReceived += new EventHandler<OnMessageReceivedArgs>(OnMessageReceived);
            client.OnUserJoined += new EventHandler<OnUserJoinedArgs>(OnUserJoin);
            client.OnUserLeft += new EventHandler<OnUserLeftArgs>(OnUserLeft);
            //client.OnWhisperCommandReceived += clientWhsiperReceived;
            client.Connect();
            MainChat.Text = MainChat.Text + "\n" + " <<< Покдлючаюсь... >>> ";
        }

        private void OnUserLeft(object sender, OnUserLeftArgs e)
        {
            string MessageAnwser = "";
            MessageAnwser =  DateTime.Now.ToLongTimeString() + " " +  e.Username + " ВЫШЕЛ трансляции!" + Environment.NewLine;
            MainChat.Text = MainChat.Text + "\n" + DateTime.Now.ToLongTimeString() + " " + MessageAnwser;
            File.AppendAllText(path, MessageAnwser, Encoding.UTF8);
        }
        private void OnUserJoin(object sender, OnUserJoinedArgs e)
        {
            string MessageAnwser = "";
            MessageAnwser = "/me @"+e.Username + " Добро пожаловать!";
            MainChat.Text = MainChat.Text + "\n" + DateTime.Now.ToLongTimeString() + " " + MessageAnwser;
            //client.SendMessage(MessageAnwser);
            MessageAnwser = "\n" + DateTime.Now.ToLongTimeString() + " " + e.Username + " ЗАШЕЛ на трансляцию!" + Environment.NewLine;
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string createText = MessageAnwser + Environment.NewLine;
                File.WriteAllText(path, createText, Encoding.UTF8);
            }
            File.AppendAllText(path, MessageAnwser, Encoding.UTF8);

            if (!users.Contains(e.Username)) {
                users.Add(e.Username);
            }


        }
        int DropCount = 0;
        List<String> usersVotedForDrop = new List<String>();
        
        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        { string MessageAnwser="";
            // CheckForIllegalCrossThreadCalls = false;
            string incMessage = e.ChatMessage.Message;
            if (incMessage.ToLower() == "!петуч")
            {
                MessageAnwser = "@" + e.ChatMessage.Username + " Петуч";
            }
            else {


                if (incMessage.ToLower() == "!инфа")
                {
                    MessageAnwser = "Стим: http://steamcommunity.com/profiles/76561198041342498/  Ютуб: https://www.youtube.com/channel/UCbRdMvQxPFRMXDOUBXR7RdA  Донат: http://www.donationalerts.ru/r/vinger";
                }
                else
                if (incMessage.ToLower() == "!команды")
                {
                    MessageAnwser = "!петуч; !инфа; !дроп; !тнп; !кинуть [предмет] [цель]; !дуэль [цель]";
                }

                else
                 if (incMessage.ToLower() == "!up")
                {
                    if (TwitchApi.Streams.BroadcasterOnline((textBox1.Text)))
                    {       MessageAnwser = "Стрим идет : " + TwitchApi.Streams.GetUptime(textBox1.Text).ToString().Substring(0, 8) + " | Сейчас смотрят: " + curViewrs + " | Пик: " + maxViewrs;
                        }
                    else {
                        MessageAnwser = "Стрим оффлайн";
                    }
                }

                else
                if (incMessage.ToLower() == "!дроп")
                {
                    if (!users.Contains(e.ChatMessage.Username))
                    {
                        DropCount++;
                        usersVotedForDrop.Add(e.ChatMessage.Username);
                        MessageAnwser = "Голосов за дроп игры: " + DropCount.ToString();
                        label2.Text = "Голосов за дроп игры: " + DropCount.ToString();
                    }
                }
                else
                 if (incMessage.ToLower() == "!тнп")
                {
                    Random rnd = new Random();
                    int randomdeul = rnd.Next(6);
                    if (randomdeul == 1) {
                        MessageAnwser = "Тестирование @" + e.ChatMessage.Username + " результат: " + SmilesForPidor[randomdeul]+" (положительный)";
                    }
                    else
                    MessageAnwser = "Тестирование @" + e.ChatMessage.Username+ " результат: " + SmilesForPidor[randomdeul] + " (отрицательный)";
                }
                else
                { 
                int spaceCount = 0;
                for (int i = 0; i < incMessage.Length; i++) {
                    if (incMessage[i] == ' ')
                        {
                        spaceCount++;
                        }
                            }
                   
                    if (spaceCount > 1)
                    {
                        string incMessageFirstWord = incMessage.Substring(0, incMessage.IndexOf(' '));


                        if (incMessageFirstWord.ToLower() == "!кинуть")
                        {

                            string incMessageLastWord = incMessage.Substring(incMessage.LastIndexOf(' ') + 1);
                            int temp = incMessage.Length - incMessageLastWord.Length - incMessageFirstWord.Length - 1;
                            string throwObject = incMessage.Substring(incMessage.IndexOf(' '), temp);
                            if (incMessageLastWord[0] != '@')
                            {
                                incMessageLastWord='@'+ incMessageLastWord;
                            }
                                MessageAnwser = "@" + e.ChatMessage.Username + " кинул " + throwObject + " в " + incMessageLastWord;
                            
                         }   

                    }

                    if (spaceCount == 1) {
                        string incMessageFirstWord = incMessage.Substring(0, incMessage.IndexOf(' '));
                        string incMessageFirstWord1 = incMessageFirstWord.ToLower();
                        if (incMessageFirstWord1 == "!дуэль")
                        {
                            Random rnd = new Random();
                            string incMessageLastWord = incMessage.Substring(incMessage.LastIndexOf(' ') + 1);
                            if (incMessageLastWord[0] != '@')
                            {
                                incMessageLastWord = '@' + incMessageLastWord;
                            }
                            int randomdeul = rnd.Next(2);
                            if (randomdeul == 0) {
                                MessageAnwser = "@" + e.ChatMessage.Username + " выиграл в дуэли у  "  + incMessageLastWord+" VoteYea";
                            }
                            if (randomdeul == 1)
                            {
                                
                                MessageAnwser = "@" + e.ChatMessage.Username + " проиграл в дуэли у  "  + incMessageLastWord + " VoteNay";
                            }
                        }
                        }
                }
            }

            MainChat.Text = MainChat.Text + "\n" + DateTime.Now.ToLongTimeString() +" " + e.ChatMessage.Username +": "+ e.ChatMessage.Message;
            if (MessageAnwser != "")
            {
                MainChat.Text = MainChat.Text + "\n" + DateTime.Now.ToLongTimeString() + " " + MessageAnwser;
            }
            client.SendMessage(MessageAnwser);

        }
        private void onConnected(object sender, OnConnectedArgs e)
        {
            client.JoinChannel(textBox1.Text);
            CheckForIllegalCrossThreadCalls = false;


            users.Clear();
            MainChat.Text = MainChat.Text + "\n" + "<<< Подключился к каналу "+ textBox1.Text+" >>>";
            string UsersStats = "Начало сбора статистики:" + DateTime.Now.ToString() + Environment.NewLine;
            File.AppendAllText(path2, UsersStats, Encoding.UTF8);
            StartKrakenConnectThread();


        }
        
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            client.LeaveChannel(textBox1.Text);
            client.Disconnect();
            MainChat.Text = MainChat.Text + "\n" + "<<< Отключаюсь... >>>";
        }
        private void onDisconnected(object sender, OnDisconnectedArgs e)
        {
           // CheckForIllegalCrossThreadCalls = false;



            MainChat.Text = MainChat.Text + "\n" + "<<< Отключился >>>";

            SaveStats();
            Application.Exit();
        }
        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            MainChat.SelectionStart = MainChat.Text.Length;
            MainChat.ScrollToCaret();
        }
        public void SaveStats() {
            if (!File.Exists(path2))
            {
                // Create a file to write to.
                File.WriteAllLines(path2, users, Encoding.UTF8);
            }
            else
            {
                File.AppendAllLines(path2, users, Encoding.UTF8);
            }

            string UsersStats ="Максимальное число зрителей: "+maxViewrs+ " Количество уникальных пользователей: " + users.Count.ToString() + " Конец сбора статистики:" + DateTime.Now.ToString() + Environment.NewLine + "=========================================================================================" + Environment.NewLine;
            File.AppendAllText(path2, UsersStats, Encoding.UTF8);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveStats();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DropCount = 0;
            usersVotedForDrop.Clear();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SmilesForPidor.Add("Kappa");
            SmilesForPidor.Add("KappaPride");
            SmilesForPidor.Add("Keepo");
            SmilesForPidor.Add("Kippa");
            SmilesForPidor.Add("KappaRoss");
            SmilesForPidor.Add("KappaClaus");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            MainChat.Text = MainChat.Text + "\n" + "<<<DEBUG >>>";
            if (client.IsConnected)
            {
                string message = " MrDestructoid Приятного просмотра! Комментируйте, стример вам обязательно ответит!";
                MainChat.Text = MainChat.Text + "\n " + message;
                client.SendMessage(message);

            }
            else
            {
                MainChat.Text = MainChat.Text + "\n" + "<<< Вы не подключены >>>";
            }
        }
    }
}
