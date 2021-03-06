﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pectoludus
{

    public struct CardDefinition
    {
        public const int CardFaceCount = 4;

        public string Name { get; }

        public int[] Values { get; }

        public TripleTriadCard.Category Family { get; }

        public CardDefinition(string name, int[] values, TripleTriadCard.Category family)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(values.Length == CardFaceCount);
            
            Name = name;
            Values = values;
            Family = family;
        }
    }

    [DebuggerDisplay("Name={Name}, Owner={Owner}")]
    public class TripleTriadCard
    {
        /// <summary>
        /// Stores all the information of a card, but is not playable.
        /// </summary>
        // Since name, values, and family are all static, why is this duplicated? Possible for TripleTriadCard to just contain a copy of this information?
        // Or if possible, lookup the card based on ID, so it is only stored once.
        

        /// <summary>
        /// List of all possible cards
        /// </summary>
        public static readonly Dictionary<string, CardDefinition> CardList = new Dictionary<string, CardDefinition> {
            {"Dodo", new CardDefinition("Dodo", new[] {4, 4, 2, 3}, Category.None)},
            {"Tonberry", new CardDefinition("Tonberry", new[] {2, 2, 2, 7},Category.None)},
            {"Sabotender", new CardDefinition("Sabotender", new[] {3, 4, 3, 3},Category.None)},
            {"Spriggan", new CardDefinition("Spriggan", new[] {4, 2, 3, 4},Category.None)},
            {"Pudding", new CardDefinition("Pudding", new[] {5, 2, 4, 3},Category.None)},
            {"Bomb", new CardDefinition("Bomb", new[] {3, 3, 4, 3},Category.None)},
            {"Mandragora", new CardDefinition("Mandragora", new[] {3, 4, 2, 5},Category.None)},
            {"Coblyn", new CardDefinition("Coblyn", new[] {4, 3, 3, 3},Category.None)},
        };

        private static readonly List<char> CardValueString = new List<char>();
        
        private const int CardMaxFaceValue = 10;

        public enum FaceDirection
        {
            Left,
            Up,
            Right,
            Down
        }

        /// <summary>
        /// The Family the card belongs too
        /// </summary>
        /// <remarks>This has an effect for particular GameruleType</remarks>
        /// <seealso cref="TripleTriadGameContainer.GameruleType"/>
        public enum Category
        {
            None,
            Beastman,
            Primal,
            Garlean,
            Scion,
        }

        public enum Ownership
        {
            None,
            Player,
            NPC
        }

        /// <summary>
        /// The values on the faces of the cards.
        /// </summary>
        /// <seealso cref="FaceDirection"/>
        private readonly int[] _baseValues;

        public string Name { get; private set; }
        public Category Family { get; private set; }

        public Ownership Owner { get; set; }

        /// <summary>
        /// Any card modifiers which affect the face value
        /// </summary>
        /// <seealso cref="GetCardValue"/>
        public int Modifier { get; private set; }

        static TripleTriadCard()
        {
            //Contract.Requires(CardValueString != null);
            for (int i = 0; i < CardMaxFaceValue; i++) {
                CardValueString.Add(i.ToString("X", CultureInfo.InvariantCulture)[0]);
            }
        }

        public TripleTriadCard() : this("", new int[4], Category.None) { }

        public TripleTriadCard(string name, int[] baseValues, Category family)
        {
            Contract.Requires(baseValues != null);
            Contract.Requires(baseValues.Length == CardDefinition.CardFaceCount);

            Name = name;
            _baseValues = baseValues;
            Family = family;
            Modifier = 0;
        }

        public TripleTriadCard(CardDefinition cardDef) : this(cardDef.Name, cardDef.Values, cardDef.Family) { }

        public TripleTriadCard(string cardName) : this(GetCardDefinition(cardName))
        {
        }

        static CardDefinition GetCardDefinition(string cardName)
        {
            Contract.Requires(CardList.ContainsKey(cardName));
            return CardList[cardName];
        }

        /// <summary>
        /// Gets the opposite card face, to the specified card face
        /// </summary>
        /// <param name="direction">The direction of the current card face</param>
        /// <returns>The direction of the opposite card face</returns>
        public static FaceDirection GetOppositeDirection(FaceDirection direction)
        {
            return (FaceDirection)(((int)direction + 2) % 4);
        }

        /// <summary>
        /// Gets the value of the face specified by the direction
        /// </summary>
        /// <param name="direction">The desired card face</param>
        /// <returns>Value of the specified face</returns>
        public int GetCardValue(FaceDirection direction)
        {
            Contract.Requires(Enum.IsDefined(typeof(FaceDirection), direction));
            //Contract.Ensures(Contract.Result<int>() < CardValueString.Count + Modifier);
            return _baseValues[(int)direction] + Modifier;
        }

        /// <summary>
        /// Draws the card to console, at the specified card coordinates
        /// </summary>
        /// <param name="x">The x card coordinate</param>
        /// <param name="y">The y card coordinate</param>
        /// <remarks>The coordinates are converted to screen coordinates internally, assuming a card size of 3x3</remarks>
        public void DrawCardAsASCII(int x, int y)
        {
            // ReSharper disable LocalizableElement
            Console.SetCursorPosition(x * 3, y * 3);
            Console.Write("╔{0:X}╗", _baseValues[1]);
            Console.SetCursorPosition(x * 3, y * 3 + 1);
            Console.Write("{0:X} {1:X}", _baseValues[0], _baseValues[2]);
            Console.SetCursorPosition(x * 3, y * 3 + 2);
            Console.Write("╚{0:X}╝", _baseValues[3]);
            // ReSharper enable LocalizableElement
        }
    }
}
