using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Configuration;
using System.Threading;


namespace pectoludus
{
    class Program {
        static void Main(string[] args) {
            Console.OutputEncoding = Encoding.UTF8;

            TripleTriadGameContainer gameContainer = new TripleTriadGameContainer(new List<GameruleType> {GameruleType.Plus});
            gameContainer.AddPlayer(TripleTriadCard.Ownership.Player);
            gameContainer.AddPlayer(TripleTriadCard.Ownership.NPC);

            TripleTriadHand playerHand;
            gameContainer.GetPlayerHand(TripleTriadCard.Ownership.Player, out playerHand);
            TripleTriadHand npcHand;
            gameContainer.GetPlayerHand(TripleTriadCard.Ownership.NPC, out npcHand);

            playerHand.AddManyCards(new List<string>() {"Dodo", "Tonberry","Sabotender" });

            playerHand.PlayCard(0, 0, 0);
            gameContainer.DrawCurrentGame();
            if (!Debugger.IsAttached)
                Thread.Sleep(1000);

            playerHand.PlayCard(1, 2, 0);
            gameContainer.DrawCurrentGame();
            if (!Debugger.IsAttached)
                Thread.Sleep(1000);

            npcHand.PlayCard("Tonberry", 1, 0);
            gameContainer.DrawCurrentGame();
            if (!Debugger.IsAttached)
                Thread.Sleep(1000);

            npcHand.PlayCard("Sabotender", 2, 1);
            gameContainer.DrawCurrentGame();
            if (!Debugger.IsAttached)
                Thread.Sleep(1000);

            playerHand.PlayCard(2, 1, 1);
            gameContainer.DrawCurrentGame();
            if (!Debugger.IsAttached)
                Thread.Sleep(1000);

            //npcHand.PlayCard("Dodo", 0, 1);
            //gameContainer.DrawCurrentGame();
        }
    }
}
