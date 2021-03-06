﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using static pectoludus.TripleTriadCard;

namespace pectoludus
{
    public class GreaterThanHandler : IGameRuleHandler
    {
        readonly List<FaceDirection> _affectetdCardList = new List<FaceDirection>();

        public string NameString { get; set; }
        public GameruleType RuleType { get; set; }
        public bool PropgatesSideEffects { get; set; }

        public void PerFaceStep(FaceDirection direction, TripleTriadCard existingCard, TripleTriadCard placedCard)
        {
            FaceDirection opposite = GetOppositeDirection(direction);
            if (existingCard.GetCardValue(opposite) < placedCard.GetCardValue(direction))
                _affectetdCardList.Add(direction);
        }

        public ICollection<FaceDirection> AffectedCards
        {
            get
            {
                return _affectetdCardList;
            }
        }
    }

    public class PlusHandler : IGameRuleHandler
    {

        public string NameString { get; set; }
        public GameruleType RuleType { get; set; }
        public bool PropgatesSideEffects { get; set; }

        private readonly Dictionary<int, List<FaceDirection>> _cardsWithSameSum = new Dictionary<int, List<FaceDirection>>();

        public void PerFaceStep(FaceDirection direction, TripleTriadCard existingCard, TripleTriadCard placedCard)
        {
            FaceDirection opposite = GetOppositeDirection(direction);
            int sum = existingCard.GetCardValue(opposite) + placedCard.GetCardValue(direction);

            if (!_cardsWithSameSum.ContainsKey(sum))
                _cardsWithSameSum.Add(sum, new List<FaceDirection> { direction });
            else
                _cardsWithSameSum[sum].Add(direction);

        }

        public ICollection<FaceDirection> AffectedCards
        {
            get
            {
                return ((from card in _cardsWithSameSum
                         where card.Value?.Count >= 2
                         select card.Value).SelectMany(i => i)).ToList();
            }
        }
    }

    public static class GameHandlerFactory
    {
        public static readonly Dictionary<GameruleType, GameRuleDefinition> GameruleDefinitionList = new Dictionary<GameruleType, GameRuleDefinition>() {
            {GameruleType.GreaterThan, new GameRuleDefinition("Greater Than",GameruleType.GreaterThan, false, typeof(GreaterThanHandler))},
            {GameruleType.Plus, new GameRuleDefinition("Plus", GameruleType.Plus, true, typeof(PlusHandler)) },
        };

        public static IGameRuleHandler GetHandler(GameruleType gameruleType)
        {
            GameRuleDefinition definition = GameruleDefinitionList[gameruleType];
            IGameRuleHandler handler = (IGameRuleHandler)Activator.CreateInstance(definition.Handler);

            handler.NameString = definition.NameString;
            handler.RuleType = definition.RuleType;
            handler.PropgatesSideEffects = definition.PropgatesSideEffects;

            return handler;
        }
    }

    /// <summary>
    /// Used to keep track of different rules that may be in play
    /// </summary>
    public enum GameruleType
    {
        None,
        GreaterThan,
        Plus,
        Same,
    }

    public struct GameRuleDefinition
    {
        public GameRuleDefinition(string nameString, GameruleType ruleType, bool propgatesSideEffects, Type handler)
        {
            NameString = nameString;
            RuleType = ruleType;
            PropgatesSideEffects = propgatesSideEffects;
            Handler = handler;
        }

        public string NameString { get; set; }
        public GameruleType RuleType { get; set; }
        public bool PropgatesSideEffects { get; set; }
        public Type Handler { get; set; }

    }

    public interface IGameRuleHandler
    {
        string NameString { get; set; }
        GameruleType RuleType { get; set; }
        bool PropgatesSideEffects { get; set; }

        void PerFaceStep(FaceDirection direction, TripleTriadCard existingCard, TripleTriadCard placedCard);
        ICollection<FaceDirection> AffectedCards { get; }
    }

    public struct Coordinate
    {

        public int X { get; private set; }
        public int Y { get; private set; }
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Represents a complete game of cards
    /// </summary>
    public class TripleTriadGameContainer {
        
        
        private readonly TripleTriadGamegrid _gamegrid;

        private readonly Dictionary<Category, List<Coordinate>> _currentCardsInFamily = new Dictionary<Category, List<Coordinate>>();

        private readonly Dictionary<Ownership, TripleTriadHand> _playersHands = new Dictionary<Ownership, TripleTriadHand>(2);

        

        public ICollection<GameruleType> ActiveGameRules { get; }

        /// <summary>
        /// Resets aspects of the GameContainer to prepare for a new game to be played.
        /// </summary>
        public void ResetForNewGame() {
            _gamegrid.ResetGameGrid();
            _currentCardsInFamily.Clear();
        }

        public TripleTriadGameContainer([CanBeNull] List<GameruleType> activeRules) {
            ActiveGameRules = activeRules ?? new List<GameruleType>();
            _gamegrid = new TripleTriadGamegrid(this);
        }

        internal bool PlayCard(TripleTriadCard tripleTriadCard, int x, int y)
        {
            return PlayCard(tripleTriadCard, new Coordinate(x, y));
        }

        public int GetCardCountForOwner(Ownership ownership) {
            Contract.Requires(Enum.IsDefined(typeof(Ownership), ownership));
            if (_gamegrid.CardCountByOwnerDirty)
                _gamegrid.UpdateCardCountByPlayer();

            // When all the cards belong to a single user the key will not be found.
            int count;
            return _gamegrid.CardCountByOwner.TryGetValue(ownership, out count) ? count : 0;
        }

        /// <summary>
        /// Gets the number of card in play that have the specified category
        /// </summary>
        /// <param name="category">The category of the card</param>
        /// <returns></returns>
        public int GetCardCountForCategory(Category category) {
            return !_currentCardsInFamily.ContainsKey(category) ? 0 : _currentCardsInFamily[category].Count;
        }

        public bool PlayCard(TripleTriadCard card, Coordinate coord) {
            Contract.Requires(card!= null);
            
            if (!_gamegrid.TryPlaceCard(coord, ref card)) {
                return false;
            }

            if (!_currentCardsInFamily.ContainsKey(card.Family)) {
                _currentCardsInFamily[card.Family] = new List<Coordinate> {coord};
            }
            else {
                _currentCardsInFamily[card.Family].Add(coord);
            }

            return true;
        }

        /// <summary>
        /// Adds a player to the current game, using the specified ownership to keep track of card hands
        /// </summary>
        /// <param name="ownership">The ownership key of the specified player</param>
        public void AddPlayer(Ownership ownership) {
            Contract.Requires(Enum.IsDefined(typeof(Ownership),ownership));
            _playersHands.Add(ownership, new TripleTriadHand(ownership, this));
            _gamegrid.RegisterPlayer(ownership);
        }

        /// <summary>
        /// Gets the Ownership key of all the currently attached players
        /// </summary>
        /// <returns>The list of Ownership keys of the attached players</returns>
        public IReadOnlyCollection<Ownership> Players
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<Ownership>>() != null);
                return new ReadOnlyCollection<Ownership>(_playersHands.Keys.ToList());
            }
        }

        /// <summary>
        /// Retrieve the hand of cards, for the specified owner
        /// </summary>
        /// <param name="ownership">The ownership key</param>
        /// <param name="playerHand">The hand of cards</param>
        public void GetPlayerHand(Ownership ownership, out TripleTriadHand playerHand) {
            Contract.Requires(Enum.IsDefined(typeof(Ownership), ownership));
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
