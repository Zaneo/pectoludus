using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace pectoludus
{
    /// <summary>
    /// The playing field on which the game takes place
    /// </summary>
    public class TripleTriadGamegrid
    {
        public const int FieldWidth = 3;
        public const int FieldHeight = 3;

        private readonly TripleTriadCard[][] _playingField;

        public int NumberofCardsOnField { get; private set; }

        public bool CardCountByOwnerDirty { get; set; }

        public Dictionary<TripleTriadCard.Ownership, int> CardCountByOwner { get; private set; } = new Dictionary<TripleTriadCard.Ownership, int>();

        private readonly TripleTriadGameContainer _attachedContainer;

        public TripleTriadGamegrid() : this(FieldHeight, FieldWidth, null)
        {
            
        }


        // Maybe allow container to be null?
        // Pros:
        // Can create a GameGrid without a container
        // Cons:
        // Almost all actions will be undefined and cause Null Reference exceptions
        /// <summary>
        /// Creates a GameGrid, and links it to the specified GameContainer
        /// </summary>
        /// <param name="container">The GameContainer to link to</param>
        public TripleTriadGamegrid([CanBeNull] TripleTriadGameContainer container) : this(FieldHeight, FieldWidth, container)
        {
            
        }

        public TripleTriadGamegrid(int height, int width, TripleTriadGameContainer container)
        {
            _playingField = new TripleTriadCard[height][];
            for (int i = 0; i < height; i++) {
                _playingField[i] = new TripleTriadCard[width];
            }
            _attachedContainer = container;
        }

        /// <summary>
        /// Clears the GameGrid, and resets all tracked statistics
        /// </summary>
        public void ResetGameGrid() {
            NumberofCardsOnField = 0;
            CardCountByOwnerDirty = true;
            for (int i = 0; i < FieldHeight; i++) {
                Array.Clear(_playingField[i], 0, FieldWidth);
            }
        }

       

        /// <summary>
        /// Add the specified ownership key to Gamegrid, to allow card count tracking
        /// </summary>
        /// <param name="ownership">The ownership key of the specified player</param>
        public void RegisterPlayer(TripleTriadCard.Ownership ownership) {
            CardCountByOwner.Add(ownership, 0);
        }

        /// /// <summary>
        /// Attempt to place the card at the specified card coordinate
        /// </summary>
        /// <param name="coord">The card coordinate where the card should be placed</param>
        /// <param name="card">The card that should be placed</param>
        /// <returns>Whether or not the operations completed sucessfully</returns>
        public bool TryPlaceCard(Coordinate coord, ref TripleTriadCard card)
        {
            return TryPlaceCard(coord.X, coord.Y, ref card);
        }


        public void UpdateCardCountByPlayer() {
            var owners = CardCountByOwner.Keys.ToList();
            foreach (var owner in owners) {
                CardCountByOwner[owner] = 0;
            }

            for (int i = 0; i < FieldHeight; i++) {
                for (int j = 0; j < FieldWidth; j++) {
                    var card = _playingField[i][j];
                    CardCountByOwner[card.Owner]++;
                }
            }

            CardCountByOwnerDirty = false;
        }

        /// <summary>
        /// Attempt to place the card at the specified card coordinate
        /// </summary>
        /// <param name="x">The x card coordinate</param>
        /// <param name="y">The y card coordinate</param>
        /// <param name="card">The card that should be placed</param>
        /// <returns>Whether or not the operations completed sucessfully</returns>
        public bool TryPlaceCard(int x, int y, ref TripleTriadCard card)
        {
            Contract.Requires(x >= 0);
            Contract.Requires(x < FieldWidth);
            Contract.Requires(y >= 0);
            Contract.Requires(y < FieldHeight);
            Contract.Requires(card != null);

            // The map is too full to insert a card
            // Possible fire off an event that the game is complete?
            if (NumberofCardsOnField == FieldWidth*FieldHeight) return false;

            // The specified position already contains a card
            if (_playingField[y][x] != null) return false;

            _playingField[y][x] = card;

            // Find any adjacent cards that may have been affected by this placement
            if (_attachedContainer != null) {
                PropogateSideEffects(x, y);
            }

            NumberofCardsOnField++;
            CardCountByOwnerDirty = true;

            return true;
        }

        /// <summary>
        /// Gets the coordinate offset of the card adjacent to the specified direction
        /// </summary>
        /// <param name="direction">The direction of the current card face</param>
        /// <returns>The offset coordinate to the adjacent card</returns>
        private static Coordinate GetCoorindateOffset(TripleTriadCard.FaceDirection direction) {
            // Precopmute these instead of creating a new object each time
            switch (direction) {
                case TripleTriadCard.FaceDirection.Left:
                    return new Coordinate(-1, 0);
                case TripleTriadCard.FaceDirection.Up:
                    return new Coordinate(0, -1);
                case TripleTriadCard.FaceDirection.Right:
                    return new Coordinate(1, 0);
                case TripleTriadCard.FaceDirection.Down:
                    return new Coordinate(0, 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        /// Checks to see if the placement of a card will cause ownership changes in any other cards based on the current GameruleType
        /// </summary>
        /// <param name="x">The x coordinate of the card which caused the check</param>
        /// <param name="y">The y coordinate of the card which caused the check</param>
        /// <seealso cref="GameruleType"/>
        private void PropogateSideEffects(int x, int y) {
            Contract.Requires(_attachedContainer != null);

            TripleTriadCard placedCard = _playingField[y][x];
            
            List<IGameRuleHandler> ruleHandlers = new List<IGameRuleHandler>(_attachedContainer.ActiveGameRules.Count);
            ruleHandlers.AddRange(_attachedContainer.ActiveGameRules.Select(GameHandlerFactory.GetHandler));
            
            // Get the cards adjacent to the card that cause the update
            // Select the face that is touching each of our faces (opposite face)
            foreach (TripleTriadCard.FaceDirection value in Enum.GetValues(typeof(TripleTriadCard.FaceDirection))) {
                TripleTriadCard.FaceDirection opposite = TripleTriadCard.GetOppositeDirection(value);
                Coordinate coord = GetCoorindateOffset(value);
                var nx = x + coord.X;
                var ny = y + coord.Y;

                if (nx < 0 || nx == FieldWidth || ny < 0 || ny == FieldHeight) continue;

                var existingCard = _playingField[ny][x];
                
                if (existingCard == null) continue;
                if (existingCard.Owner == placedCard.Owner) continue;

                int oppositeValue = existingCard.GetCardValue(opposite);
                int placedValue = placedCard.GetCardValue(value);

                foreach (var ruleHandler in ruleHandlers) {
                    ruleHandler.PerFaceStep(value, existingCard, placedCard);
                }
            }

            foreach (var ruleHandler in ruleHandlers) {
                var affecteCardDirections = ruleHandler.AffectedCards;
                foreach (var faceDirection in affecteCardDirections) {
                    Coordinate coord = GetCoorindateOffset(faceDirection);
                    var nx = x + coord.X;
                    var ny = y + coord.Y;

                    if (nx < 0 || nx == FieldWidth || ny < 0 || ny == FieldHeight) continue;

                    var existingCard = _playingField[ny][nx];
                    existingCard.Owner = placedCard.Owner;

                    if (ruleHandler.PropgatesSideEffects)
                        PropogateSideEffects(nx, ny);
                }
            }
        }

        /// <summary>
        /// Draws the current Gamegrid to console
        /// </summary>
        public void DrawGameGrid() {
            for (int i = 0; i < FieldHeight; i++) {
                for (int j = 0; j < FieldWidth; j++) {
                    TripleTriadCard card = _playingField[i][j];
                    if (card == null) continue;
                    Console.ForegroundColor = card.Owner == TripleTriadCard.Ownership.Player ? ConsoleColor.Cyan : ConsoleColor.Red;
                    card.DrawCardAsASCII(j, i);
                }
            }
        }
    }
}
