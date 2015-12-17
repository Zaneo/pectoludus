using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeContractTest
{
    /// <summary>
    /// Represents a complete game of cards
    /// </summary>
    class TripleTriadGameContainer {
        public struct Coordinate {

            public int X { get; private set; }
            public int Y { get; private set; }
            public Coordinate(int x, int y) {
                X = x;
                Y = y;
            }
        }


        /// <summary>
        /// Used to keep track of different rules that may be in play
        /// </summary>
        public enum Gamerules {
            None,
        }

        private readonly TripleTriadGamegrid _gamegrid;

        private readonly Dictionary<TripleTriadCard.Category, List<Coordinate>> _currentCardsInFamily;

        private readonly Dictionary<TripleTriadCard.Ownership, TripleTriadHand> _playersHands;

        public TripleTriadGameContainer() {
            _currentCardsInFamily = new Dictionary<TripleTriadCard.Category, List<Coordinate>>();
            _gamegrid = new TripleTriadGamegrid();
            _playersHands = new Dictionary<TripleTriadCard.Ownership, TripleTriadHand>(2);
        }

        internal bool PlayCard(TripleTriadCard tripleTriadCard, int x, int y)
        {
            return PlayCard(tripleTriadCard, new Coordinate(x, y));
        }

        public bool PlayCard(TripleTriadCard card, Coordinate coord) {
            Contract.Requires(card!= null);
            
            if (!_gamegrid.TryPlaceCard(coord, ref card)) {
                return false;
            }

            if (!_currentCardsInFamily.ContainsKey(card.Family)) {
                _currentCardsInFamily[card.Family] = new List<Coordinate> {coord};
            }

            _currentCardsInFamily[card.Family].Add(coord) ;

            return true;
        }

        /// <summary>
        /// Adds a player to the current game, using the specified ownership to keep track of card hands
        /// </summary>
        /// <param name="ownership">The ownership key of the specified player</param>
        public void AddPlayer(TripleTriadCard.Ownership ownership) {
            Contract.Requires(Enum.IsDefined(typeof(TripleTriadCard.Ownership),ownership));
            _playersHands.Add(ownership, new TripleTriadHand(ownership, this));
            _gamegrid.RegisterPlayer(ownership);
        }

        /// <summary>
        /// Retrieve the hand of cards, for the specified owner
        /// </summary>
        /// <param name="ownership">The ownership key</param>
        /// <param name="playerHand">The hand of cards</param>
        public void GetPlayerHand(TripleTriadCard.Ownership ownership, out TripleTriadHand playerHand) {
            Contract.Requires(Enum.IsDefined(typeof(TripleTriadCard.Ownership), ownership));
            playerHand = _playersHands[ownership];
        }

        /// <summary>
        /// Draws the Gamegrid to console
        /// </summary>
        public void DrawCurrentGame() {
            _gamegrid.DrawGameGrid();
        }
    }
}
