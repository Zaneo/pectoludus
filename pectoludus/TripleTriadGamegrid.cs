using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeContractTest
{
    /// <summary>
    /// The playing field on which the game takes place
    /// </summary>
    internal class TripleTriadGamegrid
    {
        public const int FieldWidth = 3;
        public const int FieldHeight = 3;
        private readonly TripleTriadCard[,] _playingField;

        public int NumberofCardsOnField { get; private set; }
        internal Dictionary<TripleTriadCard.Ownership, int> CardCountByOwner { get; }

        public TripleTriadGamegrid()
        {
            _playingField = new TripleTriadCard[FieldHeight, FieldWidth];
            CardCountByOwner = new Dictionary<TripleTriadCard.Ownership, int>();
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
        public bool TryPlaceCard(TripleTriadGameContainer.Coordinate coord, ref TripleTriadCard card)
        {
            return TryPlaceCard(coord.X, coord.Y, ref card);
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
            if (_playingField[y, x] != null) return false;

            _playingField[y, x] = card;

            // Find any adjacent cards that may have been affected by this placement
            PropogateSideEffects(x, y);

            NumberofCardsOnField++;
            return true;
        }

        /// <summary>
        /// Gets the opposite card face, to the specified card face
        /// </summary>
        /// <param name="direction">The direction of the current card face</param>
        /// <returns>The direction of the opposite card face</returns>
        private static TripleTriadCard.FaceDirection GetOppositeDirection(TripleTriadCard.FaceDirection direction) {
            return (TripleTriadCard.FaceDirection)(((int)direction + 2) % 4) ;
        }

        /// <summary>
        /// Gets the coordinate offset of the card adjacent to the specified direction
        /// </summary>
        /// <param name="direction">The direction of the current card face</param>
        /// <returns>The offset coordinate to the adjacent card</returns>
        private static TripleTriadGameContainer.Coordinate GetCoorindateOffset(TripleTriadCard.FaceDirection direction) {
            // Precopmute these instead of creating a new object each time
            switch (direction) {
                case TripleTriadCard.FaceDirection.Left:
                    return new TripleTriadGameContainer.Coordinate(-1, 0);
                case TripleTriadCard.FaceDirection.Up:
                    return new TripleTriadGameContainer.Coordinate(0, -1);
                case TripleTriadCard.FaceDirection.Right:
                    return new TripleTriadGameContainer.Coordinate(1, 0);
                case TripleTriadCard.FaceDirection.Down:
                    return new TripleTriadGameContainer.Coordinate(0, 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        /// Checks to see if the placement of a card will cause ownership changes in any other cards based on the current Gamerules
        /// </summary>
        /// <param name="x">The x coordinate of the card which caused the check</param>
        /// <param name="y">The y coordinate of the card which caused the check</param>
        /// <seealso cref="TripleTriadGameContainer.Gamerules"/>
        private void PropogateSideEffects(int x, int y) {
            TripleTriadCard placedCard = _playingField[y, x];

            // Get the cards adjacent to the card that cause the update
            // Select the face that is touching each of our faces (opposite face)
            foreach (TripleTriadCard.FaceDirection value in Enum.GetValues(typeof(TripleTriadCard.FaceDirection))) {
                TripleTriadCard.FaceDirection opposite = GetOppositeDirection(value);
                TripleTriadGameContainer.Coordinate coord = GetCoorindateOffset(value);
                var nx = x + coord.X;
                var ny = y + coord.Y;

                if (nx < 0 || nx == FieldWidth || ny < 0 || ny == FieldHeight) continue;

                var existingCard = _playingField[ny, nx];
                
                if (existingCard == null) continue;
                if (existingCard.Owner == placedCard.Owner) continue;
                
                // Basic GameRule: Greater than
                // If this is made a Gamerule rather than assumed, could provide more flexible game modes?
                if (existingCard.GetCardValue(opposite) < placedCard.GetCardValue(value))
                    existingCard.Owner = placedCard.Owner;
            }
        }

        /// <summary>
        /// Checks to see if the placement of a card will cause ownership changes in any other cards based on the current Gamerules
        /// </summary>
        /// <param name="x">The x coordinate of the card which caused the check</param>
        /// <param name="y">The y coordinate of the card which caused the check</param>
        /// <seealso cref="TripleTriadGameContainer.Gamerules"/>
        [Obsolete("Replaced by PropogateSideEffects. Not to be removed until PropogateSideEffect has been completely verified")]
        private void PropogateSideEffectsOld(int x, int y) {
            Contract.Requires(x >= 0);
            Contract.Requires(x < FieldWidth);
            Contract.Requires(y >= 0);
            Contract.Requires(y < FieldHeight);

            TripleTriadCard placedCard = _playingField[y, x];
            TripleTriadCard existingCard;

            #region Basic Greater than Check

            if (x > 0) {
                existingCard = _playingField[y, x - 1];
                if (existingCard?.Owner != placedCard.Owner && existingCard?.GetCardValue(TripleTriadCard.FaceDirection.Right) < placedCard.GetCardValue(TripleTriadCard.FaceDirection.Left)) {
                    CardCountByOwner[existingCard.Owner]--;
                    CardCountByOwner[placedCard.Owner]++;
                    existingCard.Owner = placedCard.Owner;
                }
            }
            if (x < FieldWidth - 1) {
                existingCard = _playingField[y, x + 1];
                if (existingCard?.Owner != placedCard.Owner && existingCard?.GetCardValue(TripleTriadCard.FaceDirection.Left) < placedCard.GetCardValue(TripleTriadCard.FaceDirection.Right)) {
                    CardCountByOwner[existingCard.Owner]--;
                    CardCountByOwner[placedCard.Owner]++;
                    existingCard.Owner = placedCard.Owner;
                }
            }

            if (y > 0) {
                existingCard = _playingField[y - 1, x];
                if (existingCard?.Owner != placedCard.Owner && existingCard?.GetCardValue(TripleTriadCard.FaceDirection.Down) < placedCard.GetCardValue(TripleTriadCard.FaceDirection.Up)) {
                    CardCountByOwner[existingCard.Owner]--;
                    CardCountByOwner[placedCard.Owner]++;
                    existingCard.Owner = placedCard.Owner;
                }
            }
            if (y < FieldHeight - 1) {
                existingCard = _playingField[y + 1, x];
                if (existingCard?.Owner != placedCard.Owner && existingCard?.GetCardValue(TripleTriadCard.FaceDirection.Up) < placedCard.GetCardValue(TripleTriadCard.FaceDirection.Down)) {
                    CardCountByOwner[existingCard.Owner]--;
                    CardCountByOwner[placedCard.Owner]++;
                    existingCard.Owner = placedCard.Owner;
                }
            }

            #endregion
        }

        /// <summary>
        /// Draws the current Gamegrid to console
        /// </summary>
        public void DrawGameGrid() {
            for (int i = 0; i < FieldHeight; i++) {
                for (int j = 0; j < FieldWidth; j++) {
                    TripleTriadCard card = _playingField[i, j];
                    if (card == null) continue;
                    Console.ForegroundColor = card.Owner == TripleTriadCard.Ownership.Player ? ConsoleColor.Cyan : ConsoleColor.Red;
                    card.DrawCard(j, i);
                }
            }
        }
    }
}
