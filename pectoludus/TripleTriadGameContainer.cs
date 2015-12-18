using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pectoludus
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


        public struct GameRuleDefinition {
            public GameRuleDefinition(string nameString, GameruleType ruleType, bool propgatesSideEffects, Type handler) {
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

        public interface IGameRuleHandler {
            string NameString { get; set; }
            GameruleType RuleType { get; set; }
            bool PropgatesSideEffects { get; set; }

            void PerFaceStep(TripleTriadCard.FaceDirection direction, TripleTriadCard existingCard, TripleTriadCard placedCard);
            List<TripleTriadCard.FaceDirection> GetAffectedCards();
        }

        public class GreaterThanHandler : IGameRuleHandler {
            readonly List<TripleTriadCard.FaceDirection> _affectetdCardList = new List<TripleTriadCard.FaceDirection>();

            public string NameString { get; set; }
            public GameruleType RuleType { get; set; }
            public bool PropgatesSideEffects { get; set; }

            public void PerFaceStep(TripleTriadCard.FaceDirection direction, TripleTriadCard existingCard, TripleTriadCard placedCard) {
                TripleTriadCard.FaceDirection opposite = TripleTriadCard.GetOppositeDirection(direction);
                if (existingCard.GetCardValue(opposite) < placedCard.GetCardValue(direction))
                    _affectetdCardList.Add(opposite);
            }

            public List<TripleTriadCard.FaceDirection> GetAffectedCards() {
                return _affectetdCardList;
            }
        }

        public class PlusHandler : IGameRuleHandler {

            public string NameString { get; set; }
            public GameruleType RuleType { get; set; }
            public bool PropgatesSideEffects { get; set; }

            private readonly Dictionary<int, List<TripleTriadCard.FaceDirection>> _cardsWithSameSum = new Dictionary<int,List<TripleTriadCard.FaceDirection>>();

            public void PerFaceStep(TripleTriadCard.FaceDirection direction, TripleTriadCard existingCard, TripleTriadCard placedCard) {
                TripleTriadCard.FaceDirection opposite = TripleTriadCard.GetOppositeDirection(direction);
                int sum = existingCard.GetCardValue(opposite) + placedCard.GetCardValue(direction);

                if (!_cardsWithSameSum.ContainsKey(sum))
                    _cardsWithSameSum.Add(sum,new List<TripleTriadCard.FaceDirection>() {opposite});
                else
                    _cardsWithSameSum[sum].Add(opposite);

            }

            public List<TripleTriadCard.FaceDirection> GetAffectedCards() {
                return ((from card in _cardsWithSameSum
                    where card.Value?.Count >= 2
                    select card.Value).SelectMany(i => i)).ToList();
            }
        }

        public static class GameHandlerFactory {
            public static IGameRuleHandler GetHandler(GameruleType gameruleType) {
                GameRuleDefinition definition = GameruleDefinitionList[gameruleType];
                IGameRuleHandler handler = (IGameRuleHandler) Activator.CreateInstance(definition.Handler);

                handler.NameString = definition.NameString;
                handler.RuleType = definition.RuleType;
                handler.PropgatesSideEffects = definition.PropgatesSideEffects;

                return handler;
            }
        }


        /// <summary>
        /// Used to keep track of different rules that may be in play
        /// </summary>
        public enum GameruleType {
            None,
            GreaterThan,
            Plus,
            Same,
        }

        private readonly TripleTriadGamegrid _gamegrid;

        private readonly Dictionary<TripleTriadCard.Category, List<Coordinate>> _currentCardsInFamily = new Dictionary<TripleTriadCard.Category, List<Coordinate>>();

        private readonly Dictionary<TripleTriadCard.Ownership, TripleTriadHand> _playersHands = new Dictionary<TripleTriadCard.Ownership, TripleTriadHand>(2);

        public readonly List<GameruleType> ActiveGameRules;

        public static readonly Dictionary<GameruleType, GameRuleDefinition> GameruleDefinitionList = new Dictionary<GameruleType, GameRuleDefinition>() {
            {GameruleType.GreaterThan, new GameRuleDefinition("Greater Than",GameruleType.GreaterThan, false, typeof(GreaterThanHandler))},
            {GameruleType.Plus, new GameRuleDefinition("Plus", GameruleType.Plus, true, typeof(PlusHandler)) },
        };

        public TripleTriadGameContainer(List<GameruleType> activeRules) {
            ActiveGameRules = activeRules;
            _gamegrid = new TripleTriadGamegrid(this);
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
