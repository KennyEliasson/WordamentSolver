using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace WordamentSolver
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var swedishWords = new List<string>();
            foreach (var line in File.ReadLines("dictionary.txt"))
                swedishWords.Add(line.Split('/')[0].ToUpper());

            var board = new Board(new List<string>
            {
                "SK|A/Å|D|A",
                "X|X|X|X",
                "X|X|X|X",
                "X|X|X|X"
            });

            var trie = new Trie<string>(swedishWords, x => x, StringComparer.Ordinal);
            var results = board.CreateAllCombinations(trie);

            var g = results;
        }
    }

    public class Board
    {
        private readonly BoardLetter[,] _box;

        public Board(List<string> lines)
        {
            // Alla linjer lika långa
            var howManyBoxes = lines[0].Split('|').Length;
            _box = new BoardLetter[lines.Count, howManyBoxes];

            for (var row = 0; row < lines.Count; row++)
            {
                var bricks = lines[row].Split('|');
                for (var column = 0; column < bricks.Length; column++)
                {
                    var boardLetter = new BoardLetter
                    {
                        Column = column,
                        Row = row,
                        Letter = bricks[column]
                    };

                    _box[row, column] = boardLetter;
                }
            }
            
            foreach (var letter in _box)
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

        public List<string> CreateAllCombinations(Trie<string> validWords)
        {
            var results = new HashSet<string>();
            for (var row = 0; row < _box.GetLength(0); row++)
            for (var column = 0; column < _box.GetLength(1); column++)
                _box[row, column].Build(new HashSet<BoardLetter>(), results, validWords);

            return results.Distinct().OrderBy(x => x.Length).ToList();
        }
    }
    
    [DebuggerDisplay("{Letter} at {Row},{Column}")]
    public class BoardLetter
    {
        public string Letter { get; set; }

        public List<BoardLetter> Adjacent { get; set; }
        public bool Visited { get; set; }

        public int Row { get; set; }
        public int Column { get; set; }

        public void Build(HashSet<BoardLetter> visited, HashSet<string> foundWords, Trie<string> validWords, StringBuilder word = null)
        {
            if (word == null)
                word = new StringBuilder();

            foreach (var letter in Letter.Split('/'))
            {
                AppendSingleLetter(letter, visited, foundWords, validWords, new StringBuilder(word.ToString()));
            }            
        }

        private void AppendSingleLetter(string appendLetter, HashSet<BoardLetter> visited, HashSet<string> foundWords, Trie<string> validWords, StringBuilder word)
        {
            word.Append(appendLetter);
            var currentVisited = new HashSet<BoardLetter>(visited) {this};

            var matches = validWords.Retrieve(word.ToString());
            if (matches.Count <= 0)
                return;

            var index = matches.BinarySearch(word.ToString());
            if (index >= 0)
                foundWords.Add(matches[index]);

            foreach (var letter in Adjacent)
            {
                if (visited.Contains(letter))
                    continue;

                letter.Build(currentVisited, foundWords, validWords, word);
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
            if (obj.GetType() != GetType()) return false;
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
}
