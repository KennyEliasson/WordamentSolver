using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace WordamentSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var board = new Board(new List<string>()
            {
                "KENN",
                "JENN",
                "TEST",
                "AWES",
            });

            var trie = new Trie<string>(new List<string>() { "TAWST" }, x => x, StringComparer.Ordinal);
            board.CreateAllCombinations(trie);

            var f = board;
        }
    }

    public class Board
    {
        private readonly char[,] _letters = new char[4, 4];
        private readonly BoardLetter[,] _box = new BoardLetter[0,0];

        public Board(char[,] letters)
        {
            _letters = letters;


            foreach (var letter in _letters)
            {
                Console.WriteLine(letter);
            }
        }

        public Board(List<string> lines)
        {
            // Alla linjer lika långa
            _box = new BoardLetter[lines.Count, lines[0].Length];


            for (int row = 0; row < lines.Count; row++)
            {
                
                for (int column = 0; column < lines[row].Length; column++)
                {
                    var boardLetter = new BoardLetter {
                        Column = column,
                        Row = row,
                        Letter = lines[row][column]
                    };

                    _letters[row, column] = lines[row][column];

                    _box[row, column] = boardLetter;
                }
            }



            foreach (var letter in _box)
            {
                letter.Adjacent = new List<BoardLetter> {
                    GetBoardLetterAtPosition(letter.Column - 1, letter.Row),
                    GetBoardLetterAtPosition(letter.Column + 1, letter.Row),
                    GetBoardLetterAtPosition(letter.Column, letter.Row - 1),
                    GetBoardLetterAtPosition(letter.Column, letter.Row + 1),
                    GetBoardLetterAtPosition(letter.Column - 1, letter.Row - 1),
                    GetBoardLetterAtPosition(letter.Column - 1, letter.Row + 1),
                    GetBoardLetterAtPosition(letter.Column + 1, letter.Row - 1),
                    GetBoardLetterAtPosition(letter.Column + 1, letter.Row + 1)
                }.Where(x => x != null).ToList();
            }

           

        }

        private BoardLetter GetBoardLetterAtPosition(int column, int row )
        {
            if (row < 0)
                return null;

            if (row >= _box.GetLength(0))
                return null;

            if (column < 0)
                return null;

            if (column >= _box.GetLength(1))
                return null;

            return _box[row, column];
        }

        public void CreateAllCombinations(Trie<string> validWords)
        {
            var results = new List<string>();
            _box[2, 0].Build(new HashSet<BoardLetter>(), results, validWords);
            
        }
    }
    
    [DebuggerDisplay("{Letter} at {Row},{Column}")]
    public class BoardLetter
    {
        public char Letter { get; set; }

        public List<BoardLetter> Adjacent { get; set; }
        public bool Visited { get; set; }

        public int Row { get; set; }
        public int Column { get; set; }

        public void Build(HashSet<BoardLetter> visited, List<string> foundWords, Trie<string> validWords, StringBuilder word = null)
        {
            if (word == null)
                word = new StringBuilder();

            word.Append(Letter);
            var currentVisited = new HashSet<BoardLetter>(visited) {this};

            var matches = validWords.Retrieve(word.ToString());
            if (matches.Count <= 0)
                return;

            var index = matches.BinarySearch(word.ToString());
            if (index >= 0)
            {
                foundWords.Add(matches[index]);
            }
            

            foreach (var letter in Adjacent)
            {
                if (visited.Contains(letter))
                    continue;

                var currentWord = new StringBuilder(word.ToString());
                letter.Build(currentVisited, foundWords, validWords, currentWord);
            }
        }

        protected bool Equals(BoardLetter other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BoardLetter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Column;
            }
        }
    }

    public class Trie<TItem>
    {
        #region Constructors

        public Trie(
            IEnumerable<TItem> items,
            Func<TItem, string> keySelector,
            IComparer<TItem> comparer)
        {
            this.KeySelector = keySelector;
            this.Comparer = comparer;
            this.Items = (from item in items
                          from i in Enumerable.Range(1, this.KeySelector(item).Length)
                          let key = this.KeySelector(item).Substring(0, i)
                          group item by key)
                         .ToDictionary(group => group.Key, group => group.ToList());
        }

        #endregion

        #region Properties

        protected Dictionary<string, List<TItem>> Items { get; set; }

        protected Func<TItem, string> KeySelector { get; set; }

        protected IComparer<TItem> Comparer { get; set; }

        #endregion

        #region Methods

        public List<TItem> Retrieve(string prefix)
        {
            return this.Items.ContainsKey(prefix)
                ? this.Items[prefix]
                : new List<TItem>();
        }

        public void Add(TItem item)
        {
            var keys = (from i in Enumerable.Range(1, this.KeySelector(item).Length)
                        let key = this.KeySelector(item).Substring(0, i)
                        select key).ToList();
            keys.ForEach(key =>
            {
                if (!this.Items.ContainsKey(key))
                {
                    this.Items.Add(key, new List<TItem> { item });
                }
                else if (this.Items[key].All(x => this.Comparer.Compare(x, item) != 0))
                {
                    this.Items[key].Add(item);
                }
            });
        }

        public void Remove(TItem item)
        {
            this.Items.Keys.ToList().ForEach(key =>
            {
                if (this.Items[key].Any(x => this.Comparer.Compare(x, item) == 0))
                {
                    this.Items[key].RemoveAll(x => this.Comparer.Compare(x, item) == 0);
                    if (this.Items[key].Count == 0)
                    {
                        this.Items.Remove(key);
                    }
                }
            });
        }

        #endregion
    }

}
