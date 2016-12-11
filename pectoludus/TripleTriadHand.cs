using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pectoludus
{
    /// <summary>
    /// Represent the cards currently in a player's hand
    /// </summary>
    public class TripleTriadHand {
        public const int MaxAllowedCardsInHand = 5;
        private readonly TripleTriadCard[] _cardsInHand;
        private int _currentNumberOfCards;

        private readonly TripleTriadGameContainer _currentGameContainer;
        public TripleTriadCard.Ownership Owner { get; private set; }

        public TripleTriadHand(TripleTriadCard.Ownership ownership, TripleTriadGameContainer gameContainer) {
            _currentGameContainer = gameContainer;
            _cardsInHand = new TripleTriadCard[MaxAllowedCardsInHand];
            _currentNumberOfCards = 0;
            Owner = ownership;
        }

        /// <summary>
        /// Adds the specified card to the player's hand, and advances an internal index.
        /// </summary>
        /// <param name="card">The card that should be added</param>
        /// <remarks>Adding cards once any cards have been removed is not supported</remarks>
        public void AddCard(ref TripleTriadCard card) {
            _cardsInHand[_currentNumberOfCards++] = card;
        }

        /// <summary>
        /// Adds many cards using the specified list
        /// </summary>
        /// <param name="cardNameList">Valid list of card names</param>
        public void AddManyCards(ICollection<string> cardNameList) {
            Contract.Requires(cardNameList != null);
            foreach (var cardName in cardNameList) {
                TripleTriadCard card = new TripleTriadCard(cardName) {Owner = Owner};
                AddCard(ref card);
            }
        }

        /// <summary>
        /// Attempt to play the specified card, in the attached GameContainer, at the specified card coordinates
        /// </summary>
        /// <param name="index">The index of the card to be played</param>
        /// <param name="x">The x card coordinate</param>
        /// <param name="y">The y card coordinate</param>
        /// <returns>Whether or not the operation completed sucessfully</returns>
        /// <remarks>If the operation is succesful the card is removed from the players inventory</remarks>
        public bool PlayCard(int index, int x, int y) {
            Contract.Requires(index >= 0);
            Contract.Requires(x >= 0);
            Contract.Requires(x < TripleTriadGamegrid.FieldWidth);
            Contract.Requires(y >= 0);
            Contract.Requires(y < TripleTriadGamegrid.FieldHeight);
            if (!_currentGameContainer.PlayCard(_cardsInHand[index], x, y)) return false;

            _cardsInHand[index] = null;

            return true;
        }

        /// <summary>
        /// Attempts to play the card specified by name, in the attached GameContainer, at the specified card coordinates
        /// </summary>
        /// <param name="name">The name of the card to be played</param>
        /// <param name="x">The x card coordinate</param>
        /// <param name="y">The y card coordinate</param>
        /// <returns>Whether or not the operation completed succesfully</returns>
        /// <remarks>This is to allow cards which were not known to be in the players hand before the game was started</remarks>
        public bool PlayCard(string name, int x, int y) {
            Contract.Requires(x >= 0);
            Contract.Requires(x < TripleTriadGamegrid.FieldWidth);
            Contract.Requires(y >= 0);
            Contract.Requires(y < TripleTriadGamegrid.FieldHeight);
            TripleTriadCard card = new TripleTriadCard(name) {Owner = Owner};
            return _currentGameContainer.PlayCard(card, x, y);
        }

        /// <summary>
        /// Attempts to play the specified card by value, in the attached GameContainer, at the specified card coordinates
        /// </summary>
        /// <param name="card">The card to be played</param>
        /// <param name="x">The x card coordinate</param>
        /// <param name="y">The y card coordinate</param>
        /// <returns></returns>
        public bool PlayCard(TripleTriadCard card, int x, int y) {
            Contract.Requires(x >= 0);
            Contract.Requires(x < TripleTriadGamegrid.FieldWidth);
            Contract.Requires(y >= 0);
            Contract.Requires(y < TripleTriadGamegrid.FieldHeight);
            card.Owner = Owner;
            return _currentGameContainer.PlayCard(card, x, y);
        }
    }
}
